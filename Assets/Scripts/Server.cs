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
	public Socket newSocket;
	public IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
	public IPEndPoint clientep;
	public List<IPEndPoint> clientListUDP = new List<IPEndPoint>();
	
	Thread connectClientsThread = null;
	Thread recieveDataThread = null;
	string stringData = null;

	public string hostUsername = "";
	public string serverName = "Server";

	public bool start = false;
	bool update = false;
	public Chat chatManager;
	public bool newMessage = false;
	public GameObject gameplayScene;
	public LobbyScripts lobby;

	// Start is called before the first frame update
	void Start()
	{}

	// Update is called once per frame
	void Update()
	{
		if(start)
		{
			lobby.usernameList.Add(hostUsername);
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
			if (Input.GetKeyDown(KeyCode.Tab)) //START BUTTON
			{
				SendPlayerList();
				gameplayScene.SetActive(true);
				GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();
				manager.start = true;
				manager.UserName = hostUsername;
				string msg = "/>startgame</Starting game...";
				BroadcastServerMessage(ManageMessage(msg, true));
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (chatManager.input.text != "")
				{
					string msg = "\n" + "/>username" + hostUsername + "</" + chatManager.input.text;
					chatManager.input.text = "";
					BroadcastServerMessage(ManageMessage(msg));
				}
			}
		}
	}
	public void SendPlayerList()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		//Header
		writer.Write("/>list</");
		writer.Write(lobby.usernameList.Count);

		//List
		int i = 0;
		foreach (string user in lobby.usernameList)
		{
			writer.Write(user);
			writer.Write(i);
			i++;
		}

		Debug.Log("SendList(): Sending list...");
		BroadcastServerInfo(stream);

		//TODO: Temporary solution
		Thread.Sleep(100);
	}

	void ConnectClients()
	{
		Debug.Log("ConnectClients(): Looking for clients...");
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
				lobby.usernameList.Add(stringData);
				stringData = "User '" + stringData + "' joined the lobby!";
				string tmp = stringData;
				Debug.Log(stringData);
				Thread.Sleep(100);
				BroadcastServerMessage(ManageMessage("/>servername " + serverName, true, true));
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

	string ManageMessage(string m, bool isServer = false, bool isServernameMessage = false)
	{
		string result = "";
		string[] splitName;
		if (isServer)
		{
			if (m.Contains("/>startgame"))
			{
				string tmp = m;
				splitName = tmp.Split("</");
				m = splitName[1];
			}
			result = "\n" + m;
		}
		else
		{
			if (m.Contains("/>username"))
			{
				string tmp = m.Remove(0, 11);
				splitName = tmp.Split("</");
				m = splitName[1];
				result = "\n[" + splitName[0] + "]>>" + m;
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

		byte[] dataTMP = new byte[1024];
		dataTMP = Encoding.ASCII.GetBytes(m);

		foreach (IPEndPoint ip in clientListUDP)
        {
			EndPoint remote = (EndPoint)ip;
			newSocket.SendTo(dataTMP, dataTMP.Length, SocketFlags.None, remote);
			Debug.Log("BroadcastServerMessage(): Sent UDP Style");
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


	private void OnDestroy()
	{
		if (newSocket != null)
        {
			newSocket.Close();
			newSocket = null;
		}
	}
}
