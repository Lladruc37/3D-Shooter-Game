using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public string UserName;
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
            int c = comunicationDevice.usernameList.Count;
            Debug.Log("Start(): Player count: " + c);

            if (c != 0)
            {
                playerList = new GameObject[] { p1, p2, p3, p4 };
                Debug.Log("Start(): Player Models: " + playerList.Length);

                int i = 0;
                foreach (string userName in comunicationDevice.usernameList)
                {
                    playerList[i].name = userName;
                    pScripts.Add(playerList[i].GetComponent<SendRecieve>());
                    playerList[i].GetComponent<SendRecieve>().assigned = true;
                    ++i;
                }

                foreach (GameObject player in playerList)
                {
                    if (player.GetComponent<SendRecieve>().assigned)
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
                    else
					{
                        player.SetActive(false);
					}
                }

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
                }
            }

            start = false;
            update = true;
		}
        if(update)
		{
        }
    }

    public void SendGameState() //YOU SEND INFO
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write("/>PlayerInfo:");
        writer.Write(0);

        writer.Write(UserName);
        //Position
        foreach(SendRecieve p in pScripts)
		{
            if(p.username == UserName)
			{
                writer.Write((double)p.position.x);
                writer.Write((double)p.position.y);
                writer.Write((double)p.position.z);
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
        Debug.Log("RecieveGameState(): Recieved info");
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        string header = reader.ReadString();
        Debug.Log("RecieveGameState(): Header is " + header);
        int dump = reader.ReadInt32();

        string n = reader.ReadString();
        Debug.Log(n + " - " + UserName);
        foreach(SendRecieve p in pScripts)
		{
            if (p.username == n)
            {
                //Position
                p.position.x = (float)reader.ReadDouble();
                p.position.y = (float)reader.ReadDouble();
                p.position.z = (float)reader.ReadDouble();

                Debug.Log("RecieveGameState(): New position: " + p.position);
                p.updatePosition = true;
            }
        }

        data = null;

        //TODO: Temporary solution
        Thread.Sleep(100);
    }
}
