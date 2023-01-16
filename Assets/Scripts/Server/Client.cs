using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    //Sockets & other
    public Socket socket;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
    public IPEndPoint clientep;
    public EndPoint clientRemote;

    //Threads
    Thread connectThread = null;
    Thread receiveThread = null;
    Thread receivePlayerListThread = null;
    bool threadsActive = true;

    //User info
    public uint uuid;
    public string username = "";
    public string serverIP = "";
    public byte[] data;
    public string stringData = "", input = "";

    //Client
    public bool start = false;
    public bool update = false;
    bool connected = false;
    public bool messageReceived = false;
    public bool newServerName = false;
    public bool newServerIP = false;
    bool startGame = false;
    bool endGame = false;
    public float pingTime = 6.0f;
    float pingTimer = 0.0f;

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

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            connectThread = new Thread(Connect);
            threadsActive = true;

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
            if (connected)
            {
                pingTimer += Time.deltaTime;
                if (pingTimer >= pingTime)
                {
                    Debug.Log("Update(): No server connection detected. Leaving...");
                    endGame = true;
                }
            }

            if (messageReceived) //Adds message to the chat
            {
                messageReceived = false;
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

            if (startGame) //Called when start game message is received
            {
                startGame = false;
                manager.UserName = username;
                manager.UserUid = uuid;
                lobby.StartGame();
            }
            else if (endGame) //Called when end game message is received
            {
                endGame = false;
                lobby.LeaveServer();
            }

            if (Input.GetKeyDown(KeyCode.Return)) //Sends message to the server to be processed
            {
                if (chatManager.input.text != "")
                {
                    MemoryStream stream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(true);
                    writer.Write((byte)packetType.chat);
                    writer.Write(uuid);
                    writer.Write(chatManager.input.text);
                    SendInfo(stream);

                    chatManager.input.text = "";
                }
            }
        }
    }

    //Connect to the server
    void Connect()
    {
        if (threadsActive)
        {
            try
            {
                //Hello message & start receiving
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(true);
                writer.Write((byte)packetType.hello);
                writer.Write(username);
                SendInfo(stream);

                receiveThread = new Thread(ReceiveClient);
                try
                {
                    receiveThread.Start();
                }
                catch (ThreadStartException e)
                {
                    Debug.LogError("Start(): Error starting thread: " + e);
                }

            }
            catch (Exception e)
            {
                Debug.LogError("Connect(): Connection failed.. trying again...\n Error: " + e);
            }
        }
    }

    //Receive data from the server
    void ReceiveClient()
    {
        List<Socket> readList = new List<Socket>();
        Debug.Log("ReceiveClient(): Begin to listen...");

        try
        {
            EndPoint remote = (EndPoint)ipep;
            while (threadsActive)
            {
                readList.Clear();
                readList.Add(socket);
                Socket.Select(readList, null, null, 1);
                Thread.Sleep(10);
                if (readList.Count != 0)
                {
                    connected = true;
                    int recv;
                    byte[] tempData = new byte[1024];
                    recv = socket.ReceiveFrom(tempData, ref remote);
                    Debug.Log("ReceiveClient(): New packet received!");

                    byte[] packetData = new byte[recv];
                    Array.Copy(tempData, packetData, recv);

                    MemoryStream stream = new MemoryStream(packetData);
                    BinaryReader reader = new BinaryReader(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    bool isLoopback = reader.ReadBoolean();
                    if (!isLoopback) //To avoid loopback
                    {
                        Debug.Log("ReceiveClient(): No Loopback detected!");
                        short header = reader.ReadByte();
                        packetType type = (packetType)header;

                        switch (type)
                        {
                            case packetType.error:
                                {
                                    Debug.Log("ReceiveClient(): Error packet type received :c");
                                    break;
                                }
                            case packetType.servername:
                                {
                                    newServerName = true;
                                    stringData = reader.ReadString();
                                    Debug.Log("ReceiveClient(): New server name change detected");
                                    startGame = reader.ReadBoolean();
                                    if (startGame)
                                    {
                                        Debug.Log("ReceiveClient(): Game in progress detected");
                                        manager.matchStarted = true;
                                        manager.pScriptsMidGame.Clear();
                                        int count = reader.ReadInt32();
                                        for (int i = 0; i != count; ++i)
                                        {
                                            SendReceive newPlayer = new SendReceive();
                                            newPlayer.uid = reader.ReadUInt32();
                                            newPlayer.position.x = manager.ConvertFromFixed(reader.ReadUInt16(), -130f, 0.01f);
                                            newPlayer.position.y = manager.ConvertFromFixed(reader.ReadUInt16(), -130f, 0.01f);
                                            newPlayer.position.z = manager.ConvertFromFixed(reader.ReadUInt16(), -130f, 0.01f);
                                            manager.pScriptsMidGame.Add(newPlayer);
                                        }
                                    }
                                    Thread.Sleep(100);
                                    break;
                                }
                            case packetType.playerInfo:
                                {
                                    Debug.Log("ReceiveClient(): New game state detected");
                                    if (!manager.win) //Stops the information updates if someone has won
                                    {
                                        manager.data = packetData;
                                        manager.receiveThread = new Thread(manager.ReceiveGameState);
                                        try
                                        {
                                            manager.receiveThread.Start();
                                        }
                                        catch (ThreadStartException e)
                                        {
                                            Debug.LogError("ReceiveClient(): Error starting thread: " + e);
                                        }
                                    }
                                    break;
                                }
                            case packetType.list:
                                {
                                    Debug.Log("ReceiveClient(): New users list detected");
                                    data = packetData;
                                    receivePlayerListThread = new Thread(ReceivePlayerList);
                                    receivePlayerListThread.Start();
                                    Thread.Sleep(100);
                                    break;
                                }
                            case packetType.ping:
                                {
                                    Debug.Log("ReceiveClient(): Ping");
                                    pingTimer = 0.0f;
                                    MemoryStream streamPing = new MemoryStream();
                                    BinaryWriter writer = new BinaryWriter(streamPing);
                                    writer.Write(true);
                                    writer.Write((byte)packetType.ping);
                                    writer.Write(uuid);
                                    SendInfo(streamPing);
                                    break;
                                }
                            default:
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
                                    Debug.Log("ReceiveClient(): Message was: " + stringData);
                                    messageReceived = true;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ReceiveClient(): Loopback detected. Procedure canceled.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ReceiveClient(): Error receiving: " + e);
        }
    }

    //Send gameplay data to the server
    public void SendInfo(MemoryStream stream)
    {
        Debug.Log("SendInfo(): Sending gameplay state...");

        byte[] dataTMP = stream.GetBuffer();
        Debug.Log("SendInfo(): Data Length is: " + dataTMP.Length);

        try
        {
            socket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, ipep);
        }
        catch (Exception e)
        {
            Debug.LogError("SendInfo(): Error receiving: " + e);
        }
    }

    //Reads player list & setup to start game
    public void ReceivePlayerList()
    {
        if (threadsActive)
        {
            Debug.Log("ReceiveList(): Received info");
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            //Header
            reader.ReadBoolean();
            short header = reader.ReadByte();
            packetType type = (packetType)header;
            Debug.Log("ReceiveList(): Header is " + type);

            //List
            lobby.clientList.Clear();
            int count = reader.ReadInt32();
            Debug.Log("ReceiveList(): Count: " + count);
            for (int i = 0; i < count; i++)
            {
                uint uid = reader.ReadUInt32();
                string tmpUsername = reader.ReadString();
                reader.ReadBoolean(); //Dump
                string ipString = reader.ReadString();
                int port = reader.ReadInt32();
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipString), port);
                int spawnIndex = reader.ReadInt32();
                if (tmpUsername == username)
                {
                    uuid = uid;
                }
                Debug.Log("ReceiveList(): Received data: " + uid + " - " + tmpUsername);
                if (lobby.clientList.Exists(user => user.uid == uid))
                {
                    Debug.Log("ReceiveList(): Updating data...");
                    lobby.clientList.Find(user => user.uid == uid).uid = uid;
                    lobby.clientList.Find(user => user.uid == uid).username = tmpUsername;
                    lobby.clientList.Find(user => user.uid == uid).ip = ip;
                    lobby.clientList.Find(user => user.uid == uid).spawnIndex = spawnIndex;
                }
                else
                {
                    Debug.Log("ReceiveList(): Adding data...");
                    lobby.clientList.Add(new PlayerNetInfo(uid, tmpUsername, ip,spawnIndex));
                }
            }
            Debug.Log("ReceiveList(): Done reading list");
            data = null;
        }
    }

    //Close all threads
    public void CloseThreads()
    {
        threadsActive = false;

        try
        {
            if (connectThread != null)
            {
                connectThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error in connect thread: " + e);
        }

        try
        {
            if (receiveThread != null)
            {
                receiveThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error in receive thread: " + e);
        }

        try
        {
            if (receivePlayerListThread != null)
            {
                receivePlayerListThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error in receive list thread: " + e);
        }
    }

    //Close all connections
    public void Leave()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(false);
        writer.Write((byte)packetType.goodbye);
        writer.Write(uuid);
        SendInfo(stream);

        start = false;
        update = false;
        connected = false;
        newServerName = false;
        newServerIP = false;
        messageReceived = false;
        socket = null;
        ipep = new IPEndPoint(IPAddress.Any, 9050);

        if (socket != null)
        {
            socket.Close();
            socket = null;
        }

        CloseThreads();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quitting application...");
        Leave();
    }
}
