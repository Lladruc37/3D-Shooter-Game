using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Camera lobbyCamera;
    public Camera playerCam;
    public Camera camBack;
    public bool start = false;

    public float sensibility = 100.0f;
    public Transform playerBody;
    public float xRotation;

    void Update()
    {
        if (start) //game starts
        {
            start = false;
            lobbyCamera.enabled = false;
            playerCam.enabled = true;
            camBack.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.F1)) // lock/unlock mouse
            {
                switch(Cursor.lockState)
                {
                    case CursorLockMode.Locked:
                        {
                            Cursor.lockState = CursorLockMode.None;
                            break;
                        }
                    case CursorLockMode.None:
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                            break;
                        }
                }
            }
            if (Input.GetButton("Fire2")) //back camera
            {
                playerCam.enabled = false;
                camBack.enabled = true;
            }
            else
            {
                playerCam.enabled = true;
                camBack.enabled = false;
            }

            if (Cursor.lockState == CursorLockMode.Locked) //camera rotation using the mouse
            {
                float mouseX = Input.GetAxis("Mouse X") * sensibility * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * sensibility * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90, 90);

                transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

                playerBody.Rotate(Vector3.up * mouseX);
                camBack.transform.LookAt(playerBody);
            }
        }
    }
}
