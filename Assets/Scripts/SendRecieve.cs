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
    //double x = 0.0f, y = 0.0f, z = 0.0f;
    Vector3 currentp;
    Vector3 lastp;
    bool lerp = false;
    float myTimer = 0.0f;
    float lerpTime = 0.0f;
    float interpolationTimer = 0.1f;
    // Update is called once per frame
    void Update()
    {
        myTimer += Time.deltaTime;
        if(isControlling)
        {
            if(moving)
            {
                lastp = currentp;
                currentp = this.transform.localPosition;
                if (myTimer >= interpolationTimer)
                {
                    myTimer = 0;
                    Debug.Log("Update(): Current position: " + currentp);

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
                //lerp = true;
                //lastp = currentp;

                this.transform.localPosition = currentp;
            }
            //TODO: LERP
   //         if (lerp)
			//{
   //             Debug.Log("LERP: " + lastp + ";" + currentp);
   //             transform.localPosition = Vector3.Lerp(lastp, currentp, lerpTime / interpolationTimer);
   //             lerpTime += Time.deltaTime;
   //             if(lerpTime >= 1)
			//	{
   //                 lerp = false;
   //                 lerpTime = 0;
			//	}
   //         }
        }
    }

    public void SendGameState()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Header
        writer.Write("/>PlayerInfo:");

        //Position
        writer.Write((double)currentp.x);
        writer.Write((double)currentp.y);
        writer.Write((double)currentp.z);

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
            currentp.x = (float)reader.ReadDouble();
            currentp.y = (float)reader.ReadDouble();
            currentp.z = (float)reader.ReadDouble();

            Debug.Log("RecieveGameState(): New position: " + currentp);
            updatePosition = true;
        }

        data = null;

        //TODO: Temporary solution
        Thread.Sleep(100);
    }
}
