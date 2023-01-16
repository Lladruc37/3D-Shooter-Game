using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Server : MonoBehaviour
{
    //Sockets & other
    public Socket socket;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
    public IPEndPoint clientep;

    //Threads
    Thread receiveDataThread = null;
    bool threadsActive = true;

    //User info
    public uint uid = 0;
    public string hostUsername = "";
    public string stringData = null;

    //Server
    public string serverName = "Server";
    public uint maxUid = 0;
    public bool start = false;
    bool update = false;
    public bool newMessage = false;

    //Lobby
    public LobbyScripts lobby;
    public Chat chatManager;

    //Pings
    float pingTime = 3.0f;
    float pingTimer = 0.0f;
    List<uint> pingList = new List<uint>();

    //Gameplay
    public GameObject gameplayScene;
    public GameplayManager manager;
    public List<int> blacklistedSpawns = new List<int>();

    void Update()
    {
        if (start) //Starts listening for client data
        {
            start = false;
            update = true;
            uid = maxUid;
            Debug.Log("Start(): Starting server...");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ipep);

            receiveDataThread = new Thread(ReceiveServer);
            threadsActive = true;
            try
            {
                receiveDataThread.Start();
            }
            catch (ThreadStartException e)
            {
                Debug.LogError("Start(): Error starting thread: " + e);
            }

            //Adds this user to the list of players
            int spawnIndex = RandomizeSpawnIndex();
            AddClient(hostUsername, ipep, spawnIndex);
            Debug.Log("Start(): Server started successfully!");
        }

        if (update)
        {
            if (newMessage) //Adds message to the chat
            {
                newMessage = false;
                chatManager.SendMsg(stringData);
                stringData = "";
            }
            if (Input.GetKeyDown(KeyCode.Return)) //Sends message to all the clients
            {
                if (chatManager.input.text != "")
                {
                    MemoryStream stream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(false);
                    writer.Write((byte)packetType.chat);
                    string resultingMessage = "\n[" + hostUsername + "]>>" + chatManager.input.text;
                    chatManager.SendMsg(resultingMessage);
                    writer.Write(resultingMessage);

                    chatManager.input.text = "";

                    BroadcastServerInfo(stream);
                }
            }
            Ping();
        }
    }

    //Ping the players
    public void Ping()
    {
        pingTimer += Time.deltaTime;
        if (pingTimer >= pingTime)
        {
            pingTimer = 0.0f;
            pingList.Add(uid);
            List<PlayerNetInfo> currentList = lobby.clientList;
            foreach (PlayerNetInfo user in currentList)
            {
                if (!pingList.Contains(user.uid))
                {
                    Debug.Log("Ping(): No ping from " + user.uid + ": " + user.username);
                    GoodbyeUser(user.uid);
                    if (receiveDataThread != null)
                    {
                        threadsActive = false;
                        while (!threadsActive)
						{
                            if (!receiveDataThread.IsAlive)
							{
                                threadsActive = true;
                                receiveDataThread = null;
                                receiveDataThread = new Thread(ReceiveServer);
                            }
                        }
                        try
                        {
                            receiveDataThread.Start();
                        }
                        catch (ThreadStartException e)
                        {
                            Debug.LogError("Ping(): Error starting thread: " + e);
                        }
                    }
                }
            }
            pingList.Clear();

            if (threadsActive)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(false);
                writer.Write((byte)packetType.ping);

                BroadcastServerInfo(stream);
            }
        }
    }

    //Sends player list to all users
    public void SendPlayerList()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write(false);
        writer.Write((byte)packetType.list);
        writer.Write(lobby.clientList.Count);

        //List
        int i = 0;
        foreach (PlayerNetInfo user in lobby.clientList)
        {
            Debug.Log("SendPlayerList(): Values: " + user.uid + " - " + user.username + " - " + user.ip + " - " + user.spawnIndex);
            writer.Write(user.uid);
            writer.Write(user.username);
            writer.Write(true);
            writer.Write(user.ip.Address.ToString());
            writer.Write(user.ip.Port);
            writer.Write(user.spawnIndex);
            i++;
        }

        Debug.Log("SendPlayerList(): Sending list...");

        BroadcastServerInfo(stream);
    }

    //Adds a new client to the list of connected clients
    PlayerNetInfo AddClient(string username, IPEndPoint ip, int spawnIndex)
    {
        PlayerNetInfo newPlayer = null;
        if(ip.ToString() != "")
        {
            newPlayer = new PlayerNetInfo(maxUid, username, ip, spawnIndex);
            lobby.clientList.Add(newPlayer);
            maxUid++;
            Debug.Log("AddClient(): Connected to: " + ip.ToString());
        }
        else
        {
            Debug.LogError("AddClient(): No clients connected. Waiting to accept...");
        }
        return newPlayer;
    }

    //Sends the player list to the list of listening users
    public void BroadcastServerInfo(MemoryStream stream)
    {
        byte[] dataTMP = stream.GetBuffer();

        foreach (PlayerNetInfo info in lobby.clientList)
        {
            if (info.ip.Address.ToString() != "0.0.0.0")
            {
                EndPoint remote = (EndPoint)info.ip;
                socket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
            }
        }
    }

    //Sends the gameplay data to the list of listening users
    public void BroadcastServerInfo(byte[] data)
    {
        Debug.Log("BroadcastPlayerInfo(): Sending gameplay state from player...");

        foreach (PlayerNetInfo info in lobby.clientList)
        {
            if (info.ip.Address.ToString() != "0.0.0.0")
            {
                EndPoint remote = (EndPoint)info.ip;
                socket.SendTo(data, data.Length, SocketFlags.None, remote);
                Debug.Log("BroadcastPlayerInfo(): Message sent successfully");
            }
        }
    }

    //Eliminates a user from the list and removes the connection
    void GoodbyeUser(uint uid)
	{
        MemoryStream streamGoodbye = new MemoryStream();
        BinaryWriter writerGoodbye = new BinaryWriter(streamGoodbye);
        writerGoodbye.Write(false);
        writerGoodbye.Write((byte)packetType.chat);

        PlayerNetInfo user = lobby.clientList.Find(user => user.uid == uid);

        stringData = "\nUser '" + user.username + "' has left the server!";
        newMessage = true;
        writerGoodbye.Write(stringData);

        lobby.clientList.Remove(user);

        BroadcastServerInfo(streamGoodbye);
        Thread.Sleep(100);
        SendPlayerList();
    }

    //Receives data from users
    void ReceiveServer()
    {
        List<Socket> readList = new List<Socket>();
        Debug.Log("ReceiveServer(): Begin to listen...");

        try
        {
			while (threadsActive)
            {
                readList.Clear();
                readList.Add(socket);
                Socket.Select(readList, null, null, 1);
                Thread.Sleep(10);
                if (readList.Count != 0)
                {
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 9050);
                    EndPoint remote = (EndPoint)(sender);
                    int recv;

                    byte[] tempData = new byte[1024];
                    recv = socket.ReceiveFrom(tempData, ref remote);
                    Debug.Log("ReceiveServer(): New packet received!");

                    byte[] packetData = new byte[recv];
                    Array.Copy(tempData, packetData, recv);

                    Debug.Log("ReceiveServer(): Count for recv: " + recv);
                    Debug.Log("ReceiveServer(): Length of Data: " + packetData.Length);

                    MemoryStream stream = new MemoryStream(packetData);
                    BinaryReader reader = new BinaryReader(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    reader.ReadBoolean();
                    short header = reader.ReadByte();
                    packetType type = (packetType)header;

                    switch (type)
                    {
                        case packetType.error:
                            {
                                Debug.LogError("ReceiveServer(): Error packet type received :c");
                                break;
                            }
                        case packetType.hello:
                            {
                                Debug.Log("ReceiveServer(): New client detected!");
                                sender = (IPEndPoint)remote;
                                string userName = reader.ReadString();

                                int spawnIndex = RandomizeSpawnIndex();

                                PlayerNetInfo newPlayer = AddClient(userName, sender, spawnIndex);
                                pingList.Add(newPlayer.uid);

                                stringData = "\nUser '" + userName + "' joined the lobby!";
                                newMessage = true;

                                string tmp = stringData;
                                Debug.Log("ReceiveServer(): " + stringData);

                                MemoryStream streamHello = new MemoryStream();
                                BinaryWriter writerHello = new BinaryWriter(streamHello);
                                writerHello.Write(false);
                                writerHello.Write((byte)packetType.servername);
                                writerHello.Write(serverName);
                                writerHello.Write(manager.update);
                                if (manager.update)
                                {
                                    writerHello.Write(manager.pScripts.Count);
                                    foreach(SendReceive sr in manager.pScripts)
                                    {
                                        writerHello.Write(sr.uid);
                                        ushort x = manager.ConvertToFixed(sr.position.x, -130f, 0.01f), y = manager.ConvertToFixed(sr.position.y, -130f, 0.01f), z = manager.ConvertToFixed(sr.position.z, -130f, 0.01f);
                                        writerHello.Write(x);
                                        writerHello.Write(y);
                                        writerHello.Write(z);
                                    }
                                }

                                MemoryStream streamChat = new MemoryStream();
                                BinaryWriter writerChat = new BinaryWriter(streamChat);
                                writerChat.Write(false);
                                writerChat.Write((byte)packetType.chat);
                                writerChat.Write(stringData);

                                SendPlayerList();
                                Thread.Sleep(100);
                                byte[] dataTMP = streamHello.GetBuffer();
                                EndPoint playerEP = (EndPoint)newPlayer.ip;
                                socket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, playerEP);
                                Thread.Sleep(100);
                                BroadcastServerInfo(streamChat);
                                break;
                            }
                        case packetType.goodbye:
                            {
                                uint uid = reader.ReadUInt32();
                                Debug.Log("ReceiveServer(): Bye user: " + uid);
                                GoodbyeUser(uid);
                                break;
                            }
                        case packetType.list:
                            {
                                Debug.Log("ReceiveServer(): New game state detected");
                                manager.data = packetData;
                                manager.receiveThread = new Thread(manager.ReceiveGameState);
                                manager.receiveThread.Start();
                                BroadcastServerInfo(packetData);
                                break;
                            }
                        case packetType.chat:
                            {
                                Debug.Log("ReceiveServer(): New chat message from user!");
                                uint uid = reader.ReadUInt32();
                                string m = reader.ReadString();

                                MemoryStream streamChat = new MemoryStream();
                                BinaryWriter writerChat = new BinaryWriter(streamChat);
                                writerChat.Write(false);
                                writerChat.Write((byte)packetType.chat);
                                if (lobby.clientList.Exists(user => user.uid == uid))
                                {
                                    string username = lobby.clientList.Find(user => user.uid == uid).username;
                                    string resultingMessage = "\n[" + username + "]>>" + m;
                                    stringData = resultingMessage;
                                    writerChat.Write(resultingMessage);
                                }
                                else
                                {
                                    string resultingMessage = "Error Message: Something wrong happened!";
                                    stringData = resultingMessage;
                                    writerChat.Write(resultingMessage);
                                }
                                newMessage = true;
                                BroadcastServerInfo(streamChat);
                                break;
                            }
                        case packetType.playerInfo:
                            {
                                Debug.Log("ReceiveServer(): New game state detected");
                                if (!manager.win)
                                {
                                    manager.data = packetData;
                                    manager.receiveThread = new Thread(manager.ReceiveGameState);
                                    manager.receiveThread.Start();
                                    BroadcastServerInfo(packetData);
                                }
                                break;
                            }
                        case packetType.ping:
                            {
                                Debug.Log("ReceiveServer(): Ping");
                                uint userUid = reader.ReadUInt32();
                                if (!pingList.Contains(userUid))
                                {
                                    Debug.Log("ReceiveServer(): New ping from " + userUid);
                                    pingList.Add(userUid);
                                }
                                break;
                            }
                        default:
                            {
                                Debug.LogError("ReceiveServer(): Message was: " + stringData);
                                break;
                            }
                    }
                }
            }
		}
		catch (Exception e)
		{
			Debug.LogError("ReceiveServer(): Error receiving: " + e);
		}
	}

	public int RandomizeSpawnIndex()
	{
        var rand = new System.Random();
        int randomSpawnIndex = rand.Next(0, 15);
        if(blacklistedSpawns.Count == manager.spawnpoints.Count) //in case it is going to perma loop
		{
            blacklistedSpawns.Clear();
		}
        while (blacklistedSpawns.Contains(randomSpawnIndex))
        {
            randomSpawnIndex = rand.Next(0, 15);
        }

        blacklistedSpawns.Add(randomSpawnIndex);
        return randomSpawnIndex;
    }

    //Close all connections
    public void Close()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(false);
        writer.Write((byte)packetType.endSession);
        string msg = "\nEnding session...";
        writer.Write(msg);
        chatManager.SendMsg(msg);
        BroadcastServerInfo(stream);

        start = false;
        update = false;
        pingList.Clear();

        if (socket != null)
        {
            socket.Close();
            socket = null;
        }

        CloseThreads();
    }

    //Close all threads
    private void CloseThreads()
    {
        try
        {
            threadsActive = false;
            if (receiveDataThread != null)
            {
                receiveDataThread = null;
            }
        }
        catch (ThreadStateException e)
        {
            Debug.LogError("CloseThreads(): Error closing server: " + e);
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quitting application...");
        Close();
    }
}
