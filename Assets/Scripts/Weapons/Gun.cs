using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public PlayerHandler playerInfo;

    public float fireRate = 0.2f;
    public LineRenderer laserLine;
    public float laserDuration = 0.05f;
    float fireTimer;

    public bool isControllingGun;
    bool fire = false;

    void Start()
    {
        laserLine.enabled = false;
    }

    void Update()
    {
        laserLine.SetPosition(0, transform.position);
        //Shoot with the left click button
        if (isControllingGun)
        {
            fireTimer += Time.deltaTime;

            if (Input.GetButtonDown("Fire1") && fireTimer > fireRate)
            {
                fire = true;
                StartCoroutine(ShootLaser());
            }
            else if (Input.GetButtonUp("Fire1") && fireTimer <= fireRate)
            {
                fire = false;
                StopCoroutine(ShootLaser());
            }
        }
    }

    IEnumerator ShootLaser()
    {
        fireTimer = 0;
        muzzleFlash.Play();
        laserLine.enabled = true;
        while(fire)
		{
            Ray ray = new Ray(transform.position, transform.forward);
            if (fpsCam)
            {
                ray = new Ray(transform.position, fpsCam.transform.forward);
            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                laserLine.SetPosition(1, hit.point);

                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    Debug.Log("ShootLaser(): Hit!");
                    if (target.takeDamage(damage)) //returns true if this killed
					{
                        playerInfo.kills++;
					}
                }
            }
            else
            {
                laserLine.SetPosition(1, ray.GetPoint(range));
            }
            yield return new WaitForSeconds(laserDuration);
        }
        laserLine.enabled = false;
    }
}
