using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController cc;
    public float Velocidad = 12;

    public float Gravedad = -9.81f;
    public Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask floorMask;
    bool isGrounded;
    public bool GodMode = false;
    public bool WeaponMode = false;
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
                velocity.y = Mathf.Sqrt(3 * -2 * Gravedad);
            }

            //Move with WASD inputs
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            cc.Move(move * Velocidad * Time.deltaTime);
        }

        velocity.y += Gravedad * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        //Move with the retrieval of the weapons
        if (WeaponMode)
        {

        }

    }
}
