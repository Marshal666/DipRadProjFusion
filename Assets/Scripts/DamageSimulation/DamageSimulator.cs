using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Projectiles.ProjectileDataBuffer_Kinematic;
using UnityEngine;

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

    private enum ShrapnelState
    {
        Miss,
        InArmour,
        InInner
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

    void _SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info)
    {
        void SendMultipleShrapnel(Vector3 position, Vector3 direction, float energy, int count, float dispersion)
        {
            if(count <= 0)
                return;
            if(energy <= 0f)
                return;
            
        }

        void ShrapnelRaycast(Vector3 position, Vector3 direction, float energy,
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
                        ShrapnelRaycast(point,
                            Vector3.Reflect(direction, normal),
                            energy / 2, state);
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
                                
                float armourThickness = Vector3.Distance(ArmourBegin, ArmourEnd);
                energy -= armourThickness;
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
                dm.TakeDamage(energy);
                energy -= ohp;
            }
            
            var hits = DoubleRaycasting.DoubleRaycastAll(info.HitPosition, info.HitDirection,
                MaxDamageRaycastDistance, InnerLayers);

            if (hits == null || hits.Length < 1)
                return;

            
            ArmourBegin = ArmourEnd = new Vector3(float.NaN, float.NaN, float.NaN);
            
            for (int i = 0; i < hits.Length; i++)
            {
                GameObject g = hits[i].collider.gameObject;
                IHittable h = g.GetComponent<IHittable>();
                if (h == null)
                    continue;
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
                                
                                if (energy <= 0f)
                                {
                                    //absorbed
                                    return;
                                }
                                
                                //create additional shrapnels - if armour was penned
                                SendMultipleShrapnel(hits[i].point, direction,
                                    energy * SubShrapnelEnergyCoeff,
                                    Mathf.FloorToInt(energy / EnergyPerShrapnelCost),
                                    energy / DispersionEnergyCoeff
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

                        switch (state)
                        {
                            case ShrapnelState.Miss:
                                
                                DamageDamageable(g);

                                if (energy <= 0f)
                                    return;

                                break;
                            case ShrapnelState.InArmour:
                                
                                EndPiercingPart(hits[i].point);

                                state = ShrapnelState.InInner;
                                
                                if(energy <= 0f)
                                    return;
                                
                                DamageDamageable(g);
                                
                                if(energy <= 0f)
                                    return;
                                
                                break;
                            case ShrapnelState.InInner:
                                
                                DamageDamageable(g);

                                if (energy <= 0f)
                                    return;
                                
                                break;
                            default: throw new Exception("Invalid shrapnel state");
                        }

                        break;
                    default: throw new Exception("Invalid IHittable type");
                }
            }
        }
        
        ShrapnelRaycast(info.HitPosition, info.HitDirection, info.Energy);
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info)
    {
        Instance._SimulateDamage(info);
    }
}