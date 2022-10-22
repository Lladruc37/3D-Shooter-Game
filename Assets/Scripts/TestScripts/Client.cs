using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
	public Socket server;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.1.68"), 9050);

	public int recv;
	public byte[] data;
	public string stringData, input;
	public Socket client;
	public EndPoint clientRemote;
	public IPEndPoint clientep;

	public IPAddress adress = IPAddress.Any;
	bool connected = false;

	Thread connectThread = null;
	Thread sendThread = null;
	Thread helloThread = null;
	Thread receiveThread = null;
	public bool start = false;
	public bool update = false;
	public bool hello = true;
	public Text serverIP;
	public Text username;
	public Chat chatManager;
	public bool messageRecieved = false;

	// Start is called before the first frame update
	void Start()
	{ }

	void Connect()
	{
		try
		{
			Debug.Log("Trying to connect to server...");
			server.Connect(ipep);
			Thread.Sleep(100);
			connected = true;
			Debug.Log("Connected to server. Sending Message...");
			sendThread.Start();
			helloThread = new Thread(Hello);
			helloThread.Start();
		}
		catch (System.Exception e)
		{
			Debug.Log("Connection failed.. trying again...\n Error: " + e);
		}
	}


	void Hello()
	{
		try
		{
			while (hello)
			{
				if (server.Poll(10, SelectMode.SelectRead))
				{
					data = new byte[1024];
					Debug.Log("a");
					recv = server.Receive(data);
					Debug.Log("a2");
					stringData = Encoding.ASCII.GetString(data, 0, recv);
					Debug.Log("Message was: " + stringData);
					if (stringData.Equals(""))
					{
						Debug.Log("Data was empty :c");
						hello = false;
					}
					else
					{
						Debug.Log("Server Data recieved: " + stringData);
						messageRecieved = true;
						hello = false;
						receiveThread = new Thread(Receive);
						receiveThread.Start();
					}
				}
			}

		}
		catch (Exception e)
		{
			Debug.Log("Error receiving: " + e);
		}
	}

	void Receive()
	{
		try
		{
			while (true)
			{
				if (connected && !hello)
				{
					data = new byte[1024];
					recv = server.Receive(data);
					stringData = Encoding.ASCII.GetString(data, 0, recv);
					Debug.Log("Message was: " + stringData);
					if (stringData.Equals(""))
					{
						Debug.Log("Data was empty :c");
					}
					else
					{
						Debug.Log("Server Data recieved: " + stringData);
						messageRecieved = true;
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Error receiving: " + e);
		}
	}

	void Send()
	{
		server.Send(data, data.Length, SocketFlags.None);
	}

	// Update is called once per frame
	void Update()
	{
		if (update)
		{
			if (hello)
			{
				data = Encoding.ASCII.GetBytes(username.text.ToString());
			}

			if (messageRecieved)
			{
				messageRecieved = false;
				chatManager.SendMsg(stringData);
			}
		}
		if (start)
		{
			server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


			if (server.ProtocolType == ProtocolType.Tcp)
			{
				connectThread = new Thread(Connect);
				sendThread = new Thread(Send);
				connectThread.Start();
			}
			start = false;
			update = true;
		}
	}
	private void OnDestroy()
	{
		server.Shutdown(SocketShutdown.Both);
		server.Close();
	}
}
