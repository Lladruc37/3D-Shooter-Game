using System;
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

    //Threads
    public Thread sendThread = null;
    public Thread receiveThread = null;

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
    public List<SendReceive> pScripts;
    public float groundLevel = 1.234f;
    public List<Vector3> spawnpoints = new List<Vector3>();

    void Update()
    {
        if (start)
		{
            spawnpoints.Add(new Vector3(5.0f, groundLevel, -30.0f));
            spawnpoints.Add(new Vector3(-77.0f, groundLevel, 6.0f));
            spawnpoints.Add(new Vector3(-105.0f, groundLevel, 75.0f));
            spawnpoints.Add(new Vector3(-40.0f, groundLevel, 45.0f));
            spawnpoints.Add(new Vector3(60.0f, groundLevel, 75.0f));
            spawnpoints.Add(new Vector3(13.0f, groundLevel, 25.0f));
            spawnpoints.Add(new Vector3(65.0f, groundLevel, -95.0f));
            spawnpoints.Add(new Vector3(-16.0f, groundLevel, -81.0f));
            spawnpoints.Add(new Vector3(-105.0f, groundLevel, -105.0f));
            spawnpoints.Add(new Vector3(-100.0f, groundLevel, -50.0f));
            spawnpoints.Add(new Vector3(-20.0f, groundLevel, 10.0f));
            spawnpoints.Add(new Vector3(47.0f, 50.0f, -2.0f));
            spawnpoints.Add(new Vector3(-15.0f, 50.0f, 50.0f));
            spawnpoints.Add(new Vector3(-44.0f, 50.0f, -55.0f));
            spawnpoints.Add(new Vector3(-95.0f, 78.0f, -86.0f));
            spawnpoints.Add(new Vector3(10.0f, 78.0f, -86.0f));

            Application.targetFrameRate = 60;
            int c = lobby.clientList.Count;
            Debug.Log("Start(): Player count: " + c);

            if (c != 0) //Setup gameplay scene
            {
                playerList.Clear();
                pScripts.Clear();
                foreach (PlayerNetInfo user in lobby.clientList) //Add players to the list & instantiates them in the world
                {
                    Debug.Log("Start(): Adding pScripts, values: " + user.uid + " - " + user.username);
                    GameObject player = CreateNewPlayer(user);
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
                if (pScripts.Count < lobby.clientList.Count) //In case player joins mid-game
                {
                    foreach (PlayerNetInfo p in lobby.clientList)
                    {
                        if (!pScripts.Exists(sr => sr.uid == p.uid))
                        {
                            GameObject player = CreateNewPlayer(p);
                        }
                    }

                }
                else if (pScripts.Count > lobby.clientList.Count) //In case player leaves mid-game
                {
                    foreach (SendReceive sr in pScripts)
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
                foreach (SendReceive sr in pScripts) //Point system & UI
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
            }
            else //Timer for the winning screen
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
        hitMarkImage.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        winnerTimer = 0.0f;
        foreach (SendReceive p in pScripts)
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

    ////Debug draw cylinder (automated part)
    //private void OnDrawGizmos()
    //{
    //    DrawSphere(lp, Quaternion.identity, 350, 1);
    //}

    ////Debug draw cylinder (points & lines part)
    //public static void DrawCylinder(Vector3 position, Quaternion orientation, float height, float radius)
    //{
    //    Vector3 localUp = orientation * Vector3.up;
    //    Vector3 localRight = orientation * Vector3.right;
    //    Vector3 localForward = orientation * Vector3.forward;

    //    Vector3 basePosition = position;
    //    Vector3 topPosition = basePosition + localUp * height;

    //    Vector3 pointA = basePosition + localRight * radius;
    //    Vector3 pointB = basePosition + localForward * radius;
    //    Vector3 pointC = basePosition - localRight * radius;
    //    Vector3 pointD = basePosition - localForward * radius;

    //    Gizmos.DrawLine(pointA, pointA + (localUp * height));
    //    Gizmos.DrawLine(pointC, pointC + (localUp * height));
    //    Gizmos.DrawLine(pointD, pointD + (localUp * height));
    //    Gizmos.DrawLine(pointB, pointB + (localUp * height));

    //    Gizmos.DrawSphere(basePosition, radius);
    //    Gizmos.DrawSphere(topPosition, radius);
    //}

    //Instantiates new player given player info
    GameObject CreateNewPlayer(PlayerNetInfo u)
    {
        List<int> blacklistedSpawns = new List<int>();
        int randomSpawnIndex = UnityEngine.Random.Range(0, 15);
        bool collide = Physics.CheckSphere(spawnpoints[randomSpawnIndex],35.0f,playerMask);
        while (collide)
        {
            blacklistedSpawns.Add(randomSpawnIndex);
            randomSpawnIndex = UnityEngine.Random.Range(0, 15);
            while (blacklistedSpawns.Contains(randomSpawnIndex))
            {
                randomSpawnIndex = UnityEngine.Random.Range(0, 15);
            }
            collide = Physics.CheckSphere(spawnpoints[randomSpawnIndex], 35.0f, playerMask);
        }

        GameObject newPlayer = Instantiate(playerPrefab, spawnpoints[randomSpawnIndex], Quaternion.identity/*, this.transform*/);
        newPlayer.layer = ignoreRaycast;

        Debug.Log("CreateNewPlayer(): Initial Position: " + newPlayer.transform.localPosition);

        newPlayer.layer = LayerMask.NameToLayer("Players");
        newPlayer.name = u.username;

        SendReceive sr = newPlayer.GetComponent<SendReceive>();
        sr.uid = u.uid;
        sr.gm = this;
        sr.updateCharacter = true;
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

        //To differenciate between you or another player
        if (u.uid == UserUid)
        {
            SetupPlayer(newPlayer);
        }
        else
        {
            SetupOtherPlayer(newPlayer);
        }

        playerList.Add(newPlayer);
        Debug.Log("CreateNewPlayer(): Initial Position: " + newPlayer.transform.localPosition);
        return newPlayer;
    }

    void SetupOtherPlayer(GameObject player)
	{
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<CapsuleCollider>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<SendReceive>().isControlling = false;
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
        player.GetComponent<SendReceive>().isControlling = true;
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
        foreach (SendReceive p in pScripts)
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

        if (server)
        {
            server.BroadcastServerInfo(stream);
        }
        else if (client)
        {
            client.SendInfo(stream);
        }
    }

    public void ReceiveGameState() //GATHER OTHERS INFO
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        reader.ReadBoolean();
        short header = reader.ReadByte();

        uint uid = reader.ReadUInt32();
        string dump = reader.ReadString();

        SendReceive pSender = null;
        foreach (SendReceive p in pScripts)
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
            Debug.Log("ReceiveGameState(): Hit player: " + _uidHit);
            if (UserUid == _uidHit)
            {
                foreach (SendReceive p in pScripts)
                {
                    if (p.uid == UserUid)
                    {
                        p.target.TakeDamage(1);
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
