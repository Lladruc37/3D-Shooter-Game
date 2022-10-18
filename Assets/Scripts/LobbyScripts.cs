using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyScripts : MonoBehaviour
{
    private static string input;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void Go2Create()
    {
        SceneManager.LoadScene(1);
    }
    public void Go2Join()
    {
        SceneManager.LoadScene(2);
    }
    public void ReadStringInput(string s)
    {
        input = s;
        Debug.Log("New name: " + input);
    }

    public void StartServer()
    {
        Debug.Log("Created server: " + input);
        SceneManager.LoadScene(3);
    }

    public void JoinServer()
    {
        Debug.Log("Joined server: " + input);
        SceneManager.LoadScene(3);
    }
}
