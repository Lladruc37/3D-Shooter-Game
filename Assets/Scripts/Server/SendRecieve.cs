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
    public Gun gun;
    public MouseLook gunDirection;
    public Target target;

    public int kills = 0;
    public int uidHit = -1;
    public int uidRecieved = -1;

    public bool moving;
    public float timer;

    public bool updateCharacter;
    float myTimer = 0.0f;
    float interpolationTimer = 0.15f;
    //bool lerp = false;
    //float lerpTime = 0.0f;

    public Vector3 position;
    public Vector3 rotation;
    public string username;
    public uint uid;

    int lastHP = -1;
    Vector3 lastp = Vector3.one;
    Vector3 lastr = Vector3.one;

    // Update is called once per frame
    void Update()
    {
        username = name;
        if (!updateCharacter)
        {
            position = transform.localPosition;
            rotation = transform.rotation.eulerAngles;
        }
        myTimer += Time.deltaTime;
        if(isControlling)
        {
            if (target.health > 0)
            {
                if (myTimer >= interpolationTimer)
                {
                    position = transform.localPosition;
                    rotation = transform.rotation.eulerAngles;
                    rotation.x = gunDirection.xRotacion;
                    if (Vector3.Distance(lastp, position) > 0.0f || Vector3.Angle(lastr, rotation) > 0.0f || gun.fire || lastHP != target.health)
                    {
                        lastHP = target.health;
                        lastp = position;
                        lastr = rotation;
                        rotation.x = 0.0f;
                        myTimer = 0;
                        //Debug.Log("Update(): Current position: " + position);

                        gm.sendThread = new Thread(gm.SendGameState);
                        gm.sendThread.Start();
                    }
                }
            }
        }
        else
        {
            if (updateCharacter)
            {
                updateCharacter = false;
                //lerp = true;
                //lastp = currentp;
                Debug.Log("Uid: " + uid + ", Hp: " + target.health);

                if (target.health > 0)
                {
                    //Debug.Log("Update(): New Position of " + username + ": " + position);
                    this.transform.rotation = Quaternion.Euler(rotation);
                    gunDirection.transform.localRotation = Quaternion.Euler(gunDirection.xRotacion, 0, 0);
                }
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
