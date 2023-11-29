using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Projectiles.ProjectileDataBuffer_Kinematic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class DamageSimulator : MonoBehaviour
{
    private static DamageSimulator _Instance = null;

    public static DamageSimulator Instance => _Instance;

    public LayerMask InnerLayers;

    public float MaxDamageRaycastDistance = 32f;

    public float BounceOffAngle = 80f;

    public float BounceOffEnergyCost = 40f;

    public float EnergyPerShrapnelCost = 30f;

    public float DispersionEnergyCoeff = 20f;

    public float SubShrapnelEnergyCoeff = 0.7f;

    public float SubShrapnelEnergyCost = 30f;

    public float ArmourEnergyCoeff = 10f;

    public float VectorKEps = 0.0001f;
    
    private enum ShrapnelState
    {
        Miss,
        InArmour,
        InInner
    }

    [Serializable]
    public class VisualDamageNode
    {
        public Vector3 Start;
        public Vector3 End;

        public List<Vector3> HitPoints;
        
        public List<VisualDamageNode> Children;

        public VisualDamageNode Parent;

        public VisualDamageNode(Vector3 start, Vector3 end, VisualDamageNode parent = null)
        {
            Start = start;
            End = end;

            Parent = parent;

            Children = new List<VisualDamageNode>(8);
            HitPoints = new List<Vector3>(8);
        }

        public VisualDamageNode AddChild(Vector3 start, Vector3 end)
        {
            VisualDamageNode ret = new VisualDamageNode(start, end, this);
            Children.Add(ret);
            return ret;
        }
        
    }

    private void OnEnable()
    {
        if (_Instance && _Instance != this)
        {
            Destroy(_Instance);
        }

        _Instance = this;
    }

    float GetHitAngle(Vector3 dir, Vector3 normal)
    {
        float angle = 0;
        if (Vector3.Dot(dir, normal) >= 0f)
        {
            angle = Vector3.Angle(dir, normal);
        }
        else
        {
            angle = Vector3.Angle(-dir, normal);
        }

        return angle;
    }

    void _SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info, VisualDamageNode visual, int seed)
    {
        
        Random.InitState(seed);
        
        void SendMultipleShrapnel(Vector3 position, Vector3 direction, float energy, int count, float dispersion, 
            VisualDamageNode node, ShrapnelState state)
        {
            if(count <= 0)
                return;
            if(energy < SubShrapnelEnergyCost)
                return;

            print($"SendMultipleShrapnel: energy: {energy}, count: {count}, disp: {dispersion}");

            direction = direction.normalized;
            
            Vector3 getRandomDirection()
            {
                Vector3 helper = Vector3.up;
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

            for (int i = 0; i < count; i++)
            {
                Vector3 newDir = getRandomDirection();
                //print($"newDIr: {newDir}");
                ShrapnelRaycast(position + VectorKEps * direction, newDir, energy,
                    node.AddChild(position, position + newDir), state);
            }
            
        }

        void ShrapnelRaycast(Vector3 position, Vector3 direction, float energy, VisualDamageNode node,
            ShrapnelState state = ShrapnelState.Miss)
        {
            
            Vector3 ArmourBegin, ArmourEnd;

            void HandleArmourHit(Vector3 point, Vector3 direction, Vector3 normal)
            {
                float angle = GetHitAngle(direction, normal);
                if (angle >= BounceOffAngle)
                {
                    if (energy / 2 >= BounceOffEnergyCost)
                    {
                        //bounce off - create a new shrapnel with two times less energy
                        Vector3 refl = Vector3.Reflect(direction, normal);
                        ShrapnelRaycast(point,
                            refl,
                            energy / 2, 
                            node.AddChild(point, point + refl), 
                            state);
                        energy = 0f;
                    }
                    else
                    {
                        //try to go through some armour again..
                        state = ShrapnelState.InArmour;
                        ArmourBegin = point;
                    }
                }
                else
                {
                    //try to go through some armour again..
                    state = ShrapnelState.InArmour;
                    ArmourBegin =point;
                }
            }

            void EndPiercingPart(Vector3 point)
            {
                //in case there was no armour before this..
                if (ArmourBegin.HasNaN())
                {
                    throw new Exception("ArmourBegin not assigned properly!");
                }

                if (ArmourEnd.HasNaN())
                {
                    ArmourEnd = point;
                }

                node.End = ArmourEnd;
                float armourThickness = Vector3.Distance(ArmourBegin, ArmourEnd);
                energy -= armourThickness;
                
                //ArmourBegin = ArmourEnd = new Vector3(float.NaN, float.NaN, float.NaN);
            }

            void DamageDamageable(GameObject g)
            {
                IDamageable dm = g.GetComponent<IDamageable>();
                if (dm == null)
                {
                    DamageableLink dl = g.GetComponent<DamageableLink>();
                    if (!dl)
                        throw new Exception("IHittable is missing IDamageable component!");
                    dm = dl.Damageable;
                }

                float ohp = dm.HP;
                if (ohp > 0f)
                {
                    dm.TakeDamage(energy);
                    energy -= ohp;
                }
            }

            node.Start = position;
            node.End = position + direction.normalized * MaxDamageRaycastDistance;
            
            var hits = DoubleRaycasting.DoubleRaycastAll(position, direction, MaxDamageRaycastDistance, InnerLayers);

            print($"New shrapnel: position={position}, direction={direction}, energy={energy}, hitsL: {hits.Length}");

            if (hits == null || hits.Length < 1)
                return;
            
            ArmourBegin = ArmourEnd = new Vector3(float.NaN, float.NaN, float.NaN);
            
            for (int i = 0; i < hits.Length; i++)
            {
                GameObject g = hits[i].collider.gameObject;
                IHittable h = g.GetComponent<IHittable>();
                if (h == null)
                    continue;
                node.End = hits[i].point;
                switch (h.Type)
                {
                    case IHittable.HittableType.InnerArea:
                        switch (state)
                        {
                            case ShrapnelState.Miss:
                                //no additional shrapnel, continue going forward
                                state = ShrapnelState.InInner;
                                break;
                            case ShrapnelState.InArmour:
                                //armour is either penned or this shrapnel is absorbed (later)
                                
                                EndPiercingPart(hits[i].point);
                                
                                state = ShrapnelState.InInner;
                                
                                //create additional shrapnels - if armour was penned
                                SendMultipleShrapnel(hits[i].point, direction,
                                    energy * SubShrapnelEnergyCoeff,
                                    Mathf.FloorToInt(energy / EnergyPerShrapnelCost),
                                    energy / DispersionEnergyCoeff,
                                    node,
                                    state
                                );
                                break;
                            case ShrapnelState.InInner:
                                //continue the shrapnel moving
                                break;
                            default: throw new Exception("Invalid shrapnel state");
                        }

                        break;
                    case IHittable.HittableType.Armour:
                        switch (state)
                        {
                            case ShrapnelState.Miss:
                                //armour was hit (piercing start or bounce off)
                                HandleArmourHit(hits[i].point, direction, hits[i].normal);
                                break;
                            case ShrapnelState.InArmour:
                                //continue going through armour
                                ArmourEnd = hits[i].point;
                                break;
                            case ShrapnelState.InInner:
                                //either bounce off or get absorbed (later)
                                HandleArmourHit(hits[i].point, direction, hits[i].normal);
                                break;
                            default: throw new Exception("Invalid shrapnel state");
                        }

                        break;
                    case IHittable.HittableType.Damageable:
                        node.HitPoints.Add(hits[i].point);
                        switch (state)
                        {
                            case ShrapnelState.Miss:
                                
                                DamageDamageable(g);

                                break;
                            case ShrapnelState.InArmour:
                                
                                EndPiercingPart(hits[i].point);

                                state = ShrapnelState.InInner;
                                
                                DamageDamageable(g);

                                
                                break;
                            case ShrapnelState.InInner:
                                
                                DamageDamageable(g);
                                
                                break;
                            default: throw new Exception("Invalid shrapnel state");
                        }

                        break;
                    default: throw new Exception("Invalid IHittable type");
                }
                if (energy <= 0f)
                {
                    return;
                }
            }
        }
        
        ShrapnelRaycast(info.HitPosition, info.HitDirection, info.Energy, visual);
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info, VisualDamageNode visual,
        int seed)
    {
        Instance._SimulateDamage(info, visual, seed);
    }
}