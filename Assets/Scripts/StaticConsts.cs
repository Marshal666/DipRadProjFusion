using System.Collections;
using System.Collections.Generic;
using Projectiles;
using Projectiles.ProjectileDataBuffer_Kinematic;
using UnityEngine;

public class StaticConsts : MonoBehaviour
{

    static StaticConsts instance;

    public static StaticConsts Instance => instance;

    public GameObject _RootObject;

    public static GameObject RootObject => Instance._RootObject;

    public LayerMask _ShellHitLayers;

    public LayerMask _ShellHitLayersTest;

    public int _MaxShellRaycastTicks = 256;

    public static LayerMask ShellHitLayers => Instance._ShellHitLayers;

    public static LayerMask ShellHitLayersTest => Instance._ShellHitLayersTest;

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

    public float _ShellDeathDetonationProb = 0.34f;

    public static float ShellDeathDetonationProb => Instance._ShellDeathDetonationProb;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance);
        instance = this;
    }

}
