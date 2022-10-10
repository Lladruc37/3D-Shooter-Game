using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
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

    public IPAddress adress = IPAddress.Any;
    public string username = "NoNameChad";

    // Start is called before the first frame update
    void Start()
    {
        newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        newSocket.Bind(ipep);
    }

    // Update is called once per frame
    void Update()
    {
        if(adress != IPAddress.Any)
		{
            clientep = new IPEndPoint(adress, 1337);
            byte[] msg = Encoding.ASCII.GetBytes("Username: " + username);
            newSocket.SendTo(msg, msg.Length, SocketFlags.None, clientep);
            Debug.Log("Message sent. Waiting for server feedback...");

            recv = newSocket.ReceiveFrom(data, ref clientRemote);
        }
    }
}
