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
                    string msg = "\n" + "/>client/>uuid" + uid + "</" + chatManager.input.text;
                    chatManager.input.text = "";
                    BroadcastServerMessage(ManageMessage(msg));
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
        writer.Write("/>list</");
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

    //Process message
    public string ManageMessage(string m, bool isServer = false, bool isServernameMessage = false)
    {
        string result = "";
        string[] splitName;
        if (isServer) //Will show as a server message/notification
        {
            if (m.Contains("/>startgame"))
            {
                string tmp = m;
                splitName = tmp.Split("</");
                m = splitName[1];
            }
            else if (m.Contains("/>endsession"))
            {
                string tmp = m;
                splitName = tmp.Split("</");
                m = splitName[1];
            }
            result = "\n" + m;
        }
        else //Will show as a user message
        {
            if (m.Contains("/>client/>uuid")) //process user uid to get username
            {
                string tmp = m.Remove(0, 15);
                splitName = tmp.Split("</");
                m = splitName[1];
                uint uid = uint.Parse(splitName[0]);
                string username = "";
                if (lobby.usersList.ContainsKey(uid))
                {
                    username = lobby.usersList[uid];
                    result = "\n[" + username + "]>>" + m;
                }
                else
                {
                    Debug.LogError("ManageMessage(): No username with UID: " + uid);
                    result = "";
                }
            }
            else
            {
                Debug.LogError("ManageMessage(): Error: No username detected");
            }
        }
        Debug.Log("ManageMessage(): Sending message: " + result);
        if (!isServernameMessage) //send it to this user also
        {
            stringData = result;
            newMessage = true;
        }
        return result;
    }

    //Sends the processed message to the list of listening users
    public void BroadcastServerMessage(string m)
    {
        byte[] dataTMP = new byte[1024];
        dataTMP = Encoding.ASCII.GetBytes(m);

        foreach (IPEndPoint ip in clientListUDP)
        {
            EndPoint remote = (EndPoint)ip;
            socket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
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

                byte[] dataTMP = new byte[1024];
                Debug.Log("RecieveData(): Begin to listen...");
                recv = socket.ReceiveFrom(dataTMP, ref remote);
                Debug.Log("RecieveData(): New message!");

                Debug.Log("RecieveData(): Count for stringData: " + recv);
                Debug.Log("RecieveData(): Length of Data: " + dataTMP.Length);

                stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
                Debug.Log("RecieveData(): Message was: " + stringData);

                socket.SendTo(dataTMP, recv, SocketFlags.None, remote);
                if (stringData.Equals(""))
                {
                    Debug.LogError("RecieveData(): Data was empty :c");
                }
                else if (stringData.Contains("/>hello</")) //Hello message
                {
                    Debug.Log("RecieveData(): New client detected!");
                    sender = (IPEndPoint)remote;
                    AddClientUDP(sender);

                    stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
                    string[] tmpSplit = stringData.Split("</");
                    AddPlayer(tmpSplit[1]);

                    stringData = "User '" + tmpSplit[1] + "' joined the lobby!";

                    string tmp = stringData;
                    Debug.Log("RecieveData(): " + stringData);
                    Thread.Sleep(1);
                    BroadcastServerMessage(ManageMessage("/>servername " + serverName, true, true));
                    Thread.Sleep(1);
                    SendPlayerList();
                    Thread.Sleep(1);
                    BroadcastServerMessage(ManageMessage(tmp, true));
                }
                else if (stringData.Contains("/>goodbye</")) //Goodbye message
				{
                    string[] tmpSplit = stringData.Split("</");
                    uint tmpUid = uint.Parse(tmpSplit[1]);
                    stringData = "User " + lobby.usersList[tmpUid] + " has left the server!";
                    lobby.usersList.Remove(tmpUid);
                    Thread.Sleep(1);
                    BroadcastServerMessage(ManageMessage(stringData,true));
                    Thread.Sleep(1);
                    sender = (IPEndPoint)remote;
                    clientListUDP.Remove(sender);
                    SendPlayerList();
                }
                else if (stringData.Contains("/>PlayerInfo:")) //Gameplay data
                {
                    Debug.Log("RecieveData(): New game state detected");
                    manager.data = dataTMP;
                    manager.recieveThread = new Thread(manager.RecieveGameState);
                    manager.recieveThread.Start();
                    BroadcastPlayerInfo(dataTMP);
                }
                else //Process & broadcast message
                {
                    Debug.Log("RecieveData(): Client data recieved: " + stringData);
                    BroadcastServerMessage(ManageMessage(stringData));
                }
                Thread.Sleep(1);
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
