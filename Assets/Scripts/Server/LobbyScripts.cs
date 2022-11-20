using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class LobbyScripts : MonoBehaviour
{
    //UI
    public Camera lobbyCamera;
    public Canvas lobbyCanvas;
    public Text title;

    public Canvas inputCanvas;
    public InputField inputUserName;
    public InputField inputServer;

    //Chat
    public Canvas chatCanvas;
    public Text chatText;
    public InputField inputChat;

    public GameObject startGameButton;
    public GameObject exitGameButton;

    public Server server;
    public Client client;
    public GameObject gameplayScene;
    public Dictionary<uint,string> usersList = new Dictionary<uint, string>();

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

    //Write the name of the server
    public void ReadStringInputServer(string s)
    {
        inputServer.text = s;
        if(server)
		{
            server.serverName = s;
		}
        else
		{
            client.newServerIP = true;
            client.serverIP = s;
		}
        Debug.Log("ReadStringInputServer(): New name: " + inputServer.text);
    }

    //Write the name of the user
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
        Debug.Log("JoinServer(): Joined server: " + inputServer.text);
        title.text = "No server found...";
        inputCanvas.GetComponent<Canvas>().enabled = false;
        exitGameButton.SetActive(true);
        
        chatText.text = "This is the beginning of the chat!";
        client.start = true;
    }

    //Close a server created
    public void EndServer()
    {
        Debug.Log("EndServer(): Ending server.");
        string msg = "/>endsession</Ending session...";
        server.BroadcastServerMessage(server.ManageMessage(msg, true, true));
        inputCanvas.GetComponent<Canvas>().enabled = true;
        chatCanvas.GetComponent<Canvas>().enabled = false;
        startGameButton.SetActive(false);
        exitGameButton.SetActive(false);
        inputUserName.text = "";
        inputServer.text = "";
        inputChat.text = "";
        server.Close();
        title.text = "Host a server!";

        GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();
        gameplayScene.SetActive(false);
        manager.start = false;
        lobbyCamera.enabled = true;
        Cursor.lockState = CursorLockMode.None;

        lobbyCanvas.GetComponent<Canvas>().enabled = true;
    }

    //Leave the server where the player is
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
        client.Send("/>goodbye</"+ client.uuid);
        client.Leave();
        title.text = "Join a server!";

        GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();
        gameplayScene.SetActive(false);
        manager.start = false;
        lobbyCamera.enabled = true;
        Cursor.lockState = CursorLockMode.None;

        lobbyCanvas.GetComponent<Canvas>().enabled = true;
    }

    //Button that starts the game
    public void StartGame()
    {
        GameplayManager manager = gameplayScene.GetComponent<GameplayManager>();
        lobbyCanvas.GetComponent<Canvas>().enabled = false;
        gameplayScene.SetActive(true);
        exitGameButton.SetActive(false);
        manager.start = true;

        if (server)
        {
            server.SendPlayerList();
            manager.UserName = server.hostUsername;
            string msg = "/>startgame</Starting game...";
            server.BroadcastServerMessage(server.ManageMessage(msg, true));
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