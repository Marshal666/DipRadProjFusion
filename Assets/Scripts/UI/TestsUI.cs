using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestsUI : MonoBehaviour
{
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("main");
    }

}
