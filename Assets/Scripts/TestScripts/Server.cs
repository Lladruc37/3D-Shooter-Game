using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
	public Socket newSocket;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

	public String newServername;
	public int recv;
	public byte[] data;
	//public Socket client;
	//public EndPoint clientRemote;
	public IPEndPoint clientep;

	public List<Socket> clientList = new List<Socket>();
	public List<Socket> clientsAccepted = new List<Socket>();
	List<string> usernames = new List<string>();

	Thread connectClientsThread = null;
	Thread helloThread = null;
	Thread recieveDataThread = null;
	string stringData = null;
	public bool start = false;
	bool update = false;
	public Chat chatManager;
	public bool clientJoined = false;

	// Start is called before the first frame update
	void Start()
	{}

	// Update is called once per frame
	void Update()
	{
		if (start)
        {
			clientList = new List<Socket>(8);
			newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			newSocket.Bind(ipep);
			if (newSocket.ProtocolType == ProtocolType.Tcp)
			{
				connectClientsThread = new Thread(ConnectClients);
				helloThread = new Thread(Hello);
				connectClientsThread.Start();
			}
			start = false;
			update = true;
        }
		if (update)
		{
			if(clientJoined)
			{
				clientJoined = false;
				chatManager.SendMsg(stringData);
			}
		}
	}

	void ConnectClients()
	{
		Debug.Log("Looking for clients...");
		try
		{
			while (true)
			{
				newSocket.Listen(10);
				Debug.Log("Waiting for client...");
				Socket client = newSocket.Accept();
				clientep = (IPEndPoint)client.RemoteEndPoint;
				if (clientList.Count.Equals(0))
				{
					AddClient(client);
				}
				else
				{
					foreach (Socket c in clientList)
					{
						if (clientep != (IPEndPoint)c.RemoteEndPoint)
						{
							AddClient(client);
						}
						else
						{
							Debug.Log("No clients connected. Waiting to accept...");
						}
					}
				}

			}
		}
		catch (Exception e)
		{
			Debug.Log("Error receiving: " + e);
		}
	}

	void AddClient(Socket newClient)
	{
		if (clientep.ToString() != "")
		{
			clientList.Add(newClient);
			Debug.Log("Connected to: " + clientep.ToString());
			helloThread.Start();
		}
		else
		{
			Debug.Log("No clients connected. Waiting to accept...");
		}
	}

	void Hello()
	{
		try
		{
			bool done = false;
			while (!done)
			{
				Socket.Select(clientList, null, null, -1);
				foreach (Socket c in clientList)
				{
					data = new byte[1024];
					recv = c.Receive(data);
					stringData = Encoding.ASCII.GetString(data, 0, recv);
					Debug.Log("Message was: " + stringData);
					if (stringData.Equals(""))
					{
						Debug.Log("Data was empty :c");
					}
					else
					{
						//TODO: check username
						stringData = "User '" + stringData + "' joined the lobby!";
						Debug.Log(stringData);
						clientJoined = true;
						clientsAccepted.Add(c);
						BroadcastMessage(stringData, true);
						recieveDataThread = new Thread(RecieveData);
						recieveDataThread.Start();
						done = true;
					}
					Thread.Sleep(100);
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Error receiving: " + e);
		}
	}

	void BroadcastMessage(string m, bool isServer = false)
	{
		foreach (Socket c in clientsAccepted)
		{
			if (isServer)
			{
				m += "\n" + m;
			}
			else
			{
				string[] name;
				if(m.Contains("/>username"))
				{
					string tmp = m.Remove(0, 11);
					name = tmp.Split("</");
					m = name[1];
				}
				else
				{
					Debug.Log("Error: No username detected");
					return;
				}
				m += "\n[" + name[0] + "]>>" + m;
			}
			data = Encoding.ASCII.GetBytes(stringData);
			c.Send(data, data.Length, SocketFlags.None);
			Debug.Log("sent");
		}
	}

	void RecieveData()
	{
		try
		{
			while (true)
			{
				foreach (Socket c in clientsAccepted)
				{
					if (c.Poll(10, SelectMode.SelectRead))
					{
						data = new byte[1024];
						recv = c.Receive(data);
						stringData = Encoding.ASCII.GetString(data, 0, recv);
						Debug.Log("Message was: " + stringData);
						if (stringData.Equals(""))
						{
							Debug.Log("Data was empty :c");
						}
						else
						{
							Debug.Log("Client data recieved: " + stringData);
							//TODO: chat message from user & send it the all players
						}
					}
					Thread.Sleep(100);
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Error receiving: " + e);
		}
	}


	private void OnDestroy()
	{
		//TODO: abort() & clear() all ongoing threads
		//TODO: shutdown() & close() all clients sockets
		newSocket.Close();
	}
}
