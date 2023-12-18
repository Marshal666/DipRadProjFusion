using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstantForward : MonoBehaviour
{

    public Toggle Toggle;

    private void Awake()
    {
        if(!Toggle)
        {
            Toggle = GetComponent<Toggle>();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Toggle.isOn = !Toggle.isOn;
        }
    }

}
