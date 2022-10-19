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
    public Canvas inputCanvas;
    public Canvas chatCanvas;
    public Server server;
    public Client client;

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
        Debug.Log("New name: " + inputServer.text);
    }

    public void ReadStringInputUser(string s)
    {
        inputUserName.text = s;
        Debug.Log("Username: " + inputUserName.text);
    }

    public void StartServer()
    {
        Debug.Log("Created server: " + inputServer.text);
        title.text = "Welcome to " + inputServer.text + "! IP: " + GetLocalIPv4();
        server.start = true;
        inputCanvas.GetComponent<Canvas>().enabled = false;
        chatCanvas.GetComponent<Canvas>().enabled = true;
    }

    public void JoinServer()
    {
        Debug.Log("Joined server: " + inputServer.text);
        title.text = "Welcome to " + "" + "! IP: " + inputServer.text;
        client.start = true;
        inputCanvas.GetComponent<Canvas>().enabled = false;
        chatCanvas.GetComponent<Canvas>().enabled = true;
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}