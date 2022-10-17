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
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

	public int recv;
	public byte[] data;
	public Socket client;
	public EndPoint clientRemote;
	public IPEndPoint clientep;

	Thread recieveClientsThread = null;

	// Start is called before the first frame update
	void Start()
	{
		newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		newSocket.Bind(ipep);
		if (newSocket.ProtocolType == ProtocolType.Tcp)
		{
			newSocket.Listen(10);
			Debug.Log("Waiting for client...");
			recieveClientsThread = new Thread(RecieveClients);
			recieveClientsThread.Start();
		}
		//else if (newSocket.ProtocolType == ProtocolType.Udp)
		//{
		//	recv = newSocket.ReceiveFrom(data, ref clientRemote);
		//	Debug.Log("Waiting for clients...");
		//	clientep = (IPEndPoint)client.RemoteEndPoint;
		//	Debug.Log("Connected: " + clientep.ToString() + "\n Sending feedback...");
		//	byte[] msg = Encoding.ASCII.GetBytes("Server Feedback. Message Recieved.");
		//	newSocket.SendTo(msg, msg.Length, SocketFlags.None, clientRemote);
		//}

	}

	void RecieveClients()
	{
		client = newSocket.Accept();
		clientep = (IPEndPoint)client.RemoteEndPoint;
		Debug.Log("Connected: " + clientep.ToString() + "\n Sending feedback...");
	}

	// Update is called once per frame
	void Update()
	{
	}

	private void OnDestroy()
	{
		newSocket.Close();
	}
}
