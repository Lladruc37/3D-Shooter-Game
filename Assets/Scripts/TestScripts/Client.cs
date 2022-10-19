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
	public Socket server;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

	public int recv;
	public byte[] data;
	public String stringData, input;
	public Socket client;
	public EndPoint clientRemote;
	public IPEndPoint clientep;

	public IPAddress adress = IPAddress.Any;
	public string username = "NoNameChad";
	bool connected = false;

	Thread connectThread = null;
	Thread receiveThread = null;
	bool once = true;

	// Start is called before the first frame update
	void Start()
	{
		server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		if (server.ProtocolType == ProtocolType.Tcp)
		{
			connectThread = new Thread(Connect);
			receiveThread = new Thread(Receive);
			connectThread.Start();

		}
		//else if (server.ProtocolType == ProtocolType.Udp)
		//{
		//	if (adress != IPAddress.Any)
		//	{
		//		clientep = new IPEndPoint(adress, 9050);
		//		byte[] msg = Encoding.ASCII.GetBytes("Username: " + username);
		//		server.SendTo(msg, msg.Length, SocketFlags.None, clientep);
		//		Debug.Log("Message sent. Waiting for server feedback...");

		//		recv = server.ReceiveFrom(data, ref clientRemote);
		//	}
		//}
	}

	void Connect()
	{
		try
		{
			server.Connect(ipep);
			connected = true;
			Debug.Log("Trying to connect to server...");
		}
		catch (System.Exception e)
		{
			Debug.Log("Connection failed.. trying again...\n Error: " + e);
		}
	}

	void Receive()
	{
		if (connected)
		{
			try
			{
				Debug.Log("a");
				recv = server.Receive(data);
				Debug.Log("a2");
				stringData = Encoding.ASCII.GetString(data, 0, recv);
				Debug.Log(stringData);
				if (stringData.Equals(""))
				{
					Debug.Log("Data was empty :c");
				}
				else
				{
				Debug.Log("Conected! Data recieved: " + stringData);
				}
			}
			catch(Exception e)
			{
				Debug.Log("Error receiving: " + e);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (once)
		{
			receiveThread.Start();
			once = false;
		}
	}
}
