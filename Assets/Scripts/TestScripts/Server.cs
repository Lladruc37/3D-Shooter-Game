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
	public bool isTCP = true;

	public Socket newSocket;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

	public String newServername;
	public int recv;
	public byte[] data;
	//public Socket client;
	//public EndPoint clientRemote;
	public IPEndPoint clientep;

	//UDP
	public List<IPEndPoint> clientListUDP = new List<IPEndPoint>();

	//TCP
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
	public bool newMessage = false;

	// Start is called before the first frame update
	void Start()
	{}

	// Update is called once per frame
	void Update()
	{
		if (isTCP)
		{
			if (start)
			{
				clientList = new List<Socket>(8);
				newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				newSocket.Bind(ipep);
				connectClientsThread = new Thread(ConnectClients);
				connectClientsThread.Start();
				start = false;
				update = true;
			}
			if (update)
			{
				if (newMessage)
				{
					newMessage = false;
					chatManager.SendMsg(stringData);
				}
			}
		}
		else
        {
			if(start)
			{
				newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				newSocket.Bind(ipep);
				connectClientsThread = new Thread(ConnectClients);
				connectClientsThread.Start();
				start = false;
				update = true;
			}
			if (update)
			{
				if (newMessage)
				{
					newMessage = false;
					chatManager.SendMsg(stringData);
				}
			}
		}
	}

	void ConnectClients()
	{
		Debug.Log("Looking for clients...");
		if (isTCP)
		{
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
						AddClientTCP(client);
					}
					else
					{
						foreach (Socket c in clientList)
						{
							if (clientep != (IPEndPoint)c.RemoteEndPoint)
							{
								AddClientTCP(client);
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
		else
        {
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 8050);
			EndPoint remote = (EndPoint)(sender);

			try
            {
				data = new byte[1024];
				recv = newSocket.ReceiveFrom(data, ref remote);
				AddClientUDP(sender);

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
					BroadcastServerMessage(ManageMessage(stringData, true));
					recieveDataThread = new Thread(RecieveData);
					recieveDataThread.Start();
				}
			}
			catch (Exception e)
            {
				Debug.Log("Error receiving: " + e);
			}
		}
	}

	void AddClientTCP(Socket newClient)
	{
		if (clientep.ToString() != "")
		{
			clientList.Add(newClient);
			Debug.Log("Connected to: " + clientep.ToString());
			helloThread = new Thread(Hello);
			helloThread.Start();
		}
		else
		{
			Debug.Log("No clients connected. Waiting to accept...");
		}
	}

	void AddClientUDP(IPEndPoint newClient)
	{
		if (newClient.ToString() != "")
		{
			clientListUDP.Add(newClient);
			Debug.Log("Connected to: " + newClient.ToString());
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
						clientsAccepted.Add(c);
						Thread.Sleep(100);
						BroadcastServerMessage(ManageMessage(stringData, true));
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

	string ManageMessage(string m, bool isServer = false)
	{
		string result = "";
		if (isServer)
		{
			result = "\n" + m;
		}
		else
		{
			string[] name;
			if (m.Contains("/>username"))
			{
				string tmp = m.Remove(0, 11);
				name = tmp.Split("</");
				m = name[1];
				result = "\n[" + name[0] + "]>>" + m;
			}
			else
			{
				Debug.Log("Error: No username detected");
			}
		}
		Debug.Log("sending message: " + result);
		stringData = result;
		newMessage = true;
		return result;
	}

	void BroadcastServerMessage(string m)
	{
		if (isTCP)
        {
			foreach (Socket c in clientsAccepted)
			{
				data = new byte[1024];
				Debug.Log("Broadcasting message: " + m);
				data = Encoding.ASCII.GetBytes(m);
				c.Send(data, data.Length, SocketFlags.None);
				Debug.Log("Sent TCP Style");
			}
		}
		else
        {
			foreach (IPEndPoint ip in clientListUDP)
            {
				data = Encoding.ASCII.GetBytes(m);
				EndPoint remote = (EndPoint)ip;
				newSocket.SendTo(data, data.Length, SocketFlags.None, remote);
				Debug.Log("Sent UDP Style");
			}
		}
	}

	void RecieveData()
	{
		if (isTCP)
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
								BroadcastServerMessage(ManageMessage(stringData));
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
		else
        {
			try
			{
				while (true)
				{
					foreach (IPEndPoint ip in clientListUDP)
					{
						data = new byte[1024];
						EndPoint remote = (EndPoint)ip;
						recv = newSocket.ReceiveFrom(data, ref remote);

						stringData = Encoding.ASCII.GetString(data, 0, recv);
						Debug.Log("Message was: " + stringData);

						newSocket.SendTo(data, recv, SocketFlags.None, remote);
						if (stringData.Equals(""))
						{
							Debug.Log("Data was empty :c");
						}
						else
						{
							Debug.Log("Client data recieved: " + stringData);
							//TODO: chat message from user & send it the all players
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
	}


	private void OnDestroy()
	{
		//TODO: abort() & clear() all ongoing threads
		//TODO: shutdown() & close() all clients sockets
		newSocket.Close();
	}
}
