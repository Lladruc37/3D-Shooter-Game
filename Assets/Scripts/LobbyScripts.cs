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
    public Text title;
    public Text inputUserName;
    public Text inputServer;
    public GameObject gamePlayScene;
    public Canvas inputCanvas;
    public Canvas chatCanvas;
    public Server server;
    public Client client;
	//public string serverName = "";

	private void Start()
	{
        Application.targetFrameRate = 60;
	}
	public void Go2Create()
    {
        SceneManager.LoadScene(1);
    }
    public void Go2Join()
    {
        SceneManager.LoadScene(2);
    }
    public void ReadStringInputServer(string s)
    {
        inputServer.text = s;
        if(server)
		{
            server.ServerUsername = s;
		}
        else
		{
            client.newServerIP = true;
		}
        Debug.Log("ReadStringInputServer(): New name: " + inputServer.text);
    }

    public void ReadStringInputUser(string s)
    {
        inputUserName.text = s;
        Debug.Log("ReadStringInputUser(): Username: " + inputUserName.text);
    }

    public void StartServer()
    {
        Debug.Log("StartServer(): Created server: " + inputServer.text);
        title.text = "Welcome to " + inputServer.text + "!\n IP: " + GetLocalIPv4();
        inputCanvas.GetComponent<Canvas>().enabled = false;
        chatCanvas.GetComponent<Canvas>().enabled = true;
        gamePlayScene.SetActive(true);
        server.start = true;
    }

    public void JoinServer()
    {
        Debug.Log("JoinServer(): Joined server: " + inputServer.text);
        title.text = "No server found..." /*IP:  + inputServer.text*/;
        inputCanvas.GetComponent<Canvas>().enabled = false;
        gamePlayScene.SetActive(true);
        client.start = true;
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}