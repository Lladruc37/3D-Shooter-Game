using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController cc;
    public Camera cam;

    //movement
    Vector3 direction = new Vector3();
    public Vector3 velocity;
    public float speed = 12.0f;
    public float height = 15.0f;
    public float gravity = -12.0f;
    public float maxStrength = 20.0f;
    public int maxBounces = 5;
    int currentBounceCount = 0;
    float currentStrength = 0.0f;

    //ground & gun
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask floorMask;
    public bool isGrounded;
    public bool GodMode = false;
    public bool WeaponMode = true;

    void Update()
    {
        //Move with WASD in god mode
        if (GodMode)
        {
            //Gravity
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, floorMask);

            //Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(3 * -2 * gravity);
            }

            //Move with WASD inputs
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            //Movement calculations
            Vector3 move = transform.right * x + transform.forward * z;
            if (cc.enabled) cc.Move(speed * Time.deltaTime * move);
        }
		else if (WeaponMode) //Move with the recoil of the weapons
        {
            //Gravity
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, floorMask);
            
            //Shooting depending on the camera rotaion
            if (Input.GetButtonDown("Fire1") && Cursor.lockState == CursorLockMode.Locked)
            {
                velocity = Vector3.zero;
                if (isGrounded) velocity.y += Mathf.Sqrt(height * -0.1f * gravity);
                direction = cam.transform.forward;
                currentBounceCount = maxBounces;
                currentStrength = maxStrength;
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

        //Movement calculations
        velocity.y += gravity * Time.deltaTime;
        if (cc.enabled) cc.Move(velocity * Time.deltaTime);
    }
}
