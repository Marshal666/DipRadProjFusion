using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticConsts : MonoBehaviour
{

    static StaticConsts instance;

    public static StaticConsts Instance => instance;

    public LayerMask _ShellHitLayers;

    public float _MaxShellRaycastLength = 16384f;

    public static LayerMask ShellHitLayers => Instance._ShellHitLayers;

    public static float MaxShellRaycastLength => Instance._MaxShellRaycastLength;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance);
        instance = this;
    }

}
