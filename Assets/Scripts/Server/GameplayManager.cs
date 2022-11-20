using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    public uint UserUid;
    public string UserName;

    Target playerHp;
    public Text playerText;
    public Text hpText;

    public bool start, update = false;
    public float groundLevel = 1.234f;
    public LobbyScripts comunicationDevice;
    public GameObject p1, p2, p3, p4;
    public GameObject[] playerList;

    public byte[] data;
    public Thread sendThread = null;
    public Thread recieveThread = null;
    public Server server;
    public Client client;
    public List<SendRecieve> pScripts;

    public Text firstPlayerText;
    public GameObject winnerBox;
    public Text winnerText;
    public int firstPlayer = 0;
    public string firstPlayerUsername = "";
    float winnerTimer = 0.0f;
    public float winnerTime = 5.0f;
    //bool kill = false;
    //int lastKills = 0;

    // Start is called before the first frame update
    void Start()
    {}

    // Update is called once per frame
    void Update()
    {
        if (start)
		{
            int c = comunicationDevice.usersList.Count;
            Debug.Log("Start(): Player count: " + c);

            if (c != 0)
            {
                //TODO: INSTANTIATE PLAYERS & ADD RANDOM UID
                playerList = new GameObject[] { p1, p2, p3, p4 };
                Debug.Log("Start(): Player Models: " + playerList.Length);

                int i = 0;
                foreach (KeyValuePair<uint,string> u in comunicationDevice.usersList)
                {
                    Debug.Log("Start(): Adding pScripts, values: " + u.Key + " - " + u.Value);
                    playerList[i].name = u.Value;
                    pScripts.Add(playerList[i].GetComponent<SendRecieve>());
                    playerList[i].GetComponent<SendRecieve>().assigned = true;
                    playerList[i].GetComponent<SendRecieve>().uid = u.Key;
                    ++i;
                }

                foreach (GameObject player in playerList)
                {
                    if (player.GetComponent<SendRecieve>().assigned)
                    {
                        if (player.name == UserName)
                        {
                            SetupPlayer(player);
                        }
                        else
                        {
                            SetupOtherPlayer(player);
                        }
                    }
                    else
					{
                        player.SetActive(false);
					}
                }

                InitializePosition(c);
            }

            start = false;
            update = true;
		}

        if(update)
		{
            hpText.text = "HP: " + playerHp.health.ToString();
            foreach (SendRecieve p in pScripts)
            {
                if (p.kills > firstPlayer)
                {
                    firstPlayer = p.kills;
                    firstPlayerUsername = p.username;
                }
                if (playerText && p.uid == UserUid)
                {
                    //if (p.kills != lastKills)
                    //{
                    //    kill = true;
                    //    lastKills = p.kills;
                    //    Debug.Log("KILL COUNTER UP");
                    //}
                    playerText.text = p.kills.ToString();
                }
            }
            firstPlayerText.text = firstPlayer.ToString();
            if (firstPlayer >= 25)
            {
                GameEnd();
            }
            if (winnerText.text != "")
            {
                winnerTimer += Time.deltaTime;
                if (winnerTimer >= winnerTime)
                {
                    winnerText.text = "";
                    winnerTimer = 0.0f;
                }
            }
        }
    }

    void GameEnd() //TODO: Return to lobby + X player wins
    {
        winnerBox.SetActive(true);
        winnerText.text = firstPlayerUsername + " wins the game!";
    }

    void SetupOtherPlayer(GameObject player)
	{
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<CapsuleCollider>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<SendRecieve>().isControlling = false;
        player.GetComponentInChildren<MouseLook>().enabled = false;
        player.GetComponentInChildren<Gun>().isControllingGun = false;
        Camera[] cameras = player.GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
        }
    }

    void SetupPlayer(GameObject player)
	{
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<SendRecieve>().isControlling = true;
        player.GetComponentInChildren<MouseLook>().enabled = true;
        player.GetComponentInChildren<MouseLook>().start = true;
        player.GetComponentInChildren<Gun>().isControllingGun = true;
        playerHp = player.GetComponent<Target>();
        Camera[] cameras = player.GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            camera.enabled = true;
        }
        UserUid = player.GetComponent<SendRecieve>().uid;
    }

    void InitializePosition(int c)
	{
        switch (c)
        {
            //TODO: Bug Start Position
            case 1:
                {
                    p1.transform.localPosition = new Vector3(-15.0f, groundLevel, -1.0f);
                    break;
                }
            case 2:
                {
                    p1.transform.localPosition = new Vector3(-15.0f, groundLevel, 90.0f);
                    p2.transform.localPosition = new Vector3(-15.0f, groundLevel, -105.0f);
                    break;
                }
            case 3:
                {
                    p1.transform.localPosition = new Vector3(-15.0f, groundLevel, 90.0f);
                    p2.transform.localPosition = new Vector3(-15.0f, groundLevel, -105.0f);
                    p3.transform.localPosition = new Vector3(-110.0f, groundLevel, -15.0f);
                    break;
                }
            case 4:
                {
                    p1.transform.localPosition = new Vector3(-15.0f, groundLevel, 90.0f);
                    p2.transform.localPosition = new Vector3(-15.0f, groundLevel, -105.0f);
                    p3.transform.localPosition = new Vector3(-110.0f, groundLevel, -15.0f);
                    p4.transform.localPosition = new Vector3(80.0f, groundLevel, -15.0f);
                    break;
                }
        }

    }

    public void SendGameState() //YOU SEND INFO
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write("/>PlayerInfo:");
        writer.Write(UserUid);
        writer.Write(UserName);

        //Position
        foreach (SendRecieve p in pScripts)
        {
            if (p.uid == UserUid)
            {
                //writer.Write(kill);
                //if (kill)
                //{
                //    uint killcount = 0;
                //    List<uint> kills = new List<uint>();
                //    foreach (SendRecieve k in pScripts)
                //    {
                //        if (k.target.health <= 0)
                //        {
                //            kills.Add(k.uid);
                //            killcount++;
                //        }
                //    }
                //    writer.Write(killcount);
                //    foreach (uint k in kills)
                //    {
                //        writer.Write(k);
                //    }
                //    kill = false;
                //}

                //Health
                writer.Write(p.target.health);
                writer.Write(p.kills);

                //Position
                writer.Write((double)p.position.x);
                writer.Write((double)p.position.y);
                writer.Write((double)p.position.z);

                //Rotation
                writer.Write((double)p.rotation.x);
                writer.Write((double)p.rotation.y);
                writer.Write((double)p.rotation.z);

                //Weapon Action
                writer.Write(p.gun.fire);
                writer.Write((double)p.gunDirection.xRotacion);
                writer.Write(p.uidHit);
                p.uidHit = -1;

                break;
            }
        }

        Debug.Log("SendGameState(): Sending serialized data...");
        if (server)
        {
            server.BroadcastServerInfo(stream);
        }
        else if (client)
        {
            client.SendInfo(stream);
        }

        //TODO: Temporary solution
        Thread.Sleep(100);
    }

    public void RecieveGameState() //GATHER OTHERS INFO
    {
        Debug.Log("RecieveGameState(" + UserUid + "): Recieved info");
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        string header = reader.ReadString();
        Debug.Log("RecieveGameState(" + UserUid + "): Header is " + header);

        uint uid = reader.ReadUInt32();
        string dump = reader.ReadString();
        Debug.Log(dump + " - " + UserName);

        SendRecieve pSender = null;
        foreach (SendRecieve p in pScripts)
        {
            if (p.uid == uid && uid != UserUid)
            {
                pSender = p;
                break;
            }
        }
        //bool kill = reader.ReadBoolean();
        //if (kill)
        //{
        //    uint count = reader.ReadUInt32();
        //    List<uint> kills = new List<uint>();
        //    for (int i = 0; i > count; ++i)
        //    {
        //        uint k = reader.ReadUInt32();
        //        kills.Add(k);
        //    }
        //    foreach (SendRecieve k in pScripts)
        //    {
        //        if (kills.Contains(k.uid))
        //        {
        //            k.target.health = 0;
        //        }
        //    }
        //}

        if (pSender != null)
        {
            //Health
            pSender.target.health = reader.ReadInt32();
            pSender.kills = reader.ReadInt32();

            //Position
            pSender.position.x = (float)reader.ReadDouble();
            pSender.position.y = (float)reader.ReadDouble();
            pSender.position.z = (float)reader.ReadDouble();

            //Rotation
            pSender.rotation.x = (float)reader.ReadDouble();
            pSender.rotation.y = (float)reader.ReadDouble();
            pSender.rotation.z = (float)reader.ReadDouble();

            //Weapon Action
            pSender.gun.fire = reader.ReadBoolean();
            pSender.gunDirection.xRotacion = (float)reader.ReadDouble();

            //Debug.Log("RecieveGameState(" + UserUid + "): New position: " + p.position + "with uid: " + p.uid);
            pSender.updateCharacter = true;

            int _uidHit = reader.ReadInt32();
            if (UserUid == _uidHit)
            {
                foreach (SendRecieve p in pScripts)
                {
                    if (p.uid == UserUid)
                    {
                        p.target.takeDamage(1);
                        break;
                    }
                }
            }
        }

        data = null;

        //TODO: Temporary solution
        Thread.Sleep(100);
    }
}
