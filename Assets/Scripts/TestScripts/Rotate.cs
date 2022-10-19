using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using System.Threading;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    public float increment = 0.0f;
    byte[] data;
    Thread loadImageThread = null;

    // Start is called before the first frame update
    void Start()
    {
        loadImageThread = new Thread(loadImage);
    }

    void loadImage()
    {
        Debug.LogWarning("Starting Thread!");
        data = new byte[1000000000];
        for (int y = 0; y < 1000000000; y++)
        {
            data[y] = 1;
        }
        System.IO.File.WriteAllBytes("amongus", data);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 1, 0), increment);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            loadImageThread.Start();
        }
    }
}
