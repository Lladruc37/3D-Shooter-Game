using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem muzzeFlash;

    public float fireRate = 0.2f;
    public Transform laserOrigin;
    public LineRenderer laserLine;
    public float laserDuration = 0.05f;
    float fireTimer;

    //Recoil settings
    public Vector3 upRecoil;
    Vector3 originalRotation;

    void Start()
    {
        originalRotation = transform.localEulerAngles;
    }

    void Update()
    {
        //Shoot with the left click button
        fireTimer += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && fireTimer > fireRate)
        {            
            Shoot();
        }
    }

    void Shoot()
    {
        //Shooting doing raycast
        fireTimer = 0;
        muzzeFlash.Play();
        laserLine.SetPosition(0, laserOrigin.position);
        Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                laserLine.SetPosition(1, hit.point);
                target.takeDamage(damage);
            }
        }
        else
        {
            laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * range));
        }
        StartCoroutine(ShootLaser());
    }

    IEnumerator ShootLaser()
    {
        laserLine.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }
}
