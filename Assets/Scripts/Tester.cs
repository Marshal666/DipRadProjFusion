using System;
using System.Collections;
using System.Collections.Generic;
using Projectiles.ProjectileDataBuffer_Kinematic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Tester : MonoBehaviour
{

    public Vector3 StartPoint = default;

    public Vector3 EndPoint = new Vector3(0, 0, 3f);

    

    public DamageableRoot DamageableRoot;

    public float[] original, current;
    
    public bool Shoot = false;

    public bool Restore = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Shoot)
        {
            Vector3 start = transform.TransformPoint(StartPoint);
            Vector3 end = transform.TransformPoint(EndPoint);

            original = DamageableRoot.GetState();
            
            DamageSimulator.SimulateDamage(
                new KinematicProjectileDataBuffer.ProjectileHitInfo()
                {
                    Energy = 500,
                    HitDirection = end - start,
                    HitPosition = start
                }
                );

            current = DamageableRoot.GetState();

            Shoot = false;
        }

        if (Restore)
        {
            if (original != null)
            {
                DamageableRoot.SetState(original);
            }

            Restore = false;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 start = transform.TransformPoint(StartPoint);
        Vector3 end = transform.TransformPoint(EndPoint);
        
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(start, end);
    }
}
