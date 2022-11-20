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
    public Socket newSocket;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
    public IPEndPoint clientep;
    public List<IPEndPoint> clientListUDP = new List<IPEndPoint>();

    Thread recieveDataThread = null;
    string stringData = null;

    public string hostUsername = "";
    public string serverName = "Server";
    public uint maxUid = 0;
    public uint uid = 0;

    public bool start = false;
    bool update = false;
    public Chat chatManager;
    public bool newMessage = false;
    public GameObject gameplayScene;
    public LobbyScripts lobby;
    public GameplayManager manager;

    // Start is called before the first frame update
    void Start()
    { }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            start = false;
            update = true;
            uid = maxUid;
            Debug.Log("Server(): Starting server...");
            newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            newSocket.Bind(ipep);

            recieveDataThread = new Thread(RecieveData);
            try
            {
                recieveDataThread.Start();
            }
            catch (ThreadStartException e)
            {
                Debug.LogError("Start(): Error starting thread: " + e);
            }

            AddPlayer(hostUsername);
            Debug.Log("Server(): Server started successfully!");
        }

        if (update)
        {
            if (newMessage)
            {
                newMessage = false;
                chatManager.SendMsg(stringData);
            }
            if (Input.GetKeyDown(KeyCode.Return))
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

        //TODO: Temporary solution
        Thread.Sleep(100);
    }

    void AddPlayer(string name)
    {
        lobby.usersList.Add(maxUid, name);
        maxUid++;
    }

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

    public string ManageMessage(string m, bool isServer = false, bool isServernameMessage = false)
    {
        string result = "";
        string[] splitName;
        if (isServer)
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
        else
        {
            if (m.Contains("/>client/>uuid"))
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
        if (!isServernameMessage)
        {
            stringData = result;
            newMessage = true;
        }
        return result;
    }

    public void BroadcastServerMessage(string m)
    {
        //Debug.Log("BroadcastServerMessage(): Broadcasting message: " + m);

        byte[] dataTMP = new byte[1024];
        dataTMP = Encoding.ASCII.GetBytes(m);

        foreach (IPEndPoint ip in clientListUDP)
        {
            EndPoint remote = (EndPoint)ip;
            newSocket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
            //Debug.Log("BroadcastServerMessage(): Message sent successfully");
        }
    }

    public void BroadcastServerInfo(MemoryStream stream)
    {
        //Debug.Log("BroadcastServerInfo(): Sending gameplay state...");

        byte[] dataTMP = new byte[1024];
        dataTMP = stream.GetBuffer();

        //Debug.Log("BroadcastServerInfo(): Data Length is: " + stream.Length);

        foreach (IPEndPoint ip in clientListUDP)
        {
            EndPoint remote = (EndPoint)ip;
            newSocket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
            //Debug.Log("BroadcastServerInfo(): Message sent successfully");
        }
    }

    public void BroadcastPlayerInfo(byte[] data)
    {
        Debug.Log("BroadcastPlayerInfo(): Sending gameplay state from player...");

        foreach (IPEndPoint ip in clientListUDP)
        {
            EndPoint remote = (EndPoint)ip;
            newSocket.SendTo(data, data.Length, SocketFlags.None, remote);
            Debug.Log("BroadcastPlayerInfo(): Message sent successfully");
        }
    }

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
                recv = newSocket.ReceiveFrom(dataTMP, ref remote);
                Debug.Log("RecieveData(): New message!");

                Debug.Log("RecieveData(): Count for stringData: " + recv);
                Debug.Log("RecieveData(): Length of Data: " + dataTMP.Length);

                stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
                Debug.Log("RecieveData(): Message was: " + stringData);

                newSocket.SendTo(dataTMP, recv, SocketFlags.None, remote);
                if (stringData.Equals(""))
                {
                    Debug.LogError("RecieveData(): Data was empty :c");
                }
                else if (stringData.Contains("/>hello</"))
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
                    Thread.Sleep(100);
                    BroadcastServerMessage(ManageMessage("/>servername " + serverName, true, true));
                    Thread.Sleep(100);
                    SendPlayerList();
                    Thread.Sleep(100);
                    BroadcastServerMessage(ManageMessage(tmp, true));
                }
                else if (stringData.Contains("/>goodbye</"))
				{
                    string[] tmpSplit = stringData.Split("</");
                    uint tmpUid = uint.Parse(tmpSplit[1]);
                    stringData = "User " + lobby.usersList[tmpUid] + " has left the server!";
                    lobby.usersList.Remove(tmpUid);
                    Thread.Sleep(100);
                    BroadcastServerMessage(ManageMessage(stringData,true));
                    Thread.Sleep(100);
                    SendPlayerList();
                }
                else if (stringData.Contains("/>PlayerInfo:"))
                {
                    Debug.Log("RecieveData(): New game state detected");
                    manager.data = dataTMP;
                    manager.recieveThread = new Thread(manager.RecieveGameState);
                    manager.recieveThread.Start();
                    BroadcastPlayerInfo(dataTMP);
                }
                else
                {
                    Debug.Log("RecieveData(): Client data recieved: " + stringData);
                    //TODO: chat message from user & send it the all players
                    BroadcastServerMessage(ManageMessage(stringData));
                }
                Thread.Sleep(100);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("RecieveData(): Error receiving: " + e);
        }
    }
    public void Close()
    {
        start = false;
        update = false;

        if (newSocket != null)
        {
            newSocket.Close();
            newSocket = null;
        }

        CloseThreads();
    }
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
        if (newSocket != null)
        {
            newSocket.Close();
            newSocket = null;
        }

        CloseThreads();
    }
}
