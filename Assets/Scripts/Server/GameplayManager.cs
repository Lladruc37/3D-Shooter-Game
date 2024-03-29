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
    public int spawnIndex;

    public PlayerNetInfo() { }

    public PlayerNetInfo(uint _uid, string _username, IPEndPoint _ip, int _spawnIndex)
    {
        uid = _uid;
        username = _username;
        ip = _ip;
        spawnIndex = _spawnIndex;
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
    public bool newPlayer;

    //Threads
    public Thread sendThread = null;
    public Thread receiveThread = null;

    //Point system
    public int pointsToScore = 2;
    public int firstPlayer = 0;
    public string firstPlayerUsername = "";
    public float winnerTime = 5.0f;
    public float winnerTimer = 0.0f;
    public Text firstPlayerText; //winning player
    public Text playerText; //you
    public bool win = false;
    public Text winnerText; //"you win" text
    public GameObject winnerBox;

    //Other UI
    Target targetScript;
    public Text hpText;
    public AudioSource victoryJingle;

    //User data
    public uint UserUid;
    public string UserName;
    public bool matchStarted, start, startTwo, update = false;

    //All users data
    public List<GameObject> playerList = new List<GameObject>();
    public List<SendReceive> pScripts;
    public List<SendReceive> pScriptsMidGame;
    public float groundLevel = 1.234f;
    public List<Vector3> spawnpoints = new List<Vector3>();

    //Health Packs
    public GameObject healthPackPrefab;
    public List<SimpleCollectibleScript> healthPacks = new List<SimpleCollectibleScript>();
    bool healthPackReceived = false;
    public bool healthPack = false;
    public int healthPackId = 0;
    public int healthPackIdReceived = 0;

    void Update()
    {
        if (startTwo)
        {
            foreach (PlayerNetInfo user in lobby.clientList) //Add players to the list & instantiates them in the world
            {
                if (user.uid == UserUid)
                {
                    Debug.Log("StartTwo(): Adding pScripts, values: " + user.uid + " - " + user.username);
                    GameObject player = CreateNewPlayer(user);
                    break;
                }
            }
            update = true;
            startTwo = false;
        }

        if (start)
		{
			//Spawnpoints
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

            //Health Packs
            if (!matchStarted)
            {
                InstantiateHealthPacks();
            }
			else
			{
                List<SimpleCollectibleScript> tmpList = new List<SimpleCollectibleScript>();
                foreach (SimpleCollectibleScript hp in healthPacks)
                {
                    Vector3 hpPos = new Vector3();
                    switch (hp.id)
                    {
                        case 1:
                            {
                                hpPos = new Vector3(9.5f, 35.0f, -49.0f);
                                break;
                            }
                        case 2:
                            {
                                hpPos = new Vector3(-96.5f, 35.0f, -31.5f);
                                break;
                            }
                        case 3:
                            {
                                hpPos = new Vector3(-62.75f, 28.0f, 11.25f);
                                break;
                            }
                        case 4:
                            {
                                hpPos = new Vector3(-48.5f, 17.0f, 27.0f);
                                break;
                            }
                        case 5:
                            {
                                hpPos = new Vector3(-53.25f, 20.0f, 56.25f);
                                break;
                            }
                        case 6:
                            {
                                hpPos = new Vector3(14.5f, 17.5f, 46.0f);
                                break;
                            }
                        case 7:
                            {
                                hpPos = new Vector3(-14.5f, 17.5f, -14.75f);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                    SimpleCollectibleScript newHp = Instantiate(healthPackPrefab, hpPos, Quaternion.identity).GetComponent<SimpleCollectibleScript>();
                    newHp.id = hp.id;
                    tmpList.Add(newHp);
                }
                healthPacks = tmpList;
            }

            Application.targetFrameRate = 60;
			int c = lobby.clientList.Count;
			Debug.Log("Start(): Player count: " + c);

			if (c != 0) //Setup gameplay scene
			{
				playerList.Clear();
				pScripts.Clear();
                int i = 0;
				foreach (PlayerNetInfo user in lobby.clientList) //Add players to the list & instantiates them in the world
				{
					if (matchStarted)
					{
						if (user.uid != UserUid) //Instantiate all except player
						{
							Debug.Log("Start(): Adding pScripts, values: " + user.uid + " - " + user.username);
							GameObject player = CreateNewPlayer(user, true);
						}
					}
					else
					{
						Debug.Log("Start(): Adding pScripts, values: " + user.uid + " - " + user.username);
						GameObject player = CreateNewPlayer(user);
					}
                    ++i;
                    Debug.Log("COUNT MID GAME: " + i);
				}
			}
			Debug.Log("Start(): Player Models: " + playerList.Count);

			start = false;
			if (matchStarted)
			{
				startTwo = true;
				matchStarted = false;
			}
			else
			{
				update = true;
			}
		}

		if (update) //Updates point system & HP UI
		{
            hpText.text = "HP: " + targetScript.health.ToString();
            if (!win)
            {
                if(healthPackReceived)
				{
                    healthPackReceived = false;
                    foreach (SimpleCollectibleScript hp in healthPacks)
                    {
                        if (hp.id == healthPackIdReceived)
                        {
                            GameObject tmp = hp.gameObject;
                            healthPacks.Remove(hp);
                            Destroy(tmp);
                            break;
                        }
                    }
                }
                if (pScripts.Count < lobby.clientList.Count) //In case player joins mid-game
                {
                    newPlayer = true;
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
                    lobby.EndGame();
                }
            }
        }
    }

    private void InstantiateHealthPacks()
    {
        SimpleCollectibleScript hp = Instantiate(healthPackPrefab, new Vector3(9.5f, 35.0f, -49.0f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 1;
        healthPacks.Add(hp);
        hp = Instantiate(healthPackPrefab, new Vector3(-96.5f, 35.0f, -31.5f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 2;
        healthPacks.Add(hp);
        hp = Instantiate(healthPackPrefab, new Vector3(-62.75f, 28.0f, 11.25f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 3;
        healthPacks.Add(hp);
        hp = Instantiate(healthPackPrefab, new Vector3(-48.5f, 17.0f, 27.0f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 4;
        healthPacks.Add(hp);
        hp = Instantiate(healthPackPrefab, new Vector3(-53.25f, 20.0f, 56.25f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 5;
        healthPacks.Add(hp);
        hp = Instantiate(healthPackPrefab, new Vector3(14.5f, 17.5f, 46.0f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 6;
        healthPacks.Add(hp);
        hp = Instantiate(healthPackPrefab, new Vector3(-14.5f, 17.5f, -14.75f), Quaternion.identity).GetComponent<SimpleCollectibleScript>();
        hp.id = 7;
        healthPacks.Add(hp);
    }
    public void DeleteHealthPacks()
    {
        foreach (SimpleCollectibleScript hp in healthPacks)
        {
            Destroy(hp.gameObject);
        }
    }

    //End game setup
    void GameEnd()
    {
        Debug.Log("GameEnd(): Game finished! Kills: " + firstPlayer);
        winnerBox.SetActive(true);
        winnerText.text = firstPlayerUsername + " wins the game!";
        win = true;
        winnerTimer = 0.0f;
        firstPlayer = 0;

        hitMarkImage.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        playerText.text = 0.ToString();

        if (server) RandomizeSpawnPoints();
        DeleteHealthPacks();
        foreach (SendReceive p in pScripts)
		{
            if (firstPlayer == p.kills) SendGameState();
        }
        pScripts.Clear();
        lobbyCamera.enabled = true;
        foreach (GameObject gO in playerList)
        {
            Destroy(gO);
        }

        lobby.mainAudioListener.enabled = true;
        lobby.gameMusic.Stop();
        victoryJingle.Play();
        playerList.Clear();
    }

    //Randomize spawnpoints for beginning a game if not midgame
    void RandomizeSpawnPoints()
	{
        server.blacklistedSpawns.Clear();
        foreach(PlayerNetInfo p in lobby.clientList)
		{
            p.spawnIndex = server.RandomizeSpawnIndex();
		}
	}

    //Instantiates new player given player info
    GameObject CreateNewPlayer(PlayerNetInfo u, bool midGame = false)
    {
        GameObject newPlayer = null;
        if (!midGame)
        {
            Debug.Log("CreateNewPlayer(): Chosen Position: " + spawnpoints[u.spawnIndex]);
            newPlayer = Instantiate(playerPrefab, spawnpoints[u.spawnIndex], Quaternion.identity);
        }
        else
        {
            foreach(SendReceive tmp in pScriptsMidGame)
            {
                if (tmp.uid == u.uid)
                {
                    newPlayer = Instantiate(playerPrefab, tmp.position, Quaternion.identity);
                }
            }
        }

        newPlayer.layer = 6;
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
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<CapsuleCollider>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<SendReceive>().isControlling = false;
        player.GetComponentInChildren<MouseLook>().enabled = false;
        player.GetComponentInChildren<Gun>().isControllingGun = false;
        Camera[] cameras = player.GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
        }
        player.GetComponentInChildren<AudioListener>().enabled = false;
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
        player.GetComponentInChildren<AudioListener>().enabled = true;
    }

    public void SendGameState() //User sends their information to other players
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write(false);
        writer.Write((byte) packetType.playerInfo);
        writer.Write(UserUid);
        writer.Write(UserName);

        lock (pScripts)
        {
            foreach (SendReceive p in pScripts)
            {
                if (p.uid == UserUid)
                {
                    //Health & Kills
                    writer.Write(p.target.health);
                    writer.Write(p.kills);

                    //Position
                    ushort x = ConvertToFixed(p.position.x, -130f, 0.01f), y = ConvertToFixed(p.position.y, -130f, 0.01f), z = ConvertToFixed(p.position.z, -130f, 0.01f);
                    writer.Write(x);
                    writer.Write(y);
                    writer.Write(z);

                    //Direction
                    ushort dx = ConvertToFixed(p.move.direction.x, -1f, 0.0001f),
                        dy = ConvertToFixed(p.move.direction.y, -1f, 0.0001f),
                        dz = ConvertToFixed(p.move.direction.z, -1f, 0.0001f);

                    writer.Write(dx);
                    writer.Write(dy);
                    writer.Write(dz);

                    //Rotation
                    ushort ry = ConvertToFixed(p.rotation.y / 360.0f, 0.0f, 0.0001f);
                    writer.Write(ry);

                    //Weapon Action
                    writer.Write(p.gun.fire);
                    ushort rx = ConvertToFixed(p.gunDirection.xRotation / 90.0f, -1f, 0.0001f);
                    writer.Write(rx);
                    writer.Write(p.uidHit);
                    p.uidHit = -1;

                    //Health Pack
                    writer.Write(healthPack);
                    if (healthPack)
					{
                        writer.Write(healthPackId);
                        healthPack = false;
					}

                    break;
                }
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

    public void ReceiveGameState() //User recieves info from other players
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        reader.ReadBoolean();
        short header = reader.ReadByte();

        uint uid = reader.ReadUInt32();
        string username = reader.ReadString();

        SendReceive pSender = null;
        lock (pScripts)
        {
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
                pSender.updateCharacter = true;

                //Health & Kills
                pSender.target.health = reader.ReadInt32();
                pSender.kills = reader.ReadInt32();

                //Position
                pSender.position.x = ConvertFromFixed(reader.ReadUInt16(), -130f, 0.01f);
                pSender.position.y = ConvertFromFixed(reader.ReadUInt16(), -130f, 0.01f);
                pSender.position.z = ConvertFromFixed(reader.ReadUInt16(), -130f, 0.01f);

                //Direction
                pSender.move.direction.x = ConvertFromFixed(reader.ReadUInt16(), -1f, 0.0001f);
                pSender.move.direction.y = ConvertFromFixed(reader.ReadUInt16(), -1f, 0.0001f);
                pSender.move.direction.z = ConvertFromFixed(reader.ReadUInt16(), -1f, 0.0001f);

                //Rotation
                pSender.rotation.y = ConvertFromFixed(reader.ReadUInt16(), 0.0f, 0.0001f) * 360.0f;

                //Weapon Action
                pSender.gun.fire = reader.ReadBoolean();
                pSender.gunDirection.xRotation = ConvertFromFixed(reader.ReadUInt16(), -1f, 0.0001f) * 90.0f;

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
                            Debug.Log("ReceiveGameState(): Resulting health: " + p.target.health);
                            break;
                        }
                    }
                }

                //Health Pack
                healthPackReceived = reader.ReadBoolean();
                if (healthPackReceived) healthPackIdReceived = reader.ReadInt32();
            }
        }
        data = null;
    }

    // For positions & rotations
    // 0.01 & 0.0001 precision respectively
    public UInt16 ConvertToFixed(float inNumber, float inMin, float inPrecision)
	{
        return (UInt16)((inNumber - inMin) /inPrecision);
	}

    public float ConvertFromFixed(UInt16 inNumber, float inMin, float inPrecision)
	{
        return (float)(inNumber * inPrecision) + inMin;
	}
}
