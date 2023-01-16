using System.Collections.Generic;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class LobbyScripts : MonoBehaviour
{
    //UI
    public Text title;
    public Camera lobbyCamera;
    public Canvas lobbyCanvas;
    public Canvas inputCanvas;
    public InputField inputUserName;
    public InputField inputServer;
    public GameObject startGameButton;
    public GameObject exitGameButton;

    //Chat
    public Canvas chatCanvas;
    public Text chatText;
    public InputField inputChat;

    //Server/Client & other
    public Server server;
    public Client client;
    public GameObject gameplayScene;
    public List<PlayerNetInfo> clientList = new List<PlayerNetInfo>();

    //Cap framerate
    private void Start()
	{
        Application.targetFrameRate = 60;
	}

    //Create server
	public void Go2Create()
    {
        SceneManager.LoadScene(1);
    }

    //Join server
    public void Go2Join()
    {
        SceneManager.LoadScene(2);
    }

    //Scene where the user chooses if it hosts or joins a server
    public void Go2Choose()
    {
        SceneManager.LoadScene(0);
    }

    //Server: Write the name of the server
    //Client: Write the IP you want to connect
    public void ReadStringInputServer(string s)
    {
        inputServer.text = s;
        if(server)
		{
            server.serverName = s;
		}
        else
		{
            bool valid = true;

            string[] ipStringArr = s.Split('.');
            int[] ipIntArr = new int[ipStringArr.Length];
            if (ipIntArr.Length != 4)
            {
                Debug.LogError("ReadStringInputServer(): Not a valid IP address.");
                valid = false;
            }
            else
            {
                for (int i = 0; i < ipStringArr.Length; i++)
                {
                    ipIntArr[i] = Int32.Parse(ipStringArr[i]);
                    if (ipIntArr[i] < 0 || ipIntArr[i] > 255)
                    {
                        Debug.LogError("ReadStringInputServer(): Not a valid IP address.");
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                client.newServerIP = true;
                client.serverIP = s;
            }
            else
            {
                inputServer.text = "";
            }
        }
        Debug.Log("ReadStringInputServer(): New name: " + inputServer.text);
    }

    //Write the name of your username
    public void ReadStringInputUser(string s)
    {
        inputUserName.text = s;
        if (server)
        {
            server.hostUsername = s;
        }
        else
        {
            client.username = s;
        }
        Debug.Log("ReadStringInputUser(): Username: " + inputUserName.text);
    }

    //Create the server
    public void StartServer()
    {
        Debug.Log("StartServer(): Created server: " + inputServer.text);
        title.text = "Welcome to " + inputServer.text + "!\n IP: " + GetLocalIPv4();
        inputCanvas.GetComponent<Canvas>().enabled = false;
        chatCanvas.GetComponent<Canvas>().enabled = true;
        startGameButton.SetActive(true);
        exitGameButton.SetActive(true);
        chatText.text = "This is the beginning of the chat!";
        server.start = true;
    }

    //Join a server created
    public void JoinServer()
    {
        if (inputServer.text != "" && inputUserName.text != "")
        {
            Debug.Log("JoinServer(): Joined server: " + inputServer.text);
            title.text = "No server found...";
            inputCanvas.GetComponent<Canvas>().enabled = false;
            exitGameButton.SetActive(true);

            chatText.text = "This is the beginning of the chat!";
            client.start = true;
        }
        else
        {
            Debug.LogError("JoinServer(): Please input Username & IP");
        }
    }

    //Close a server created
    public void EndServer()
    {
        Debug.Log("EndServer(): Ending server.");
        clientList.Clear();

        inputCanvas.GetComponent<Canvas>().enabled = true;
        chatCanvas.GetComponent<Canvas>().enabled = false;
        startGameButton.SetActive(false);
        exitGameButton.SetActive(false);
        inputUserName.text = "";
        inputServer.text = "";
        inputChat.text = "";
        server.Close();
        title.text = "Host a server!";
        title.enabled = true;

        GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();
        gameplayScene.SetActive(false);
        manager.start = false;
        lobbyCamera.enabled = true;
        Cursor.lockState = CursorLockMode.None;

        lobbyCanvas.GetComponent<Canvas>().enabled = true;
    }

    //Leave the joined server
    public void LeaveServer()
    {
        Debug.Log("LeaveServer(): Leaving server.");
        title.text = "No server found...";
        inputCanvas.GetComponent<Canvas>().enabled = true;
        chatCanvas.GetComponent<Canvas>().enabled = false;
        exitGameButton.SetActive(false);
        inputUserName.text = "";
        inputServer.text = "";
        inputChat.text = "";
        clientList.Clear();

        client.Leave();
        title.text = "Join a server!";
        title.enabled = true;

        GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();

        foreach (SendReceive sr in manager.pScripts)
        {
            Destroy(sr.gameObject);
        }

        manager.pScripts.Clear();
        manager.playerList.Clear();

        gameplayScene.SetActive(false);
        manager.start = false;
        lobbyCamera.enabled = true;
        Cursor.lockState = CursorLockMode.None;

        lobbyCanvas.GetComponent<Canvas>().enabled = true;
    }

    //Called when game starts
    public void StartGame()
    {
        GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();
        gameplayScene.SetActive(true);
        title.enabled = false;
        manager.start = true;
        exitGameButton.SetActive(false);

        if (server)
        {
            server.SendPlayerList();
            manager.UserName = server.hostUsername;
            manager.UserUid = server.uid;
            startGameButton.SetActive(false);

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(false);
            writer.Write((byte)packetType.startGame);
            string msg = "\nStarting game...";
            writer.Write(msg);
            server.BroadcastServerInfo(stream);

            server.chatManager.SendMsg(msg);
        }

        Debug.Log("LobbyScripts(): Game scene enabled...");
    }

    //Show the IP of the player that creates the server
    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}