using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    //Sockets & other
    public Socket server;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
    public IPEndPoint clientep;
    public EndPoint clientRemote;

    //Threads
    Thread connectThread = null;
    Thread receiveThread = null;
    Thread recievePlayerListThread = null;

    //User info
    public uint uuid;
    public string username;
    public string serverIP;
    public byte[] data;
    public string stringData, input;

    //Client
    public bool start = false;
    public bool update = false;
    bool connected = false;
    public bool messageRecieved = false;
    public bool newServerName = false;
    public bool newServerIP = false;
    bool startGame = false;
    bool endGame = false;

    //Lobby
    public LobbyScripts lobby;
    public Chat chatManager;
    public Canvas chatCanvas;
    public Text clientTitle;

    //Gameplay
    public GameObject gameplayScene;
    public GameplayManager manager;

    void Update()
    {
        if (start)
        {
            if (newServerIP) //Sets inputed IP
            {
                ipep = new IPEndPoint(IPAddress.Parse(serverIP), 9050);
            }

            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            connectThread = new Thread(Connect);

            try
            {
                connectThread.Start();
            }
            catch (ThreadStartException e)
            {
                Debug.LogError("Start(): Error starting thread: " + e);
            }

            start = false;
            update = true;
        }

        if (update)
        {
            if (messageRecieved) //Adds message to the chat
            {
                messageRecieved = false;
                Debug.Log("Update(): CurrentMessage: " + stringData);
                chatManager.SendMsg(stringData);
                stringData = "";
            }
            if (newServerName) //Update the lobby title string
            {
                newServerName = false;
                Debug.Log("Update(): serverName is: " + stringData);
                clientTitle.text = "Welcome to " + stringData + "!";
                chatCanvas.GetComponent<Canvas>().enabled = true;
                Debug.Log("Update(): Changed server title to: " + clientTitle.text);
            }
            if (startGame) //Called when start game message is recieved
            {
                startGame = false;
                manager.UserName = username;
                lobby.StartGame();
            }
            else if (endGame) //Called when end game message is recieved
            {
                endGame = false;
                lobby.LeaveServer();
            }

            if (Input.GetKeyDown(KeyCode.Return)) //Sends message to the server to be processed
            {
                if (chatManager.input.text != "")
                {
                    string msg = "\n" + "/>client/>chat" + uuid + "</" + chatManager.input.text;
                    chatManager.input.text = "";
                    Send(msg);
                }
            }
        }
    }

    //Connect to the server
    void Connect()
    {
        try
        {
            //Hello message & start receiving
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(true);
            writer.Write((short)packetType.hello);
            writer.Write(username);

            SendInfo(stream);

            connected = true;
            receiveThread = new Thread(Receive);
            try
            {
                receiveThread.Start();
            }
            catch (ThreadStartException e)
            {
                Debug.LogError("Start(): Error starting thread: " + e);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError("Connect(): Connection failed.. trying again...\n Error: " + e);
        }
    }

    //Receive data from the server
    void Receive()
    {
        try
        {
            EndPoint Remote = (EndPoint)ipep;
            while (true)
            {
                if (connected)
                {
                    int recv;
                    byte[] dataTMP = new byte[1024];

                    recv = server.ReceiveFrom(dataTMP, ref Remote);

                    MemoryStream stream = new MemoryStream(dataTMP);
                    BinaryReader reader = new BinaryReader(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    bool isLoopback = reader.ReadBoolean();
                    Debug.Log("Recieve(): New message detected in client side!");
                    if (!isLoopback) //To avoid loopback
                    {
                        short header = reader.ReadInt16();
                        packetType type = (packetType)header;
                        if (type == packetType.error)
                        {
                            Debug.Log("Recieve(): Data was empty :c");
                        }
                        else if (type == packetType.servername) //Update server name
                        {
                            newServerName = true;
                            stringData = reader.ReadString();
                            Debug.Log("Recieve(): New server name change detected");
                            Thread.Sleep(200);
                        }
                        else if (type == packetType.playerInfo) //Gameplay data
                        {
                            Debug.Log("Recieve(): New game state detected");
                            manager.data = dataTMP;
                            manager.recieveThread = new Thread(manager.RecieveGameState);
                            try
                            {
                                manager.recieveThread.Start();
                            }
                            catch (ThreadStartException e)
                            {
                                Debug.LogError("Start(): Error starting thread: " + e);
                            }
                        }
                        else if (type == packetType.list) //Player List to sync server & client to start game
                        {
                            Debug.Log("Recieve(): New users list detected");
                            data = dataTMP;
                            recievePlayerListThread = new Thread(RecievePlayerList);
                            recievePlayerListThread.Start();
                            Thread.Sleep(200);
                        }
                        else
                        {
                            //Start/End game
                            if (type == packetType.startGame)
                            {
                                startGame = true;
                            }
                            else if (type == packetType.endSession)
                            {
                                endGame = true;
                            }
                            //Add message to the chat
                            stringData = reader.ReadString();
                            messageRecieved = true;
                            Debug.Log("Recieve(): No new server name changes detected");
                        }
                        Thread.Sleep(1);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Recieve(): Error receiving: " + e);
        }
    }

    //Send data to the server
    public void Send(string m)
    {
        Debug.Log("Send(): Sending message..." + m);
        byte[] dataTMP = Encoding.ASCII.GetBytes(m);
        try
        {
            server.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, ipep);
        }
        catch (Exception e)
        {
            Debug.LogError("Send(): Error receiving: " + e);
        }
    }

    //Send gameplay data to the server
    public void SendInfo(MemoryStream stream)
    {
        Debug.Log("SendInfo(): Sending gameplay state...");

        byte[] dataTMP = new byte[1024];
        dataTMP = stream.GetBuffer();

        Debug.Log("SendInfo(): Data Length is: " + stream.Length);

        try
        {
            server.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, ipep);
        }
        catch (Exception e)
        {
            Debug.LogError("SendInfo(): Error receiving: " + e);
        }
    }

    //Reads player list & setup to start game
    public void RecievePlayerList()
    {
        Debug.Log("RecieveList(): Recieved info");
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        reader.ReadBoolean();
        short header = reader.ReadInt16();
        packetType type = (packetType)header;
        Debug.Log("RecieveList(): Header is " + type);

        //List
        lobby.usersList.Clear();
        int count = reader.ReadInt32();
        Debug.Log("RecieveList(): Count: " + count);
        for (int i = 0; i < count; i++)
        {
            string tmp = reader.ReadString();
            uint uid = reader.ReadUInt32();
            if (tmp == username)
            {
                uuid = uid;
            }
            Debug.Log("RecieveList(): Recieved data: " + uid + " - " + tmp);
            if (lobby.usersList.ContainsKey(uid))
            {
                Debug.Log("RecieveList(): Updating data");
                lobby.usersList[uid] = tmp;
            }
            else
            {
                Debug.Log("RecieveList(): Adding data");
                lobby.usersList.Add(uid, tmp);
            }
        }
        data = null;
        Thread.Sleep(1);
    }

    //Close all threads
    public void CloseThreads()
    {
        try
        {
            if (connectThread != null)
            {
                connectThread.Abort();
                connectThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error leaving server: " + e);
        }

        try
        {
            if (receiveThread != null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error leaving server: " + e);
        }

        try
        {
            if (recievePlayerListThread != null)
            {
                recievePlayerListThread.Abort();
                recievePlayerListThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error leaving server: " + e);
        }
    }

    //Close all connections
    public void Leave()
    {
        start = false;
        update = false;
        connected = false;
        newServerName = false;
        newServerIP = false;
        messageRecieved = false;
        server = null;
        ipep = new IPEndPoint(IPAddress.Any, 9050);

        if (server != null)
        {
            server.Close();
            server = null;
        }

        CloseThreads();
    }

    private void OnApplicationQuit()
    {
        if (server != null)
        {
            server.Close();
            server = null;
        }
    }
}
