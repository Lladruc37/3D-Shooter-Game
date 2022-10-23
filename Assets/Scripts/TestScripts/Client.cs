using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
	public bool isTCP = true;

	public Socket server;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.1.67"), 9050);

	public int recv;
	public byte[] data;
	public string stringData, input;
	public Socket client;
	public EndPoint clientRemote;
	public IPEndPoint clientep;

	public IPAddress adress = IPAddress.Any;
	bool connected = false;

	Thread connectThread = null;
	Thread receiveThread = null;
	public bool start = false;
	public bool update = false;
	public Text serverIP;
	public Text username;
	public Chat chatManager;
	public bool messageRecieved = false;
	public bool newServerName = false;
	public bool newServerIP = false;
	public Text clientTitle;

	// Start is called before the first frame update
	void Start()
	{}

	void Update()
	{
		if (update)
		{
			if (messageRecieved)
			{
				messageRecieved = false;
				Debug.Log("CURRENT MESSAGE: " + stringData);
				chatManager.SendMsg(stringData);
				stringData = "";
			}
			if(newServerName)
			{
				newServerName = false;
				string tmp = stringData.Remove(0, 13);
				clientTitle.text = "Welcome to " + tmp + "!";
				Debug.Log("BIBUUUUUUUUUUUUUUUUU");
			}

			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (chatManager.input.text != "")
				{
					string msg = "\n" + "/>username" + username.text.ToString() + "</" + chatManager.input.text;
					chatManager.input.text = "";
					Send(msg);
				}
			}
		}

		if (start)
		{
			if (newServerIP)
			{
				ipep = new IPEndPoint(IPAddress.Parse(serverIP.text.ToString()), 9050);
			}
			if (isTCP)
			{
				server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				connectThread = new Thread(Connect);
				connectThread.Start();
				start = false;
				update = true;
			}
			else
			{
				server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				connectThread = new Thread(Connect);
				connectThread.Start();
				start = false;
				update = true;
			}
		}
	}

	void Connect()
	{
		if (isTCP)
        {
			try
			{
				Debug.Log("Trying to connect to server...");
				server.Connect(ipep);
				Thread.Sleep(100);
				connected = true;
				Debug.Log("Connected to server. Sending Message...");
				Send(username.text.ToString());
				receiveThread = new Thread(Receive);
				receiveThread.Start();
			}
			catch (System.Exception e)
			{
				Debug.Log("Connection failed.. trying again...\n Error: " + e);
			}
		}
		else
        {
			try
            {
				Send(username.text.ToString());
				connected = true;
				receiveThread = new Thread(Receive);
				receiveThread.Start();
			}
			catch (System.Exception e)
			{
				Debug.Log("Connection failed.. trying again...\n Error: " + e);
			}
		}
	}

	void Receive()
	{
		if (isTCP)
        {
			try
			{
				while (true)
				{
					if (connected)
					{
						if (server.Poll(10, SelectMode.SelectRead))
						{
							data = null;
							GC.Collect();

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
								if(stringData.Contains("/servername"))
								{
									newServerName = true;
								}
								else
								{
									messageRecieved = true;
								}
								Thread.Sleep(100);
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
			try
			{
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				EndPoint Remote = (EndPoint)sender;

				while (true)
				{
					if (connected)
					{
						data = null;
						GC.Collect();

						data = new byte[1024];

						recv = server.ReceiveFrom(data, ref Remote);
						stringData = Encoding.ASCII.GetString(data, 0, recv);
						Debug.Log("Message was: " + stringData);
						if (stringData.Equals(""))
						{
							Debug.Log("Data was empty :c");
						}
						else
						{
							Debug.Log("Server Data recieved: " + stringData);
							if (stringData.Contains("/servername"))
							{
								newServerName = true;
								Debug.Log("NEW SERVER NAME");
							}
							else
							{
								//TODO: This if shouldn't exist
								if (!stringData.Contains("/>username"))
								{
									messageRecieved = true;
								}
                                Debug.Log("NO NEW SERVER NAME");
							}
							Thread.Sleep(100);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.Log("Error receiving: " + e);
			}
		}
	}

	void Send(string m)
	{
		data = Encoding.ASCII.GetBytes(m);
		if (isTCP)
        {
			server.Send(data, data.Length, SocketFlags.None);
		}
		else
        {
			try
			{
				server.SendTo(data, data.Length, SocketFlags.None, ipep);
			}
			catch (Exception e)
            {
				Debug.Log("Error receiving: " + e);
			}
		}
	}

	// Update is called once per frame
	private void OnDestroy()
	{
		server.Shutdown(SocketShutdown.Both);
		server.Close();
	}
}
