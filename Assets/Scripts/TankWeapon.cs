using Fusion;
using Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TankWeapon : NetworkBehaviour
{

    public TankTurret Turret;

    public Transform ShootPointLocator;

    public GameObject ShootEffect;

    public float ReloadTime => Tank.HasLoader ? ReloadTimeNormal : ReloadTimeSlow;
    public float ReloadTimeNormal = 4f;
    public float ReloadTimeSlow = 6f;

    public float AimerDistance = 128f;
    public float DefaultAimerDistance = 128f;

    public ShellStats[] Shells;

    [FormerlySerializedAs("CurrentShellIndex")] public int CurrentLocalShellIndex = 0;

    public bool Debug = false;

    public LineRenderer LaserCheck;

    public bool MainWeapon = false;

    public PlayerTankController Tank => Turret.Tank;

    public ShellStats CurrentShell => Shells[CurrentLocalShellIndex];

    [Networked]
    private TickTimer ReloadTimer { get; set; }

    [Networked]
    private NetworkBool Reloaded { get; set; } = true;

    private void Start()
    {
        float mx = Turret.lmx;
        float my = Turret.lmy;
        if (GetInput(out NetworkInputData _))
        {
            UIManager.PositionAimingCircle(GetCircleTargetPosition(DefaultAimerDistance));
        }
        if(!Debug)
        {
            LaserCheck.enabled = false;
        }

#if UNITY_EDITOR
        DebugDrawPts = new Vector3[StaticConsts.MaxShellRaycastTicks];
#endif

    }

    public override void Spawned()
    {
        ReloadTimer = TickTimer.None;
        if (Tank.IsCurrentPlayer)
        {
            UIManager.SetReloadProgress(1f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (Tank.HasGunBreech)
            {
                if (!Reloaded && !ReloadTimer.IsRunning)
                {
                    ReloadTimer = TickTimer.CreateFromSeconds(Runner, ReloadTime);
                    if(Tank.IsCurrentPlayer)
                    {
                        UIManager.SetReloadProgress(0f);
                    }
                }
                else if (!Reloaded && ReloadTimer.Expired(Runner))
                {
                    Reloaded = true;
                    ReloadTimer = TickTimer.None;
                    if (Tank.IsCurrentPlayer)
                    {
                        UIManager.SetReloadProgress(1f);
                    }
                }
                else if (!Reloaded && ReloadTimer.IsRunning && Tank.IsCurrentPlayer)
                {
                    //update UI reload
                    UIManager.SetReloadProgress(1f - ReloadTimer.RemainingTime(Runner).Value / ReloadTime);
                }
            } else
            {
                Reloaded = false;
            }
            if(data.FirePressed && Reloaded && Tank.HasGunBarrel && Tank.HasGunner)
            {
                //print("Fire");
                Reloaded = false;
                if (Tank.IsCurrentPlayer)
                {
                    UIManager.SetReloadProgress(0f);
                }

                CurrentShell.Fire();
                ShootEffect.SetActive(true);

                if (Debug)
                {
                    LaserCheck.enabled = true;
                    LaserCheck.SetPosition(0, ShootPointLocator.position);
                    LaserCheck.SetPosition(1, ShootPointLocator.position + ShootPointLocator.forward * AimerDistance);
                }
            }
            
        }
    }

    public Vector3 GetCircleTargetPosition(float dist)
    {
        Vector3 ret = ShootPointLocator.position + ShootPointLocator.forward * dist;
        return PlayerCamera.CurrentCamera.WorldToScreenPoint(ret);
    }

    Vector3 aimVel = default;
    Vector3 aimVel2 = default;
    public float aimingCirclePositionSmoothTime = 0.02f;

    //[SerializeField]
    Vector3[] DebugDrawPts = null;
    Vector3? DebugHitpoint = null;

    private void Update()
    {
        if (MainWeapon && Object && Object.HasInputAuthority)
        {
            float mx = Turret.lmx;
            float my = Turret.lmy;
            Vector3? point = CurrentShell.HitTest(ShootPointLocator.position, ShootPointLocator.forward, ref DebugDrawPts);
            if (point.HasValue)
            {
                UIManager.SetAimingCircleEnabled(true);
                AimerDistance = Vector3.Distance(ShootPointLocator.position, point.Value);
                DebugHitpoint = point.Value;

                float newScale = Mathf.Clamp((AimerDistance - StaticConsts.MinAimingCircleApprDistance) / (StaticConsts.MaxAimingCircleApprDistance - StaticConsts.MinAimingCircleApprDistance), 0f, 1f)
                    * (StaticConsts.MaxAimingCircleScale - StaticConsts.MinAimingCircleScale);
                UIManager.ScaleAimingCircle(Vector3.one * newScale);

                UIManager.PositionAimingCircle(Vector3.SmoothDamp(UIManager.GetAimingCirclePosition(), PlayerCamera.CurrentCamera.WorldToScreenPoint(point.Value), ref aimVel, aimingCirclePositionSmoothTime));

            } else
            {
                UIManager.SetAimingCircleEnabled(false);
                AimerDistance = DefaultAimerDistance;
                DebugHitpoint = null;
            }
            Vector3 Target = GetCircleTargetPosition(AimerDistance);
            UIManager.PositionGuideanceCircle(Vector3.SmoothDamp(UIManager.GetGuideanceCirclePositoin(), Target, ref aimVel2, aimingCirclePositionSmoothTime));
            //print("mm");
        }
    }

    private void OnDrawGizmos()
    {
        if(DebugDrawPts != null)
        {
            Gizmos.color = Color.yellow;
            for(int i = 1; i < DebugDrawPts.Length; i++)
            {
                Gizmos.DrawLine(DebugDrawPts[i - 1], DebugDrawPts[i]);
            }
        }
        if(DebugHitpoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(DebugHitpoint.Value, 0.2f);
        }
    }

}
