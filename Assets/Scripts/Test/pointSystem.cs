using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pointSystem : MonoBehaviour
{
    public Text firstPlayerText;
    public int firstPlayer = 0;
    public Text winnerText;
    public string firstPlayerUsername = "";
    float winnerTimer = 0.0f;
    public float winnerTime = 5.0f;
    List<PlayerHandler> playerList = new List<PlayerHandler>();

    void Start()
    {
        ListenForPlayers();
        Debug.Log(playerList.Count);
    }

    void Update()
    {
        //Show the points of the player with most points(left) & the points of your player(right)
        foreach (PlayerHandler lh in playerList)
		{
            if(lh.kills > firstPlayer)
			{
                firstPlayer = lh.kills;
                firstPlayerUsername = lh.username;
			}
		}
        firstPlayerText.text = firstPlayer.ToString();
        if(firstPlayer >= 25)
		{
            GameEnd();
		}
        if(winnerText.text != "")
		{
            winnerTimer += Time.deltaTime;
            if(winnerTimer >= winnerTime)
			{
                winnerText.text = "";
                winnerTimer = 0.0f;
			}
		}
    }

    //Shows who is the winner on screen
    void GameEnd()
	{
        winnerText.text = firstPlayerUsername + " wins the game!";
	}

    //Add players to the list to count them
    void ListenForPlayers()
	{
        GameObject[] tmp = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject go in tmp)
		{
            PlayerHandler h = go.GetComponent<PlayerHandler>();
            playerList.Add(h);
        }
    }
}
