using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RootGameManager : MonoBehaviour
{

    static RootGameManager _Instance;

    public static RootGameManager Instance => _Instance;

    public GameObject BasicSpawnerPrefab;

    public GameObject[] SpawnPointLocators;

    private void Awake()
    {
        _Instance = this; 
    }

    public void QuitGame()
    {
        BasicSpawner.Instance.QuitPlaying();
        SceneManager.LoadScene("main");
    }

    public void JoinGame()
    {
        BasicSpawner.Instance.JoinGame();
    }

    public void HostGame()
    {
        BasicSpawner.Instance.StartHost();
    }

}
