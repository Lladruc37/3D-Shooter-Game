using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController cc;
    public Camera cam;
    public float speed = 12;

    public float height = 15.0f;
    public float gravity = -12.0f;
    public float strength = 200.0f;
    public Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask floorMask;
    public bool isGrounded;
    public bool GodMode = false;
    public bool WeaponMode = true;

    Vector3 direction = new Vector3();

    void Update()
    {
        //Move with WASD in case the Weapon Mode is not working
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

            Vector3 move = transform.right * x + transform.forward * z;
            cc.Move(move * speed * Time.deltaTime);
        }
		else if (WeaponMode) //Move with the recoil of the weapons
        {
            //Gravity
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, floorMask);
            
            //Shooting depending on the camera rotaion
            if (Input.GetButtonDown("Fire1"))
            {
                //velocity = Vector3.zero;
                //velocity.y += Mathf.Sqrt(height * -2 * gravity);
                direction = cam.transform.forward;
            }

            Vector3 impact = direction.normalized * -strength;
            cc.Move(impact * speed * Time.deltaTime);

        }

		if (isGrounded && velocity.y < 0)
		{
			velocity = Vector3.zero;
            direction = Vector3.zero;
		}

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}
