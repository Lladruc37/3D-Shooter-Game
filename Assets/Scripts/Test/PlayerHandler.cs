using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHandler : MonoBehaviour
{
    public string username = "default player";
    public uint uid = 0;
    public int kills = 0;
    public Text playerText;

    void Update()
    {
        if (playerText)
        {
            playerText.text = kills.ToString();
        }
    }
}
