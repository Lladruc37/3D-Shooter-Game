using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class SendRecieve : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isControlling = false;
    public Server server;
    public Client client;

    public bool moving;
    public float timer;
    public byte[] data;
    public Thread sendThread = null;
    public Thread recieveThread = null;

    bool updatePosition;
    double x = 0.0f, y = 0.0f, z = 0.0f;
    float myTimer = 0.0f;
    // Update is called once per frame
    void Update()
    {
        myTimer += Time.deltaTime;
        if(isControlling)
        {
            if(moving)
            {
                x = (double)this.transform.localPosition.x;
                y = (double)this.transform.localPosition.y;
                z = (double)this.transform.localPosition.z;
                if (myTimer >= 0.2f)
                {
                    myTimer = 0;
                    Debug.Log("Update(): Current position: " + x + "," + y + "," + z);

                    sendThread = new Thread(SendGameState);
                    sendThread.Start();
                }
            }
        }
        else
        {
            if (updatePosition)
            {
                updatePosition = false;
                this.transform.localPosition = new Vector3((float)x, (float)y, (float)z);
            }
        }
    }
    public void SendGameState()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write("/>PlayerInfo:");

        //Position
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);

        //WeaponAction

        Debug.Log("SendGameState(): Sending serialized data...");
        server.BroadcastServerInfo(stream);

        //TODO: Temporary solution
        Thread.Sleep(100);
    }
    public void RecieveGameState()
    {
        Debug.Log("RecieveGameState(): Recieved info");
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        //Header
        string header = reader.ReadString();
        Debug.Log("RecieveGameState(): Header is " + header);

        //Position
        if(!isControlling)
        {
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();

            Debug.Log("RecieveGameState(): New position: " + x + "," + y + "," + z);
            updatePosition = true;
        }

        data = null;

        //TODO: Temporary solution
        Thread.Sleep(100);
    }
}
