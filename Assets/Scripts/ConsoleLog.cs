using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleLog : MonoBehaviour
{

    public Text ConsoleText;

    static ConsoleLog instance;

    public static ConsoleLog Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public static void Log(string message)
    {
        if (Instance != null)
        {
            Instance.ConsoleText.text += message;
        }
    }

}
