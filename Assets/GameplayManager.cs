using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public string UserName;
    public GameObject p1;
    public GameObject p2;
    bool start, update = false;
    public LobbyScripts comunicationDevice;

    // Start is called before the first frame update
    void Start()
    {}

    // Update is called once per frame
    void Update()
    {
        if(start)
		{
            int c = comunicationDevice.usernameList.Count;
            switch(c)
			{
                case 1:
                    p1.transform.localPosition = new Vector3(0.5f, 0.0f, 0.5f);
                    break;
                case 2:
                    p1.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    p2.transform.localPosition = new Vector3(1.0f, 0.0f, 1.0f);
                    break;
            }
            start = false;
            update = true;
		}
        if(update)
		{

		}
    }
}
