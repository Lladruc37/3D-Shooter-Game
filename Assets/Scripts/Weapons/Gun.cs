using System.Collections;
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
