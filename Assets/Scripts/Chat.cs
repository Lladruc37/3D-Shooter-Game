using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Chat : MonoBehaviour
{
    public Text txt;
    public LobbyScripts lobby;
    public InputField input;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (input.text != "")
            {
                string msg = input.text;
                input.text = "";

                SendMsg(msg);
            }
        }
    }
    void SendMsg(string msg)
    {
        //This is where user should send message to server
        //Should also send userName

        RecieveMsg(msg);
    }

    void RecieveMsg(string msg)
    {
        //This is where server sends messages to users
        txt.text += "\n[" + lobby.inputUserName.text + "]>>" + msg;
    }
}