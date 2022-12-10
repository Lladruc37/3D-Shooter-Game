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
    //To keep track of accepted users
    public List<IPEndPoint> clientListUDP = new List<IPEndPoint>();

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

            recieveDataThread = new Thread(RecieveData);
            try
            {
                recieveDataThread.Start();
            }
            catch (ThreadStartException e)
            {
                Debug.LogError("Start(): Error starting thread: " + e);
            }

            //Adds this user to the list of players
            AddPlayer(hostUsername);
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
                    writer.Write((short)packetType.chat);
                    string resultingMessage = "\n[" + lobby.usersList[uid] + "]>>" + chatManager.input.text;
                    chatManager.SendMsg(resultingMessage);
                    writer.Write(resultingMessage);

                    chatManager.input.text = "";

                    BroadcastServerInfo(stream);
                }
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
        writer.Write((short)packetType.list);
        writer.Write(lobby.usersList.Count);

        //List
        int i = 0;
        foreach (KeyValuePair<uint, string> user in lobby.usersList)
        {
            Debug.Log("SendList(): Values: " + user.Key + " - " + user.Value);
            writer.Write(user.Value);
            writer.Write(user.Key);
            i++;
        }

        Debug.Log("SendList(): Sending list...");
        BroadcastServerInfo(stream);

        Thread.Sleep(1);
    }

    //Adds a player to the list & binds them with a uid
    void AddPlayer(string name)
    {
        lobby.usersList.Add(maxUid, name);
        maxUid++;
    }

    //Adds a user to the list of listening users (to send messages)
    void AddClientUDP(IPEndPoint newClient)
    {
        if (newClient.ToString() != "")
        {
            clientListUDP.Add(newClient);
            Debug.Log("AddClientUDP(): Connected to: " + newClient.ToString());
        }
        else
        {
            Debug.LogError("AddClientUDP(): No clients connected. Waiting to accept...");
        }
    }

    //Sends the player list to the list of listening users
    public void BroadcastServerInfo(MemoryStream stream)
    {
        byte[] dataTMP = new byte[1024];
        dataTMP = stream.GetBuffer();

        foreach (IPEndPoint ip in clientListUDP)
        {
            EndPoint remote = (EndPoint)ip;
            socket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
        }
    }

    //Sends the gameplay data to the list of listening users
    public void BroadcastPlayerInfo(byte[] data)
    {
        Debug.Log("BroadcastPlayerInfo(): Sending gameplay state from player...");

        foreach (IPEndPoint ip in clientListUDP)
        {
            EndPoint remote = (EndPoint)ip;
            socket.SendTo(data, data.Length, SocketFlags.None, remote);
            Debug.Log("BroadcastPlayerInfo(): Message sent successfully");
        }
    }

    //Receives data from users
    void RecieveData()
    {
        try
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 9050);
                EndPoint remote = (EndPoint)(sender);
                int recv;

                byte[] tempData = new byte[1024];
                Debug.Log("RecieveData(): Begin to listen...");
                recv = socket.ReceiveFrom(tempData, ref remote);
                Debug.Log("RecieveData(): New packet recieved!");

                byte[] packetData = new byte[recv];
                Array.Copy(tempData, packetData, recv);

                Debug.Log("RecieveData(): Count for recv: " + recv);
                Debug.Log("RecieveData(): Length of Data: " + packetData.Length);

                MemoryStream stream = new MemoryStream(packetData);
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(0, SeekOrigin.Begin);
                reader.ReadBoolean();
                short header = reader.ReadInt16();
                packetType type = (packetType)header;

                switch (type)
                {
                    case packetType.error:
                        {
                            Debug.LogError("Recieve(): Error packet type received :c");
                            break;
                        }
                    case packetType.hello:
                        {
                            Debug.Log("RecieveData(): New client detected!");
                            sender = (IPEndPoint)remote;
                            AddClientUDP(sender);

                            string userName = reader.ReadString();
                            AddPlayer(userName);

                            stringData = "\nUser '" + userName + "' joined the lobby!";
                            newMessage = true;

                            string tmp = stringData;
                            Debug.Log("RecieveData(): " + stringData);

                            MemoryStream streamHello = new MemoryStream();
                            BinaryWriter writerHello = new BinaryWriter(streamHello);
                            writerHello.Write(false);
                            writerHello.Write((short)packetType.servername);
                            writerHello.Write(serverName);

                            MemoryStream streamChat = new MemoryStream();
                            BinaryWriter writerChat = new BinaryWriter(streamChat);
                            writerChat.Write(false);
                            writerChat.Write((short)packetType.chat);
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
                            uint tmpUid = reader.ReadUInt32();

                            MemoryStream streamGoodbye = new MemoryStream();
                            BinaryWriter writerGoodbye = new BinaryWriter(streamGoodbye);
                            writerGoodbye.Write(false);
                            writerGoodbye.Write((short)packetType.chat);

                            stringData = "\nUser '" + lobby.usersList[tmpUid] + "' has left the server!";
                            newMessage = true;
                            writerGoodbye.Write(stringData);

                            lobby.usersList.Remove(tmpUid);
                            BroadcastServerInfo(streamGoodbye);
                            Thread.Sleep(100);
                            sender = (IPEndPoint)remote;
                            clientListUDP.Remove(sender);
                            SendPlayerList();
                            break;
                        }
                    case packetType.list:
                        {
                            Debug.Log("RecieveData(): New game state detected");
                            manager.data = packetData;
                            manager.recieveThread = new Thread(manager.RecieveGameState);
                            manager.recieveThread.Start();
                            BroadcastPlayerInfo(packetData);
                            break;
                        }
                    case packetType.chat:
                        {
                            Debug.Log("RecieveData(): New chat message from user!");
                            uint uid = reader.ReadUInt32();
                            string m = reader.ReadString();

                            MemoryStream streamChat = new MemoryStream();
                            BinaryWriter writerChat = new BinaryWriter(streamChat);
                            writerChat.Write(false);
                            writerChat.Write((short)packetType.chat);
                            if (lobby.usersList.ContainsKey(uid))
                            {
                                string username = lobby.usersList[uid];
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
                            Debug.Log("RecieveData(): New game state detected");
                            manager.data = packetData;
                            manager.recieveThread = new Thread(manager.RecieveGameState);
                            manager.recieveThread.Start();
                            BroadcastPlayerInfo(packetData);
                            break;
                        }
                    default:
                        {
                            Debug.LogError("Recieve(): Message was: " + stringData);
                            break;
                        }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("RecieveData(): Error receiving: " + e);
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
