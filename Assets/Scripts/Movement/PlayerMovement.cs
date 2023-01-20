using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Needed components
    public CharacterController cc;
    public Camera cam;
    public SendReceive sr;
    public Gun gun;

    //Movement
    public Vector3 direction = new Vector3();
    public Vector3 lastDir= new Vector3();
    public Vector3 velocity;
    public float speed = 12.0f;
    public float height = 15.0f;
    public float maxStrength = 20.0f;
    public int maxBounces = 5;
    int currentBounceCount = 0;
    float currentStrength = 0.0f;

    //Ground & Gun
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask floorMask;
    public bool isGrounded;
    public bool WeaponMode = true;

    void Update()
    {
        if (WeaponMode) //Move with the recoil of the weapons
        {
            //Gravity
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, floorMask);
            
            //Shooting depending on the camera rotaion
            if (sr.isControlling)
            {
                if (Input.GetButtonDown("Fire1") && Cursor.lockState == CursorLockMode.Locked)
                {
                    velocity = Vector3.zero;
                    if (isGrounded) velocity.y += Mathf.Sqrt(height * -0.1f * Physics.gravity.y);
                    direction = cam.transform.forward;
                    currentBounceCount = maxBounces;
                    currentStrength = maxStrength;
                }
            }
            else
            {
                // Update other characters in scene (other players)
                if (lastDir != direction || gun.fire)
                {
                    velocity = Vector3.zero;
                    if (isGrounded) velocity.y += Mathf.Sqrt(height * -0.1f * Physics.gravity.y);
                    currentBounceCount = maxBounces;
                    currentStrength = maxStrength;

                    this.transform.position = sr.position;
                    lastDir = direction;
                }
            }

            //Handles bounces impact from the recoil
            Vector3 impact = Vector3.zero;
            if (currentBounceCount == maxBounces)
            {
                impact += direction.normalized * -currentStrength;
            }
            else
            {
                impact = direction.normalized * -currentStrength;
            }
            if (cc.enabled) cc.Move(impact * speed * Time.deltaTime);
        }
        
        //Movement calculations
        velocity.y += Physics.gravity.y * Time.deltaTime;

        //Handles bounces
        if (isGrounded && velocity.y < 0)
		{
			velocity = Vector3.zero;
            if (currentBounceCount >= 0)
            {
                currentBounceCount--;
                currentStrength -= (currentStrength * 1.5f / maxBounces);
            }
            else
            {
                direction = Vector3.zero;
            }
        }

        if (cc.enabled) cc.Move(velocity * Time.deltaTime);

        //Position corrector (In case prediction doesn't work)
        if (!sr.isControlling)
        {
            if (Vector3.Distance(this.transform.position, sr.position) >= 0.5f)
            {
                this.transform.position = sr.position;
            }
        }
    }
}
