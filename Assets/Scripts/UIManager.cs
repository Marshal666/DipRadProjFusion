using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject SniperModeObjects;

    static UIManager instance;
    public static UIManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public void SetSniperModeUIObjectsEnabled(bool val)
    {
        SniperModeObjects.SetActive(val);
    }

    public static void SetSniperModeUIEnabled(bool val)
    {
        instance.SetSniperModeUIObjectsEnabled(val);
    }

}
