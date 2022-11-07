using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem muzzeFlash;

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

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        //Shooting doing raycast

        muzzeFlash.Play();

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.takeDamage(damage);
            }
        }
    }

    //Add recoil animation to the weapon
    void AddRecoil()
    {
        transform.localEulerAngles += upRecoil;
    }

    void StopRecoil()
    {
        transform.localEulerAngles = originalRotation;
    }
}
