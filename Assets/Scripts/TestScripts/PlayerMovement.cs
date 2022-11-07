using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController cc;
    public float gravity = -9.81f;
    public Vector3 speed;
    public float playerSpeed = 12.0f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask floorMask;
    bool isGrounded;
    public bool godMode = true;
    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity = 0.1f;

    void Start()
    {
       
    }

    void Update()
    {
        //Keep attached to the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, floorMask);
        if (isGrounded && speed.y < 0)
        {
            speed.y = -2f;
        }

        // God Mode to move with WASD in case the weapons don't work
        if (godMode)
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                speed.y = Mathf.Sqrt(3 * -2 * gravity);
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            cc.Move(move * playerSpeed * Time.deltaTime);

            speed.y += gravity * Time.deltaTime;
            cc.Move(speed * Time.deltaTime);
        }
    }
}
