using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pointSystem : MonoBehaviour
{
    public Text firstPlayerText;
    public int firstPlayer = 0;
    List<PlayerHandler> playerList = new List<PlayerHandler>();

    // Start is called before the first frame update
    void Start()
    {
        ListenForPlayers();
        Debug.Log(playerList.Count);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (PlayerHandler lh in playerList)
		{
            if(lh.kills > firstPlayer)
			{
                firstPlayer = lh.kills;
			}
		}
        firstPlayerText.text = firstPlayer.ToString();
    }

    //Function should be called at start and every time a player joins or leaves
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
