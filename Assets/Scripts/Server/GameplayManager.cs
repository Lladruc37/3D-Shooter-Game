using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

//Message Management
public enum packetType
{
    //Default
    error = -1,
    ping, //Regular ping

    //Client
    hello, //Ping to enter server
    chat, //Chat message
    goodbye, //Leave server
    playerInfo, //Update user

    //Server
    servername, //Confirm ping with server name
    list, //Player list
    startGame, //Start game
    endSession, //End server/game
}

public class PlayerNetInfo
{
    public uint uid;
    public string username;
    public IPEndPoint ip;

    public PlayerNetInfo() { }
    public PlayerNetInfo(uint _uid, string _username, IPEndPoint _ip)
    {
        uid = _uid;
        username = _username;
        ip = _ip;
    }
}
public class GameplayManager : MonoBehaviour
{
    //Server/Client & other
    public LobbyScripts lobby;
    public Server server;
    public Client client;
    public byte[] data;

    //Instantiate players
    public GameObject playerPrefab;
    public LayerMask playerMask;
    public LayerMask environmentMask;
    public Camera lobbyCamera;
    public Image hitMarkImage;

    //Threads
    public Thread sendThread = null;
    public Thread recieveThread = null;

    //Point system
    public int pointsToScore = 5;
    public int firstPlayer = 0;
    public string firstPlayerUsername = "";
    public float winnerTime = 5.0f;
    float winnerTimer = 0.0f;
    public Text firstPlayerText; //winning player
    public Text playerText; //you
    bool win = false;
    public Text winnerText; //"you win" text
    public GameObject winnerBox;

    //Other UI
    Target playerHp;
    public Text hpText;

    //User data
    public uint UserUid;
    public string UserName;
    public bool start, update = false;

    //All users data
    public List<GameObject> playerList = new List<GameObject>();
    public List<SendRecieve> pScripts;
    public float groundLevel = 1.234f;

    void Update()
    {
        if (start)
		{
            Application.targetFrameRate = 60;
            int c = lobby.clientList.Count;
            Debug.Log("Start(): Player count: " + c);

            if (c != 0) //Setup gameplay scene
            {
                //TODO: INSTANTIATE PLAYERS
                playerList.Clear();
                pScripts.Clear();
                foreach (PlayerNetInfo user in lobby.clientList) //Add players to the list
                {
                    Debug.Log("Start(): Adding pScripts, values: " + user.uid + " - " + user.username);
                    CreateNewPlayer(user);
                }
            }
            Debug.Log("Start(): Player Models: " + playerList.Count);

            start = false;
            update = true;
		}

        if(update) //Updates point system & HP UI
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
                    playerText.text = p.kills.ToString();
                }
            }
            firstPlayerText.text = firstPlayer.ToString();
            if (firstPlayer >= pointsToScore) //If a player reaches the goal the game ends
            {
                GameEnd();
            }
            if (win) //Timer for the winning screen
            {
                winnerTimer += Time.deltaTime;
                if (winnerTimer >= winnerTime) //Return to lobby
                {
                    win = false;
                    winnerText.text = "";
                    winnerTimer = 0.0f;
                    update = false;
                    winnerBox.SetActive(false);
                    lobby.gameplayScene.SetActive(false);
                    lobby.exitGameButton.SetActive(true);
                    lobby.lobbyCanvas.GetComponent<Canvas>().enabled = true;
                }
            }
        }
    }

    //End game setup
    void GameEnd()
    {
        Debug.Log("GameEnd(): Game finished! Kills: " + firstPlayer);
        winnerBox.SetActive(true);
        winnerText.text = firstPlayerUsername + " wins the game!";
        win = true;
        Cursor.lockState = CursorLockMode.None;
        winnerTimer = 0.0f;
        foreach (SendRecieve p in pScripts)
		{
            if (firstPlayer == p.kills) SendGameState();
        }
        firstPlayer = 0;
        lobby.lobbyCamera.enabled = true;
        foreach (GameObject gO in playerList)
        {
            Destroy(gO);
        }
        playerList.Clear();
        pScripts.Clear();
    }

    void CreateNewPlayer(PlayerNetInfo u)
    {
        GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        newPlayer.transform.parent = this.transform;

        newPlayer.transform.localPosition = new Vector3(UnityEngine.Random.Range(-115.0f, 65.0f), 1.234f, UnityEngine.Random.Range(-105.0f, 75.0f));
        while (Physics.CheckSphere(newPlayer.transform.localPosition, 35.0f, playerMask) && Physics.CheckSphere(newPlayer.transform.localPosition, 1.0f, environmentMask))
        {
            newPlayer.transform.localPosition = new Vector3(UnityEngine.Random.Range(-115.0f, 65.0f), 1.234f, UnityEngine.Random.Range(-105.0f, 75.0f));
        }

        newPlayer.layer = LayerMask.NameToLayer("Players");
        newPlayer.name = u.username;

        SendRecieve sr = newPlayer.GetComponent<SendRecieve>();
        sr.uid = u.uid;
        sr.gm = this;
        pScripts.Add(sr);

        Target t = newPlayer.GetComponent<Target>();
        t.bodyMesh.enabled = true;
        t.gunBarrelMesh.enabled = true;
        t.gun.enabled = true;
        t.deathBoxUI = winnerBox;
        t.deathText = winnerText;

        MouseLook ml = newPlayer.GetComponentInChildren<MouseLook>();
        ml.lobbyCamera = lobbyCamera;

        Gun g = newPlayer.GetComponentInChildren<Gun>();
        g.hitMark = hitMarkImage;

        if (newPlayer.name == UserName)
        {
            SetupPlayer(newPlayer);
        }
        else
        {
            SetupOtherPlayer(newPlayer);
        }

        playerList.Add(newPlayer);
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

    public void SendGameState() //YOU SEND YOUR INFO
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write(false);
        writer.Write((byte) packetType.playerInfo);
        writer.Write(UserUid);
        writer.Write(UserName);

        //Position
        foreach (SendRecieve p in pScripts)
        {
            if (p.uid == UserUid)
            {
                //Health & Kills
                writer.Write(p.target.health);
                writer.Write(p.kills);

                //Position
                ushort x = ConvertToFixed(p.position.x, -130f,0.01f), y = ConvertToFixed(p.position.y, -130f,0.01f), z = ConvertToFixed(p.position.z, -130f,0.01f);
                writer.Write(x);
                writer.Write(z);
                if (p.position.y == groundLevel)
                {
                    writer.Write(true);
                }
                else
                {
                    writer.Write(false);
                    writer.Write(y);
                }

                //Rotation
                y = ConvertToFixed(p.rotation.y / 360.0f, 0.0f, 0.0001f);
                writer.Write(y);

                //Weapon Action
                writer.Write(p.gun.fire);
                x = ConvertToFixed(p.gunDirection.xRotation / 90.0f, -1f, 0.0001f);
                writer.Write(x);
                writer.Write(p.uidHit);
                p.uidHit = -1;

                break;
            }
        }

        //Debug.Log("SendGameState(): Sending serialized data...");
        if (server)
        {
            server.BroadcastServerInfo(stream);
        }
        else if (client)
        {
            client.SendInfo(stream);
        }
        Thread.Sleep(1);
    }

    public void RecieveGameState() //GATHER OTHERS INFO
    {
        //Debug.Log("RecieveGameState(" + UserUid + "): Recieved info");
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        reader.ReadBoolean();
        short header = reader.ReadByte();
        packetType type = (packetType)header;
        //Debug.Log("RecieveGameState(" + UserUid + "): Header is " + type.ToString());

        uint uid = reader.ReadUInt32();
        string dump = reader.ReadString();
        //Debug.Log(dump + " - " + UserName);

        SendRecieve pSender = null;
        foreach (SendRecieve p in pScripts)
        {
            if (p.uid == uid && uid != UserUid)
            {
                pSender = p;
                break;
            }
        }

        if (pSender != null)
        {
            //Health & Kills
            pSender.target.health = reader.ReadInt32();
            pSender.kills = reader.ReadInt32();

            //Position
            pSender.position.x = ConvertFromFixed(reader.ReadUInt16(),-130f,0.01f);
            pSender.position.z = ConvertFromFixed(reader.ReadUInt16(), -130f,0.01f);
            if(reader.ReadBoolean())
			{
                pSender.position.y = groundLevel;
			}
            else
			{
                pSender.position.y = ConvertFromFixed(reader.ReadUInt16(), -130f,0.01f);
			}

            //Rotation
            pSender.rotation.y = ConvertFromFixed(reader.ReadUInt16(), 0.0f, 0.0001f) * 360.0f;

            //Weapon Action
            pSender.gun.fire = reader.ReadBoolean();
            pSender.gunDirection.xRotation = ConvertFromFixed(reader.ReadUInt16(), -1f, 0.0001f) * 90.0f;

            pSender.updateCharacter = true;

            //User who got hit
            int _uidHit = reader.ReadInt32();
            Debug.Log("Hit player: " + _uidHit);
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
        Thread.Sleep(1);
    }

    //for positions & rotations
    // 0.01 & 0.0001 precision respectively
    UInt16 ConvertToFixed(float inNumber, float inMin, float inPrecision)
	{
        return (UInt16)((inNumber - inMin) /inPrecision);
	}

    float ConvertFromFixed(UInt16 inNumber, float inMin, float inPrecision)
	{
        return (float)(inNumber * inPrecision) + inMin;
	}
}
