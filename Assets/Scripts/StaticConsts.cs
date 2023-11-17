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

    public float _MinAimingCircleScale = 0.2f;

    public static float MinAimingCircleScale => Instance._MinAimingCircleScale;

    public float _MaxAimingCircleScale = 2f;

    public static float MaxAimingCircleScale => Instance._MaxAimingCircleScale;

    public float _MinAimingCircleApprDistance = 256f;

    public static float MinAimingCircleApprDistance => Instance._MinAimingCircleApprDistance;

    public float _MaxAimingCircleApprDistance = 0.5f;

    public static float MaxAimingCircleApprDistance => Instance._MaxAimingCircleApprDistance;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance);
        instance = this;
    }

}
