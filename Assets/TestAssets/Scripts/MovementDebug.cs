using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementDebug : MonoBehaviour
{
    // Start is called before the first frame update
    public CharacterController controller;
    public SendRecieve sendRecieve;
    public bool altMovement = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float velocity = 3.0f;
        Vector3 direction = new Vector3();
        if (!altMovement)
        {
            direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        }
        else
		{
            if (Input.GetKey(KeyCode.I))
            {
                direction.z = 1.0f;
            }
            if (Input.GetKey(KeyCode.J))
            {
                direction.x = -1.0f;
            }
            if (Input.GetKey(KeyCode.K))
            {
                direction.z = -1.0f;
            }
            if (Input.GetKey(KeyCode.L))
            {
                direction.x = 1.0f;
            }
        }
        if (direction.x != 0 || direction.z != 0)
        {
            sendRecieve.moving = true;
        }
        else
        {
            sendRecieve.moving = false;
        }

        if (controller.enabled)
        {
            controller.Move(direction * Time.deltaTime * velocity);
        }
    }
}
