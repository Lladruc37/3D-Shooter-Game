using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class SendRecieve : MonoBehaviour
{
    public bool isControlling = false;
    public bool assigned = false;
    public GameplayManager gm;

    public bool moving;
    public float timer;

    public bool updatePosition;
    //double x = 0.0f, y = 0.0f, z = 0.0f;
    Vector3 lastp;
    bool lerp = false;
    float myTimer = 0.0f;
    float lerpTime = 0.0f;
    float interpolationTimer = 0.1f;

    public Vector3 position;
    public string username;

    // Update is called once per frame
    void Update()
    {
        username = name;
        if (!updatePosition)
        {
            position = transform.localPosition;
        }
        myTimer += Time.deltaTime;
        if(isControlling)
        {
            if(moving)
            {
                lastp = position;
                position = this.transform.localPosition;
                if (myTimer >= interpolationTimer)
                {
                    myTimer = 0;
                    Debug.Log("Update(): Current position: " + position);

                    gm.sendThread = new Thread(gm.SendGameState);
                    gm.sendThread.Start();
                }
            }
        }
        else
        {
            if (updatePosition)
            {
                Debug.Log("Update(): New Position of " + username + ": " + position);
                updatePosition = false;
                //lerp = true;
                //lastp = currentp;

                this.transform.localPosition = position;
            }
                //TODO: LERP
                //    if (lerp)
                //    {
                //        Debug.Log("LERP: " + lastp + ";" + currentp);
                //        transform.localPosition = Vector3.Lerp(lastp, currentp, lerpTime / interpolationTimer);
                //        lerpTime += Time.deltaTime;
                //        if (lerpTime >= 1)
                //        {
                //            lerp = false;
                //            lerpTime = 0;
                //        }
                //    }
        }
    }
}
