using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankUIStats : MonoBehaviour
{

    static TankUIStats _Instance;

    public static TankUIStats Instance => _Instance;

    PlayerTankController _controller = null;

    public Text SpeedText;

    private void Awake()
    {
        _Instance = this;
    }

    public static void Init(PlayerTankController controller)
    {
        _Instance._controller = controller;
    }

    private void Update()
    {
        if(_controller)
        {
            SpeedText.text = "Speed: " + (_controller.Speed * _controller.VelocitySign * 3.6f).ToString("0.00") + " km/h";
        }
    }

}
