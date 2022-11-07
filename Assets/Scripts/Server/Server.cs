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
	public bool isTCP = true;

	public Socket newSocket;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

	public string ServerUsername = "Server";
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
				if (Input.GetKeyDown(KeyCode.Return))
				{
					if (chatManager.input.text != "")
					{
						string msg = "\n" + "/>username" + ServerUsername + "</" + chatManager.input.text;
						chatManager.input.text = "";
						BroadcastServerMessage(ManageMessage(msg));
					}
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
				if (Input.GetKeyDown(KeyCode.Return))
				{
					if (chatManager.input.text != "")
					{
						string msg = "\n" + "/>username" + ServerUsername + "</" + chatManager.input.text;
						chatManager.input.text = "";
						BroadcastServerMessage(ManageMessage(msg));
					}
				}
			}
		}
	}

	public void ToggleTCP(bool boolean)
    {
		isTCP = boolean;
    }

	void ConnectClients()
	{
		Debug.Log("ConnectClients(): Looking for clients...");
		if (isTCP)
		{
			try
			{
				while (true)
				{
					newSocket.Listen(10);
					Debug.Log("ConnectClients(): Waiting for client...");
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
								Debug.Log("ConnectClients(): No clients connected. Waiting to accept...");
							}
						}
					}

				}
			}
			catch (Exception e)
			{
				Debug.Log("ConnectClients(): Error receiving: " + e);
			}
		}
		else
        {
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 9050);
			EndPoint remote = (EndPoint)(sender);

			try
            {
				int recv;
				byte[] dataTMP = new byte[1024];
				recv = newSocket.ReceiveFrom(dataTMP, ref remote);
				sender = (IPEndPoint)remote;
				AddClientUDP(sender);

				stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
				Debug.Log("ConnectClients(): Message was: " + stringData);
				if (stringData.Equals(""))
				{
					Debug.Log("ConnectClients(): Data was empty :c");
				}
				else
				{
					//TODO: check username
					stringData = "User '" + stringData + "' joined the lobby!";
					string tmp = stringData;
					Debug.Log(stringData);
					Thread.Sleep(100);
					BroadcastServerMessage(ManageMessage("/servername " + ServerUsername, true, true));
					Thread.Sleep(100);
					BroadcastServerMessage(ManageMessage(tmp, true));
					recieveDataThread = new Thread(RecieveData);
					recieveDataThread.Start();
				}
			}
			catch (Exception e)
            {
				Debug.Log("ConnectClients(): Error receiving: " + e);
			}
		}
	}

	void AddClientTCP(Socket newClient)
	{
		if (clientep.ToString() != "")
		{
			clientList.Add(newClient);
			Debug.Log("AddClientTCP(): Connected to: " + clientep.ToString());
			helloThread = new Thread(Ping);
			helloThread.Start();
		}
		else
		{
			Debug.Log("AddClientTCP(): No clients connected. Waiting to accept...");
		}
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
			Debug.Log("AddClientUDP(): No clients connected. Waiting to accept...");
		}
	}


	void Ping()
	{
		try
		{
			bool done = false;
			while (!done)
			{
				Socket.Select(clientList, null, null, -1);
				foreach (Socket c in clientList)
				{
					int recv;
					byte[] dataTMP = new byte[1024];
					recv = c.Receive(dataTMP);
					stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
					Debug.Log("Ping(): Message was: " + stringData);
					if (stringData.Equals(""))
					{
						Debug.Log("Data was empty :c");
					}
					else
					{
						//TODO: check username
						stringData = "User '" + stringData + "' joined the lobby!";
						string tmp = stringData;
						Debug.Log(stringData);
						clientsAccepted.Add(c);
						Thread.Sleep(100);
						BroadcastServerMessage(ManageMessage("/servername " + ServerUsername, true,true));
						Thread.Sleep(100);
						BroadcastServerMessage(ManageMessage(tmp, true));
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

	string ManageMessage(string m, bool isServer = false, bool isServernameMessage = false)
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
				Debug.Log("ManageMessage(): Error: No username detected");
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

	void BroadcastServerMessage(string m)
	{
		Debug.Log("BroadcastServerMessage(): Broadcasting message: " + m);

		if (isTCP)
        {
			foreach (Socket c in clientsAccepted)
			{
				byte[] dataTMP = new byte[1024];
				dataTMP = Encoding.ASCII.GetBytes(m);
				c.Send(dataTMP, dataTMP.Length, SocketFlags.None);
				Debug.Log("BroadcastServerMessage(): Sent TCP Style");
			}
		}
		else
        {
			byte[] dataTMP = new byte[1024];
			dataTMP = Encoding.ASCII.GetBytes(m);

			foreach (IPEndPoint ip in clientListUDP)
            {
				EndPoint remote = (EndPoint)ip;
				newSocket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
				Debug.Log("BroadcastServerMessage(): Sent UDP Style");
			}
		}
	}

	public void BroadcastServerInfo(MemoryStream stream)
    {
		Debug.Log("BroadcastServerInfo(): Sending gameplay state...");

		byte[] dataTMP = new byte[1024];
		dataTMP = stream.GetBuffer();

		Debug.Log("BroadcastServerInfo(): Data Length is: " + stream.Length);

		foreach (IPEndPoint ip in clientListUDP)
		{
			EndPoint remote = (EndPoint)ip;
			newSocket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
			Debug.Log("BroadcastServerInfo(): Sent UDP Style");
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
							int recv;
							byte[] dataTMP = new byte[1024];
							recv = c.Receive(dataTMP);
							stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
							Debug.Log("RecieveData(): Message was: " + stringData);
							if (stringData.Equals(""))
							{
								Debug.Log("RecieveData(): Data was empty :c");
							}
							else
							{
								Debug.Log("RecieveData(): Client data recieved: " + stringData);
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
				Debug.Log("RecieveData(): Error receiving: " + e);
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
						int recv;
						byte[] dataTmp = new byte[1024];
						EndPoint remote = (EndPoint)ip;
						recv = newSocket.ReceiveFrom(dataTmp, ref remote);

						Debug.Log("RecieveData(): Count for stringData: " + recv);
						Debug.Log("RecieveData(): Length of Data: " + dataTmp.Length);

						stringData = Encoding.ASCII.GetString(dataTmp, 0, recv);
						Debug.Log("RecieveData(): Message was: " + stringData);

						newSocket.SendTo(dataTmp, recv, SocketFlags.None, remote);
						if (stringData.Equals(""))
						{
							Debug.Log("RecieveData(): Data was empty :c");
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
			}
			catch (Exception e)
			{
				Debug.Log("RecieveData(): Error receiving: " + e);
			}
		}
	}


	private void OnDestroy()
	{
		if (newSocket != null)
        {
			newSocket.Close();
			newSocket = null;
		}
	}
}
