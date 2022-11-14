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
    public Socket server;
    public Socket client;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
    public IPEndPoint clientep;
    public IPAddress adress = IPAddress.Any;
    public EndPoint clientRemote;

    public string stringData, input;
    bool connected = false;
    Thread connectThread = null;
    Thread receiveThread = null;
    Thread recievePlayerListThread = null;

    public uint uuid;
    public string username;
    public string serverIP;
    public byte[] data;

    public bool start = false;
    bool startGame = false;
    bool endGame = false;
    public bool update = false;
    public Chat chatManager;
    public bool messageRecieved = false;
    public bool newServerName = false;
    public bool newServerIP = false;

    public Text clientTitle;
    public Canvas chatCanvas;
    public GameObject gameplayScene;
    public GameplayManager manager;
    public LobbyScripts lobby;

    // Start is called before the first frame update
    void Start()
    { }

    void Update()
    {
        if (start)
        {
            if (newServerIP)
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
            if (messageRecieved)
            {
                messageRecieved = false;
                Debug.Log("Update(): CurrentMessage: " + stringData);
                chatManager.SendMsg(stringData);
                stringData = "";
            }
            if (newServerName)
            {
                newServerName = false;
                string tmp = stringData.Remove(0, 14);
                clientTitle.text = "Welcome to " + tmp + "!";
                chatCanvas.GetComponent<Canvas>().enabled = true;
                Debug.Log("Update(): Changed server title to: " + clientTitle.text);
            }
            if (startGame)
            {
                startGame = false;
                manager.UserName = username;
                lobby.StartGame();
            }
            else if (endGame)
            {
                endGame = false;
                lobby.LeaveServer();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (chatManager.input.text != "")
                {
                    string msg = "\n" + "/>client/>uuid" + uuid + "</" + chatManager.input.text;
                    chatManager.input.text = "";
                    Send(msg);
                }
            }
        }
    }

    void Connect()
    {
        try
        {
            Send("/>client/>hello</" + username);
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
                    Debug.Log("Recieve(): New message detected in client side!");
                    stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
                    if (!stringData.Contains("/>client"))
                    {
                        Debug.Log("Recieve(): Message was: " + stringData);
                        if (stringData.Equals(""))
                        {
                            Debug.Log("Recieve(): Data was empty :c");
                        }
                        else
                        {
                            if (stringData.Contains("/>servername"))
                            {
                                newServerName = true;
                                Debug.Log("Recieve(): New server name change detected");
                            }
                            else if (stringData.Contains("/>PlayerInfo:"))
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
                            else if (stringData.Contains("/>list</"))
                            {
                                Debug.Log("Recieve(): New users list detected");
                                data = dataTMP;
                                recievePlayerListThread = new Thread(RecievePlayerList);
                                recievePlayerListThread.Start();
                            }
                            else
                            {
                                if (stringData == "\nStarting game...")
                                {
                                    startGame = true;
                                }
                                else if (stringData == "\nEnding session...")
                                {
                                    endGame = true;
                                }

                                messageRecieved = true;
                                Debug.Log("Recieve(): No new server name changes detected");
                            }
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Recieve(): Error receiving: " + e);
        }
    }
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

    public void RecievePlayerList()
    {
        Debug.Log("RecieveList(): Recieved info");
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        string header = reader.ReadString();
        Debug.Log("RecieveList(): Header is " + header);

        //List
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

        //TODO: Temporary solution
        data = null;
        Thread.Sleep(100);
    }
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
    public void Leave()
    {
        start = false;
        update = false;
        connected = false;

        if (server != null)
        {
            server.Close();
            server = null;
        }

        CloseThreads();
    }
    // Update is called once per frame
    private void OnApplicationQuit()
    {
        if (server != null)
        {
            server.Close();
            server = null;
        }
    }
}
