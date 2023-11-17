using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticConsts : MonoBehaviour
{

    static StaticConsts instance;

    public static StaticConsts Instance => instance;

    public LayerMask _ShellHitLayers;

    public int _MaxShellRaycastTicks = 256;

    public static LayerMask ShellHitLayers => Instance._ShellHitLayers;

    public static int MaxShellRaycastTicks => Instance._MaxShellRaycastTicks;

    public LayerMask _GroundLayers;

    public static LayerMask GroundLayers => Instance._GroundLayers;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance);
        instance = this;
    }

}
