using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    //Weapon info
    public int damage = 1;
    public float offset = 0.5f;
    public float range = 100f;
    public float laserDuration = 0.05f;

    public Image hitMark;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public SendReceive playerInfo;
    public LineRenderer laserLine;

    //Gun bools
    public bool isControllingGun = false;
    public bool fire = false;

    void Start()
    {
        laserLine.enabled = false;
    }

    void Update()
    {
        //Shoot lasers with the left click button
        laserLine.SetPosition(0, transform.position);
        if (isControllingGun && Cursor.lockState == CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                fire = true;
                StartCoroutine(ShootLaser());
            }
            else
            {
                StopCoroutine(ShootLaser());
                fire = false;
                laserLine.enabled = false;
            }
        }
        else
        {
            if (fire)
            {
                StartCoroutine(ShootLaser());
            }
            else
            {
                StopCoroutine(ShootLaser());
                fire = false;
                laserLine.enabled = false;
            }
        }
    }

    //Coroutine for shooting lasers
    IEnumerator ShootLaser()
    {
        muzzleFlash.Play();
        laserLine.enabled = true;

        while (fire)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (fpsCam)
            {
                Vector3 origin = fpsCam.transform.position + (offset * fpsCam.transform.forward);
                ray = new Ray(origin, fpsCam.transform.forward);
            }
            Debug.Log("ShootLaser(): Shoot!");
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                laserLine.SetPosition(1, hit.point);
                if (playerInfo.isControlling)
                {
                    Target target = hit.collider.GetComponent<Target>();
                    if (target != null) //If the target is a player
                    {
                        hitMark.enabled = true;
                        uint uid = hit.collider.GetComponent<SendReceive>().uid;
                        Debug.Log("ShootLaser(): Hit! UID: " + uid);
                        if (uid != playerInfo.uid)
                        {
                            playerInfo.uidHit = (int)uid;
                            if (target.TakeDamage(1))
                            {
                                playerInfo.kills++;
                                MemoryStream streamDeath = new MemoryStream();
                                BinaryWriter writerDeath = new BinaryWriter(streamDeath);
                                writerDeath.Write(false);
                                writerDeath.Write((byte)packetType.chat);
                                if (playerInfo.gm.server)
                                {
                                    Debug.Log("Server has killed user!!!!!!!!!!!!!!!");
                                    string msg = "\n[" + playerInfo.username + "]>>" + hit.collider.name + " has been killed by " + playerInfo.username + "!";
                                    writerDeath.Write(msg);
                                    playerInfo.gm.server.newMessage = true;
                                    playerInfo.gm.server.stringData = msg;
                                    playerInfo.gm.server.BroadcastServerInfo(streamDeath);
                                }
                                else if (playerInfo.gm.client)
                                {
                                    Debug.Log("User has killed server!!!!!!!!!!!!!!!");
                                    writerDeath.Write(playerInfo.uid);
                                    writerDeath.Write(hit.collider.name + " has been killed by " + playerInfo.username + "!");
                                    playerInfo.gm.client.SendInfo(streamDeath);
                                }
                            }
                        }
                    }
                    else
                    {
                        hitMark.enabled = false;
                        playerInfo.uidHit = -1;
                    }
                }
            }
            else
            {
                laserLine.SetPosition(1, ray.GetPoint(range));
            }
            yield return new WaitForSeconds(laserDuration);
            if (playerInfo.isControlling) hitMark.enabled = false;
            fire = false;
        }
    }
}
