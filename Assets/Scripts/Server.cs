using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    public Socket newSocket;
    public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1337);
    public bool connected = false;
    public bool exit = false;

    public int recv;
    public byte[] data;
    public Socket client;
    public EndPoint clientRemote;
    public IPEndPoint clientep;

    // Start is called before the first frame update
    void Start()
    {
        newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        newSocket.Bind(ipep);
    }

    // Update is called once per frame
    void Update()
    {
        if (exit)
        {
            newSocket.Close();
            return;
        }

        if (!connected && !exit)
		{
            try
            {
                recv = newSocket.ReceiveFrom(data, ref clientRemote);
                Debug.Log("Waiting for clients...");
                clientep = (IPEndPoint)client.RemoteEndPoint;
                Debug.Log("Connected: " + clientep.ToString() + "\n Sending feedback...");
                byte[] msg = Encoding.ASCII.GetBytes("Server Feedback. Message Recieved.");
                newSocket.SendTo(msg,msg.Length,SocketFlags.None, clientRemote);
                connected = true;
            }
            catch (System.Exception e)
            {
                Debug.Log("Connection failed.. trying again...\n Error: " + e);
            }
        }
    }
}
