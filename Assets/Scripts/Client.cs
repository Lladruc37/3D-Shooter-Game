using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
	public Socket server;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

	public string stringData, input;
	public Socket client;
	public EndPoint clientRemote;
	public IPEndPoint clientep;
	public SendRecieve sendRecieve;

	public IPAddress adress = IPAddress.Any;
	bool connected = false;

	Thread connectThread = null;
	Thread receiveThread = null;
	Thread RecievePlayerListThread = null;
	public bool start = false;
	public bool update = false;
	public Text serverIP;
	public Text username;
	public Chat chatManager;
	public bool messageRecieved = false;
	public bool newServerName = false;
	public bool newServerIP = false;
	public Text clientTitle;
	public Canvas chatCanvas = null;
	public GameObject gameplayScene;
	bool startGame = false;
	public LobbyScripts lobby;

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
				Debug.Log("Update(): CurrentMessage: " + stringData);
				chatManager.SendMsg(stringData);
				stringData = "";
			}
			if(newServerName)
			{
				newServerName = false;
				string tmp = stringData.Remove(0, 14);
				clientTitle.text = "Welcome to " + tmp + "!";
				chatCanvas.GetComponent<Canvas>().enabled = true;
				Debug.Log("Update(): Changed server title to: "+ clientTitle.text);
			}
			if(startGame)
			{
				startGame = false;
				gameplayScene.SetActive(true);
				Debug.Log("Starting client game...");
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

			server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			connectThread = new Thread(Connect);
			connectThread.Start();
			start = false;
			update = true;
		}
	}

	void Connect()
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
			Debug.Log("Connect(): Connection failed.. trying again...\n Error: " + e);
		}
	}

	void Receive()
	{
		try
		{
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint Remote = (EndPoint)sender;

			while (true)
			{
				if (connected)
				{
					int recv;
					byte[] dataTMP = new byte[1024];

					recv = server.ReceiveFrom(dataTMP, ref Remote);
					stringData = Encoding.ASCII.GetString(dataTMP, 0, recv);
					Debug.Log("Recieve(): Message was: " + stringData);
					if (stringData.Equals(""))
					{
						Debug.Log("Recieve(): Data was empty :c");
					}
					else
					{
						Debug.Log("Recieve(): Server Data recieved: " + stringData);
						if (stringData.Contains("/>servername"))
						{
							newServerName = true;
							Debug.Log("Recieve(): New server name change detected");
						}
						else if (stringData.Contains("/>PlayerInfo:"))
						{
							Debug.Log("Recieve(): New game state detected");
							sendRecieve.data = dataTMP;
							sendRecieve.recieveThread = new Thread(sendRecieve.RecieveGameState);
							sendRecieve.recieveThread.Start();
						}
						else if(stringData.Contains("/>list</"))
						{
							Debug.Log("Recieve(): New users list detected");
							RecievePlayerListThread = new Thread(RecievePlayerList);
							RecievePlayerListThread.Start();
						}
						else
						{
							if(stringData == "\nStarting game...")
							{
								startGame = true;
							}
							//TODO: This if shouldn't exist
							if (!stringData.Contains("/>username"))
							{
								messageRecieved = true;
							}
							Debug.Log("Recieve(): No new server name changes detected");
						}
						Thread.Sleep(100);
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log("Recieve(): Error receiving: " + e);
		}
	}
	void Send(string m)
	{
		Debug.Log("SENDING MESSAGE" + m);
		byte[] dataTMP = Encoding.ASCII.GetBytes(m);
		try
		{
			server.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, ipep);
		}
		catch (Exception e)
        {
			Debug.Log("Send(): Error receiving: " + e);
		}
	}

	public void RecievePlayerList()
	{
		byte[] data = null;
		Debug.Log("RecieveList(): Recieved info");
		MemoryStream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		//Header
		string header = reader.ReadString();
		Debug.Log("RecieveList(): Header is " + header);

		//List
		int count = reader.ReadInt32();
		for (int i = 0; i <= count; i++)
		{
			string tmp = reader.ReadString();
			lobby.usernameList.Add(tmp);
			int tmpi = reader.ReadInt32();
			Debug.Log(tmpi);
		}

		//TODO: Temporary solution
		Thread.Sleep(100);
	}

	// Update is called once per frame
	private void OnDestroy()
	{
		if (server != null)
        {
			server.Close();
			server = null;
		}
	}
}
