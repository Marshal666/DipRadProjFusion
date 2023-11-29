using System;
using System.Collections;
using System.Collections.Generic;
using Projectiles.ProjectileDataBuffer_Kinematic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class Tester : MonoBehaviour
{

    public Vector3 StartPoint = default;

    public Vector3 EndPoint = new Vector3(0, 0, 3f);

    public bool DrawPointsLineGizmos = false;

    public bool DrawRotatedLines = false;

    public DamageableRoot DamageableRoot;

    public float[] original, current;
    
    public bool Shoot = false;

    public bool Restore = false;

    public DamageSimulator.VisualDamageNode node;

    public int seed = 123;
    
    public bool DrawNodeGizmos = true;

    public int DispersionCount = 10;

    public float Dispersion = 2f;

    public int DispersionSeed = 123;

    public bool DrawDispersionLines = false;
    
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

            node = new DamageSimulator.VisualDamageNode(default, default);
            
            DamageSimulator.SimulateDamage(
                new KinematicProjectileDataBuffer.ProjectileHitInfo()
                {
                    Energy = 500,
                    HitDirection = end - start,
                    HitPosition = start
                },
                node,
                seed
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
            node = null;
        }
    }

    private float VectorKEps = 0.0001f;
    
    
    Vector3 getRandomDirection(Vector3 position, Vector3 direction, float dispersion)
    {
        Vector3 helper = Vector3.up;
        direction = direction.normalized;
        Vector3 dir2 = Vector3.Cross(direction, helper);
        if (dir2.sqrMagnitude < VectorKEps)
        {
            helper = Vector3.right;
        }
        dir2 = Vector3.Cross(direction, helper);
        Vector3 dir3 = Vector3.Cross(direction, dir2);

        float n1 = Random.Range(-1f, 1f);
        float n2 = Random.Range(-1f, 1f);

        Vector3 newHDir = (dir2.normalized * n1 + dir3.normalized * n2).normalized;
        newHDir *= Random.Range(0f, dispersion);

        return (position + direction + newHDir) - position;
    }

    private void OnDrawGizmos()
    {
        Vector3 start = transform.TransformPoint(StartPoint);
        Vector3 end = transform.TransformPoint(EndPoint);
        
        Vector3 dir = end - start;
        
        Gizmos.color = Color.red;

        if (DrawPointsLineGizmos)
        {
            Gizmos.DrawLine(start, end);
        }

        if (DrawRotatedLines)
        {
            Gizmos.color = Color.blue;
            
            Vector3 up = Vector3.up;
            Vector3 dir2 = Vector3.Cross(Vector3.up, dir);
            Vector3 dir3 = Vector3.Cross(dir, dir2);
            Gizmos.DrawLine(start, start + dir2);
            Gizmos.DrawLine(start, start + dir3);
            
            Gizmos.color = Color.red;
        }

        if (DrawDispersionLines)
        {
            Gizmos.color = Color.yellow;
            Random.InitState(DispersionSeed);
            for (int i = 0; i < DispersionCount; i++)
            {
                Vector3 newDir = getRandomDirection(start, dir, Dispersion);
                Gizmos.DrawLine(start, start + newDir);
            }
            Gizmos.color = Color.red;
        }

        if (DrawNodeGizmos)
        {
            if(node == null)
                return;

            void DrawNode(DamageSimulator.VisualDamageNode vn)
            {
                if(vn == null)
                    return;
                Gizmos.DrawLine(vn.Start, vn.End);
                if (vn.HitPoints != null)
                {
                    foreach (var pt in vn.HitPoints)
                    {
                        Gizmos.DrawSphere(pt, 0.05f);
                    }
                }
                if(vn.Children == null)
                    return;
                foreach (var ch in vn.Children)
                {
                    DrawNode(ch);
                }
            }
            
            DrawNode(node);
        }
    }
}
