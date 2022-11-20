using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class SendRecieve : MonoBehaviour
{
    //Variables for the different states of the players
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

    public Vector3 position;
    public Vector3 rotation;
    public string username;
    public uint uid;

    int lastHP = -1;
    Vector3 lastp = Vector3.one;
    Vector3 lastr = Vector3.one;

    void Update()
    {
        username = name;
        if (!updateCharacter) //Update the position and rotation of the players
        {
            position = transform.localPosition;
            rotation = transform.rotation.eulerAngles;
        }
        myTimer += Time.deltaTime;
        if(isControlling)
        {
            if (target.health > 0)
            {
                if (myTimer >= interpolationTimer) //Send information in a short period of time
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

                        gm.sendThread = new Thread(gm.SendGameState);
                        gm.sendThread.Start();
                    }
                }
            }
        }
        else
        {
            if (updateCharacter) //Updates character's health
            {
                updateCharacter = false;
                Debug.Log("Uid: " + uid + ", Hp: " + target.health);

                if (target.health > 0)
                {
                    this.transform.rotation = Quaternion.Euler(rotation);
                    gunDirection.transform.localRotation = Quaternion.Euler(gunDirection.xRotacion, 0, 0);
                    target.bodyMesh.enabled = true;
                    target.gunMesh.enabled = true;
                    target.gunBod.enabled = true;
                    gun.enabled = true;
                }
                this.transform.localPosition = position;
            }
            //TODO: LERP
        }
    }
}
