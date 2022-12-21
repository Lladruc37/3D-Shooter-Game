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
    public LayerMask ignoreRaycast;
    public LayerMask playerMask;
    public LayerMask environmentMask;
    public LayerMask ceilingMask;
    public Camera lobbyCamera;
    public Image hitMarkImage;
    Vector3 lp = Vector3.zero;

    //Threads
    public Thread sendThread = null;
    public Thread recieveThread = null;

    //Point system
    public int pointsToScore = 2;
    public int firstPlayer = 0;
    public string firstPlayerUsername = "";
    public float winnerTime = 5.0f;
    float winnerTimer = 0.0f;
    public Text firstPlayerText; //winning player
    public Text playerText; //you
    public bool win = false;
    public Text winnerText; //"you win" text
    public GameObject winnerBox;

    //Other UI
    Target targetScript;
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
                    GameObject player = CreateNewPlayer(user);
                    player.transform.localPosition = lp;
                }
            }
            Debug.Log("Start(): Player Models: " + playerList.Count);

            start = false;
            update = true;
		}

        if(update) //Updates point system & HP UI
		{
            hpText.text = "HP: " + targetScript.health.ToString();
            if (!win)
            {
                if (pScripts.Count < lobby.clientList.Count)
                {
                    foreach (PlayerNetInfo p in lobby.clientList)
                    {
                        if (!pScripts.Exists(sr => sr.uid == p.uid))
                        {
                            GameObject player = CreateNewPlayer(p);
                            player.transform.localPosition = lp;
                        }
                    }

                }
                else if (pScripts.Count > lobby.clientList.Count)
                {
                    foreach (SendRecieve sr in pScripts)
                    {
                        if (!lobby.clientList.Exists(p => p.uid == sr.uid))
                        {
                            GameObject gOremoved = sr.gameObject;
                            firstPlayer = 0;
                            pScripts.Remove(sr);
                            playerList.Remove(gOremoved);
                            Destroy(gOremoved);
                        }
                    }
                }
            }

            foreach (SendRecieve sr in pScripts)
            {
                if (sr.kills > firstPlayer)
                {
                    firstPlayer = sr.kills;
                    firstPlayerUsername = sr.username;
                }
                if (playerText && sr.uid == UserUid)
                {
                    playerText.text = sr.kills.ToString();
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
                    
                    lobby.title.enabled = true;
                    lobby.exitGameButton.SetActive(true);
                    if (server) lobby.startGameButton.SetActive(true);
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
        pScripts.Clear();
        firstPlayer = 0;
        playerText.text = 0.ToString();
        lobbyCamera.enabled = true;
        foreach (GameObject gO in playerList)
        {
            Destroy(gO);
        }
        playerList.Clear();
    }

    private void OnDrawGizmos()
    {
        DrawCylinder(lp, Quaternion.identity, 350, 1);
    }
    public static void DrawCylinder(Vector3 position, Quaternion orientation, float height, float radius)
    {
        Vector3 localUp = orientation * Vector3.up;
        Vector3 localRight = orientation * Vector3.right;
        Vector3 localForward = orientation * Vector3.forward;

        Vector3 basePosition = position;
        Vector3 topPosition = basePosition + localUp * height;

        Vector3 pointA = basePosition + localRight * radius;
        Vector3 pointB = basePosition + localForward * radius;
        Vector3 pointC = basePosition - localRight * radius;
        Vector3 pointD = basePosition - localForward * radius;

        Gizmos.DrawLine(pointA, pointA + (localUp * height));
        Gizmos.DrawLine(pointC, pointC + (localUp * height));
        Gizmos.DrawLine(pointD, pointD + (localUp * height));
        Gizmos.DrawLine(pointB, pointB + (localUp * height));

        Gizmos.DrawSphere(basePosition, radius);
        Gizmos.DrawSphere(topPosition, radius);
    }

    // Works with actual position not local position
    GameObject CreateNewPlayer(PlayerNetInfo u)
    {
        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(0, 1.234f, 0), Quaternion.identity, this.transform);
        newPlayer.layer = ignoreRaycast;

        Debug.Log("CreateNewPlayer(): Initial Position: " + newPlayer.transform.localPosition);

        newPlayer.transform.localPosition = new Vector3(UnityEngine.Random.Range(-115.0f, 65.0f), 1.234f, UnityEngine.Random.Range(-105.0f, 75.0f));
        lp = newPlayer.transform.localPosition;
        bool playersHit = Physics.CheckSphere(lp, 35.0f, playerMask);
        bool ceilingHit = Physics.CheckCapsule(lp, lp + new Vector3(0, 350, 0), 1.0f, ceilingMask);

        Debug.Log("CreateNewPlayer(): Hit player: " + playersHit + ", Hit Ceiling: " + ceilingHit);
        while (playersHit || ceilingHit)
        {
            Debug.Log("CreateNewPlayer(): Updating position...");
            newPlayer.transform.localPosition = new Vector3(UnityEngine.Random.Range(-115.0f, 65.0f), 1.234f, UnityEngine.Random.Range(-105.0f, 75.0f));
            lp = newPlayer.transform.localPosition;
            playersHit = Physics.CheckSphere(lp, 35.0f, playerMask);
            ceilingHit = Physics.CheckCapsule(lp, lp + new Vector3(0, 350, 0), 1.0f, ceilingMask);
            Debug.Log("CreateNewPlayer(): Hit player: " + playersHit + ", Hit Ceiling: " + ceilingHit);
        }
        Debug.Log("CreateNewPlayer(): Final Position: " + newPlayer.transform.localPosition);

        newPlayer.layer = LayerMask.NameToLayer("Players");
        newPlayer.name = u.username;

        SendRecieve sr = newPlayer.GetComponent<SendRecieve>();
        sr.uid = u.uid;
        sr.gm = this;
        sr.updateCharacter = true;
        sr.position = lp;
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

        if (u.uid == UserUid)
        {
            SetupPlayer(newPlayer);
        }
        else
        {
            SetupOtherPlayer(newPlayer);
        }

        playerList.Add(newPlayer);
        return newPlayer;
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
        targetScript = player.GetComponent<Target>();
        Camera[] cameras = player.GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            camera.enabled = true;
        }
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
