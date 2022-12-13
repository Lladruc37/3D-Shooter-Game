using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Server : MonoBehaviour
{
    //Sockets & other
    public Socket socket;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
    public IPEndPoint clientep;

    //Threads
    Thread recieveDataThread = null;

    //User info
    public uint uid = 0;
    public string hostUsername = "";
    string stringData = null;

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
    float pingTme = 1.5f;
    float pingTimer;
    List<uint> pingList = new List<uint>();

    //Gameplay
    public GameObject gameplayScene;
    public GameplayManager manager;

    void Update()
    {
        if (start) //Starts listening for client data
        {
            start = false;
            update = true;
            uid = maxUid;
            Debug.Log("Server(): Starting server...");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ipep);

            recieveDataThread = new Thread(RecieveServer);
            try
            {
                recieveDataThread.Start();
            }
            catch (ThreadStartException e)
            {
                Debug.LogError("Start(): Error starting thread: " + e);
            }

            //Adds this user to the list of players
            AddClient(hostUsername, ipep);
            Debug.Log("Server(): Server started successfully!");
        }

        if (update)
        {
            if (newMessage) //Adds message to the chat
            {
                newMessage = false;
                chatManager.SendMsg(stringData);
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
    public void Ping()
    {
        pingTimer += Time.deltaTime;
        if (pingTimer >= pingTme) //Ping the players
        {
            pingTimer = 0.0f;
            pingList.Add(uid);
            foreach (PlayerNetInfo user in lobby.clientList.ToArray())
            {
                if (!pingList.Contains(user.uid))
                {
                    Debug.Log("Ping(): No ping from " + user.uid + ": " + user.username);
                    GoodbyeUser(user.uid);
                }
            }
            pingList.Clear();

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(false);
            writer.Write((byte)packetType.ping);

            BroadcastServerInfo(stream);
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
            Debug.Log("SendList(): Values: " + user.uid + " - " + user.username + " - " + user.ip);
            writer.Write(user.uid);
            writer.Write(user.username);
            writer.Write(true);
            writer.Write(user.ip.Address.ToString());
            writer.Write(user.ip.Port);
            i++;
        }

        Debug.Log("SendList(): Sending list...");

        BroadcastServerInfo(stream);
    }

    PlayerNetInfo AddClient(string username, IPEndPoint ip)
    {
        PlayerNetInfo newPlayer = null;
        if(ip.ToString() != "")
        {
            newPlayer = new PlayerNetInfo(maxUid, username, ip);
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
    void RecieveServer()
    {
        try
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 9050);
                EndPoint remote = (EndPoint)(sender);
                int recv;

                byte[] tempData = new byte[1024];
                Debug.Log("RecieveServer(): Begin to listen...");
                recv = socket.ReceiveFrom(tempData, ref remote);
                Debug.Log("RecieveServer(): New packet recieved!");

                byte[] packetData = new byte[recv];
                Array.Copy(tempData, packetData, recv);

                Debug.Log("RecieveServer(): Count for recv: " + recv);
                Debug.Log("RecieveServer(): Length of Data: " + packetData.Length);

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
                            Debug.LogError("RecieveServer(): Error packet type received :c");
                            break;
                        }
                    case packetType.hello:
                        {
                            Debug.Log("RecieveServer(): New client detected!");
                            sender = (IPEndPoint)remote;
                            string userName = reader.ReadString();

                            PlayerNetInfo newPlayer = AddClient(userName, sender);
                            pingList.Add(newPlayer.uid);

                            stringData = "\nUser '" + userName + "' joined the lobby!";
                            newMessage = true;

                            string tmp = stringData;
                            Debug.Log("RecieveServer(): " + stringData);

                            MemoryStream streamHello = new MemoryStream();
                            BinaryWriter writerHello = new BinaryWriter(streamHello);
                            writerHello.Write(false);
                            writerHello.Write((byte)packetType.servername);
                            writerHello.Write(serverName);

                            MemoryStream streamChat = new MemoryStream();
                            BinaryWriter writerChat = new BinaryWriter(streamChat);
                            writerChat.Write(false);
                            writerChat.Write((byte)packetType.chat);
                            writerChat.Write(stringData);

                            BroadcastServerInfo(streamHello);
                            Thread.Sleep(100);
                            SendPlayerList();
                            Thread.Sleep(100);
                            BroadcastServerInfo(streamChat);
                            break;
                        }
                    case packetType.goodbye:
                        {
                            GoodbyeUser(reader.ReadUInt32());
                            break;
                        }
                    case packetType.list:
                        {
                            Debug.Log("RecieveServer(): New game state detected");
                            manager.data = packetData;
                            manager.recieveThread = new Thread(manager.RecieveGameState);
                            manager.recieveThread.Start();
                            BroadcastServerInfo(packetData);
                            break;
                        }
                    case packetType.chat:
                        {
                            Debug.Log("RecieveServer(): New chat message from user!");
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
                            Debug.Log("RecieveServer(): New game state detected");
                            manager.data = packetData;
                            manager.recieveThread = new Thread(manager.RecieveGameState);
                            manager.recieveThread.Start();
                            BroadcastServerInfo(packetData);
                            break;
                        }
                    case packetType.ping:
                        {
                            Debug.Log("RecieveServer(): Ping");
                            uint userUid = reader.ReadUInt32();
                            if (!pingList.Contains(userUid))
                            {
                                Debug.Log("RecieveServer(): New ping from " + userUid);
                                pingList.Add(userUid);
                            }
                            break;
                        }
                    default:
                        {
                            Debug.LogError("RecieveServer(): Message was: " + stringData);
                            break;
                        }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("RecieveServer(): Error receiving: " + e);
        }
    }

    //Close all connections
    public void Close()
    {
        start = false;
        update = false;

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
            if (recieveDataThread != null)
            {
                recieveDataThread.Abort();
                recieveDataThread = null;
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogError("CloseThreads(): Error closing server: " + e);
        }
    }

    private void OnApplicationQuit()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
        CloseThreads();
    }
}
