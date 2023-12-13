using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public void OpenGame()
    {
        SceneManager.LoadScene("one");
    }

    public void OpenTests()
    {
        SceneManager.LoadScene("tests");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
