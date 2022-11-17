using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Camera lobbyCamera;
    public Camera playerCam;
    public Camera camBack;

    public float sensibility = 100;
    public Transform playerBody;
    public float xRotacion;
    private void Start()
    {
        lobbyCamera.enabled = false;
        playerCam.enabled = true;
        camBack.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {

        if (Input.GetButton("Fire2"))
        {
            playerCam.enabled = false;
            camBack.enabled = true;
        }
        else
        {
            playerCam.enabled = true;
            camBack.enabled = false;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensibility * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibility * Time.deltaTime;

        xRotacion -= mouseY;
        xRotacion = Mathf.Clamp(xRotacion, -90, 90);

        transform.localRotation = Quaternion.Euler(xRotacion, 0, 0);

        playerBody.Rotate(Vector3.up * mouseX);
        camBack.transform.LookAt(playerBody);
    }
}
