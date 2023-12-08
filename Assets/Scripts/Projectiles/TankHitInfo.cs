using Projectiles.ProjectileDataBuffer_Kinematic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankHitInfo
{

    public DamageSimulator.VisualDamageNode DamageNode;

    public PlayerTankController.TransformInfo[] TransformInfos;

    public PlayerTankController Tank;

    public KinematicProjectileDataBuffer.ProjectileHitInfo ProjectileInfo;

    public DamageableRoot DamageableRoot;

    public TankHitInfo(PlayerTankController tank, KinematicProjectileDataBuffer.ProjectileHitInfo info) 
    {
        DamageNode = new DamageSimulator.VisualDamageNode(Vector3.zero, Vector3.zero);
        Tank = tank;
        DamageableRoot = tank.GetComponent<DamageableRoot>();
        TransformInfos = Tank.GetTransformInfos();
        ProjectileInfo = info;
    }

}
