using Fusion;
using Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankWeapon : NetworkBehaviour
{

    public TankTurret Turret;

    public Transform ShootPointLocator;

    public float ReloadTime = 6f;

    public float AimerDistance = 128f;
    public float DefaultAimerDistance = 128f;

    public ShellStats[] Shells;

    public int CurrentShellIndex = 0;

    public bool Debug = false;

    public LineRenderer LaserCheck;

    public bool MainWeapon = false;

    public ShellStats CurrentShell => Shells[CurrentShellIndex];

    [Networked]
    private TickTimer ReloadTimer { get; set; }

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
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if(data.FirePressed && ReloadTimer.ExpiredOrNotRunning(Runner))
            {
                //print("Fire");
                ReloadTimer = TickTimer.CreateFromSeconds(Runner, ReloadTime);
                if (Debug)
                {
                    LaserCheck.enabled = true;
                    LaserCheck.SetPosition(0, ShootPointLocator.position);
                    LaserCheck.SetPosition(1, ShootPointLocator.position + ShootPointLocator.forward * AimerDistance);
                }
                CurrentShell.Fire();
            }
            
        }
    }

    public Vector3 GetCircleTargetPosition(float dist)
    {
        Vector3 ret = ShootPointLocator.position + ShootPointLocator.forward * dist;
        return PlayerCamera.CurrentCamera.WorldToScreenPoint(ret);
    }

    Vector3 aimVel = default;
    public float aimingCirclePositionSmoothTime = 0.02f;

    private void Update()
    {
        if (MainWeapon && GetInput(out NetworkInputData _))
        {
            float mx = Turret.lmx;
            float my = Turret.lmy;
            Vector3? point = CurrentShell.HitTest(ShootPointLocator.position, ShootPointLocator.forward);
            if (point.HasValue)
            {
                AimerDistance = Vector3.Distance(ShootPointLocator.position, point.Value);
                //print($"hit: {AimerDistance}");
            } else
            {
                AimerDistance = DefaultAimerDistance;
            }
            Vector3 Target = GetCircleTargetPosition(AimerDistance);
            UIManager.PositionAimingCircle(Vector3.SmoothDamp(UIManager.GetAimingCirclePosition(), Target, ref aimVel, aimingCirclePositionSmoothTime));
            //print("mm");
        }
    }

}
