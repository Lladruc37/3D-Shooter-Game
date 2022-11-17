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
    float myTimer = 0.0f;
    float interpolationTimer = 0.15f;
    //bool lerp = false;
    //float lerpTime = 0.0f;

    public Vector3 position;
    public Vector3 rotation;
    public string username;
    public uint uid;

    Vector3 lastp = Vector3.one;

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
            if (myTimer >= interpolationTimer)
            {
                position = this.transform.localPosition;
                if (Vector3.Distance(lastp,position) > 0.0f)
                {
                    lastp = position;
                    myTimer = 0;
                    Debug.Log("Update(): Current position: " + position);
                    rotation = this.transform.rotation.eulerAngles;

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
                this.transform.rotation = Quaternion.Euler(rotation);
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
