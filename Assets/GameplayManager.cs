using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public string UserName;
    public GameObject p1;
    public GameObject p2;
    public bool start, update = false;
    public LobbyScripts comunicationDevice;

    // Start is called before the first frame update
    void Start()
    {}

    // Update is called once per frame
    void Update()
    {
        if(start)
		{
            int c = comunicationDevice.usernameList.Count;
            Debug.Log("Start(): Player count: " + c);

            if (c != 0)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                Debug.Log("Start(): Player Models: " + players.Length);

                int i = 0;
                foreach (string userName in comunicationDevice.usernameList)
                {
                    //TODO: Reverse this
                    players[i].SetActive(true);
                    players[i].name = userName;
                    ++i;
                    if (i == c) break;
                }

                foreach (GameObject player in players)
                {
                    if (player.name != UserName)
                    {
                        player.GetComponent<CharacterController>().enabled = false;
                        player.GetComponent<MovementDebug>().enabled = false;
                        player.GetComponent<SendRecieve>().isControlling = false;
                    }
                    else
                    {
                        player.GetComponent<CharacterController>().enabled = true;
                        player.GetComponent<MovementDebug>().enabled = true;
                        player.GetComponent<SendRecieve>().isControlling = true;
                    }
                }

                switch (c)
                {
                    case 1:
                        p1.transform.localPosition = new Vector3(0.5f, 1.234f, 0.5f);
                        break;
                    case 2:
                        p1.transform.localPosition = new Vector3(0.0f, 1.234f, 0.0f);
                        p2.transform.localPosition = new Vector3(1.0f, 1.234f, 1.0f);
                        break;
                }
            }

            start = false;
            update = true;
		}
        if(update)
		{

		}
    }
}
