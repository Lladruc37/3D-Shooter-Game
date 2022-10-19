using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyScripts : MonoBehaviour
{
    //UI
    public Text title;
    public Text input;
    public Canvas inputCanvas;
    public Canvas chatCanvas;

    public void Go2Create()
    {
        SceneManager.LoadScene(1);
    }
    public void Go2Join()
    {
        SceneManager.LoadScene(2);
    }
    public void ReadStringInput(string s)
    {
        input.text = s;
        Debug.Log("New name: " + input.text);
    }

    public void StartServer()
    {
        Debug.Log("Created server: " + input.text);
        title.text = "Welcome to " + input.text + "! IP: " + "";
        inputCanvas.GetComponent<Canvas>().enabled = false;
        chatCanvas.GetComponent<Canvas>().enabled = true;
    }

    public void JoinServer()
    {
        Debug.Log("Joined server: " + input.text);
        title.text = "Welcome to " + "" + "! IP: " + input.text;
        inputCanvas.GetComponent<Canvas>().enabled = false;
        chatCanvas.GetComponent<Canvas>().enabled = true;
    }
}
