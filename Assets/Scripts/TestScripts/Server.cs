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

	public List<Socket> clientList = null;

	Thread recieveClientsThread = null;
	Thread recieveDataThread = null;
	bool once = true;
	String stringData = null;

	// Start is called before the first frame update
	void Start()
	{
		clientList = new List<Socket>(8);
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
		if (clientList.Count.Equals(0))
		{
			TryAddClient(client);
		}
		else
		{
			foreach (Socket c in clientList)
			{
				if (clientep != (IPEndPoint)c.RemoteEndPoint)
				{
					TryAddClient(client);
				}
				else
				{
					Debug.Log("No clients connected. Waiting to accept...");
				}
			}
		}
	}

	void TryAddClient(Socket newClient)
	{
		if (clientep.ToString() != "")
		{
			Debug.Log("Connected! client IP: " + clientep.ToString() + " Sending feedback...");
			clientList.Add(client);
			if (once)
			{
				once = false;
				//recieveDataThread = new Thread(RecieveData);
				//recieveDataThread.Start();
			}
		}
		else
		{
			Debug.Log("No clients connected. Waiting to accept...");
		}
	}

	void RecieveData()
	{
		try
		{
			foreach (Socket c in clientList)
			{
				recv = c.Receive(data);
				stringData = Encoding.ASCII.GetString(data, 0, recv);
				if (stringData.Equals(""))
				{
					Debug.Log("Data was empty :c");
				}
				else
				{
					Debug.Log("Client data recieved: " + stringData);
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Error receiving: " + e);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Z))
		{
			Debug.Log("Sending...");
			stringData = "Hello client";
			foreach (Socket c in clientList)
			{
				data = Encoding.ASCII.GetBytes(stringData);
				c.Send(data,recv,SocketFlags.None);
				Debug.Log("sent");
			}
		}
	}

	private void OnDestroy()
	{
		newSocket.Close();
	}
}
