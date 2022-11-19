using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public int damage = 1;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public SendRecieve playerInfo;

    //public float fireRate = 0.2f;
    public LineRenderer laserLine;
    public float laserDuration = 0.05f;
    //float fireTimer;

    public bool isControllingGun;
    public bool fire = false;

    void Start()
    {
        laserLine.enabled = false;
    }

    void Update()
    {
        laserLine.SetPosition(0, transform.position);
        //Shoot with the left click button
        if (isControllingGun && Cursor.lockState == CursorLockMode.Locked)
        {
            //fireTimer += Time.deltaTime;

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
            //fireTimer += Time.deltaTime;

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

    IEnumerator ShootLaser()
    {
        muzzleFlash.Play();
        laserLine.enabled = true;

        while (fire)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (fpsCam)
            {
                ray = new Ray(transform.position, fpsCam.transform.forward);
            }
            Debug.Log("ShootLaser(): Shoot!");
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                laserLine.SetPosition(1, hit.point);
                if (playerInfo.isControlling)
                {
                    Target target = hit.collider.GetComponent<Target>();
                    if (target != null)
                    {
                        uint uid = hit.collider.GetComponent<SendRecieve>().uid;
                        Debug.Log("ShootLaser(): Hit! UID: " + uid);
                        playerInfo.uidHit = (int)uid;
                        if (target.takeDamage(1))
                        {
                            playerInfo.kills++;
                        }
                    }
                    else
                    {
                        playerInfo.uidHit = -1;
                    }
                }
            }
            else
            {
                laserLine.SetPosition(1, ray.GetPoint(range));
            }
            yield return new WaitForSeconds(laserDuration);
            fire = false;
        }
    }
}
