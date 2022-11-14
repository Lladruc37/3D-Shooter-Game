﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Chat : MonoBehaviour
{
    public Text txt;
    public GameObject chatText;
    public GameObject chatInput;
    public LobbyScripts lobby;
    public InputField input;
    // Start is called before the first frame update
    void Start()
    {}
    // Update is called once per frame
    void Update()
    {
        if (!chatInput) input.text = "";
    }
    public void SendMsg(string msg/*,bool isServer = false*/)
    {
        txt.text += /*"\n" +*/ msg;
    }
    public void ToggleChat(bool toggle)
    {
        input.text = "";
        chatText.SetActive(toggle);
        chatInput.SetActive(toggle);
    }

}