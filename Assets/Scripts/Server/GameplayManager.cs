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
    public int kills = 0;

    public Text playerText;

    public bool start, update = false;
    public LobbyScripts comunicationDevice;
    public GameObject p1, p2, p3, p4;
    public GameObject[] playerList;

    public byte[] data;
    public Thread sendThread = null;
    public Thread recieveThread = null;
    public Server server;
    public Client client;
    public List<SendRecieve> pScripts;

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
            if (playerText)
            {
                playerText.text = kills.ToString();
            }
        }
    }

    void SetupOtherPlayer(GameObject player)
	{
        player.GetComponent<CharacterController>().enabled = false;
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
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<SendRecieve>().isControlling = true;
        player.GetComponentInChildren<MouseLook>().enabled = true;
        player.GetComponentInChildren<MouseLook>().start = true;
        player.GetComponentInChildren<Gun>().isControllingGun = true;
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
                    p1.transform.localPosition = new Vector3(1.0f, 1.234f, -1.0f);
                    break;
                }
            case 2:
                {
                    p1.transform.localPosition = new Vector3(0.0f, 1.234f, -8.0f);
                    p2.transform.localPosition = new Vector3(0.0f, 1.234f, 6.0f);
                    break;
                }
            case 3:
                {
                    p1.transform.localPosition = new Vector3(0.0f, 1.234f, -8.0f);
                    p2.transform.localPosition = new Vector3(0.0f, 1.234f, 6.0f);
                    p3.transform.localPosition = new Vector3(8.0f, 1.234f, 0.0f);
                    break;
                }
            case 4:
                {
                    p1.transform.localPosition = new Vector3(0.0f, 1.234f, -8.0f);
                    p2.transform.localPosition = new Vector3(0.0f, 1.234f, 6.0f);
                    p3.transform.localPosition = new Vector3(0.0f, 1.234f, -6.0f);
                    p4.transform.localPosition = new Vector3(8.0f, 1.234f, 0.0f);
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
        foreach(SendRecieve p in pScripts)
		{
            if(p.uid == UserUid)
			{
                //Position
                writer.Write((double)p.position.x);
                writer.Write((double)p.position.y);
                writer.Write((double)p.position.z);

                //Rotation
                writer.Write((double)p.rotation.x);
                writer.Write((double)p.rotation.y);
                writer.Write((double)p.rotation.z);
                break;
            }
        }
        //WeaponAction

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

        foreach(SendRecieve p in pScripts)
		{
            if (p.uid == uid && uid != UserUid)
            {
                //Position
                p.position.x = (float)reader.ReadDouble();
                p.position.y = (float)reader.ReadDouble();
                p.position.z = (float)reader.ReadDouble();

                //Rotation
                p.rotation.x = (float)reader.ReadDouble();
                p.rotation.y = (float)reader.ReadDouble();
                p.rotation.z = (float)reader.ReadDouble();

                Debug.Log("RecieveGameState(" + UserUid + "): New position: " + p.position + "with uid: " + p.uid);
                p.updatePosition = true;
            }
        }

        data = null;

        //TODO: Temporary solution
        Thread.Sleep(100);
    }
}
