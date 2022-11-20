using System.Collections;
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

    //Empties the message line if it's not empty
    void Update()
    {
        if (!chatInput) input.text = "";
    }

    //Adds the message to the chat
    public void SendMsg(string msg)
    {
        txt.text += msg;
    }

    //For toggling the chat on & off
    public void ToggleChat(bool toggle)
    {
        input.text = "";
        chatText.SetActive(toggle);
        chatInput.SetActive(toggle);
    }
}