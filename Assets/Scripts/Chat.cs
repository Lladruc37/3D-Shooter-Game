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
        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    if (input.text != "")
        //    {
        //        string msg = input.text;
        //        input.text = "";

        //        SendMsg(msg);
        //    }
        //}
    }
    public void SendMsg(string msg/*,bool isServer = false*/)
    {
        //This is where user should send message to server
        //Should also send userName

        RecieveMsg(msg/*,isServer*/);
    }

    void RecieveMsg(string msg/*, bool isServer = false*/)
    {
        //This is where server sends messages to users
        //if (isServer)
        //{
        //    txt.text += "\n" + msg;
        //}
        //else
        //{
        //    txt.text += "\n[" + lobby.inputUserName.text + "]>>" + msg;
        //}

        txt.text += /*"\n" +*/ msg;
    }
}