using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController cc;
    public float speed = 12;

    public float height = 15.0f;
    public float gravity = -12.0f;
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

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

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

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        //Move with the recoil of the weapons
        if (WeaponMode)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, floorMask);
            float strength = -10f;
            
            //Shooting depending on the camera rotaion
            if (Input.GetButtonDown("Fire1"))
            {
                velocity.y += Mathf.Sqrt(height * -2 * gravity);
                direction = transform.forward;
            }

            Vector3 move = direction * strength;
            cc.Move(move * speed * Time.deltaTime);

            //Gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -8f;
                direction = Vector3.zero;
            }
        }

    }
}
