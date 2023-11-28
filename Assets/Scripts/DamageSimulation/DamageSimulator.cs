using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Projectiles.ProjectileDataBuffer_Kinematic;
using UnityEngine;

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
    
    private enum ShrapnelState
    {
        Miss,
        InArmour,
        InInner
    }
    
    private void Awake()
    {
        if (_Instance)
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

        void SendMultipleShrapnel(Vector3 position, Vector3 direction, int count, float dispersion)
        {
            
        }

        void ShrapnelRaycast(Vector3 position, Vector3 direction, float piercingEnergy, float hitEnergy,
            ShrapnelState state = ShrapnelState.Miss)
        {
            var hits = DoubleRaycasting.DoubleRaycastAll(info.HitPosition, info.HitDirection, 
                        MaxDamageRaycastDistance, InnerLayers);
                    
                    if(hits == null || hits.Length < 1)
                        return;
            
                    Vector3 ArmourBegin = default, ArmourEnd = default;
            
                    for (int i = 0; i < hits.Length; i++)
                    {
                        GameObject g = hits[i].collider.gameObject;
                        IHittable h = g.GetComponent<IHittable>();
                        if(h == null)
                            continue;
                        switch (h.Type)
                        {
                            case IHittable.HittableType.InnerArea:
                                switch (state)
                                {
                                    case ShrapnelState.Miss:
                                        //no additional shrapnel, continue going forward
                                        break;
                                    case ShrapnelState.InArmour:
                                        //armour is either penned or this shrapnel is absorbed (later)
                                        float armourThickness = Vector3.Distance(ArmourBegin, ArmourEnd);
                                        float energyLoss = piercingEnergy - armourThickness;
                                        if (energyLoss <= 0f)
                                        {
                                            //absorbed
                                            return;
                                        }
                                        piercingEnergy -= armourThickness;
                                        
                                        if (ArmourBegin == default && ArmourEnd == default)
                                        {
                                            continue;
                                        }
                                        //create additional shrapnels - if armour was penned
                                        SendMultipleShrapnel(hits[i].point, direction, 
                                            Mathf.Max(1, Mathf.FloorToInt(piercingEnergy / EnergyPerShrapnelCost)),
                                            piercingEnergy / DispersionEnergyCoeff
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
                                        //armour was hit (piercing start)
                                        state = ShrapnelState.InArmour;
                                        ArmourBegin = hits[i].point;
                                        break;
                                    case ShrapnelState.InArmour:
                                        //continue going through armour
                                        ArmourEnd = hits[i].point;
                                        break;
                                    case ShrapnelState.InInner:
                                        //either bounce off or get absorbed (later)
                                        float angle = GetHitAngle(direction, hits[i].normal);
                                        if (angle >= BounceOffAngle)
                                        {
                                            if ((piercingEnergy + hitEnergy / 2) / 2 >= BounceOffEnergyCost)
                                            {
                                                //bounce off - create a new shrapnel with two times less energy
                                                ShrapnelRaycast(hits[i].point, 
                                                    Vector3.Reflect(direction, hits[i].normal), 
                                                    piercingEnergy / 2, hitEnergy / 4,
                                                    state);
                                            }
                                            else
                                            {
                                                //try to go through some armour again..
                                                state = ShrapnelState.InArmour;
                                                ArmourBegin = hits[i].point;
                                            }
                                        }
                                        else
                                        {
                                            //try to go through some armour again..
                                            state = ShrapnelState.InArmour;
                                            ArmourBegin = hits[i].point;
                                        }
                                        break;
                                    default: throw new Exception("Invalid shrapnel state");
                                }
                                break;
                            case IHittable.HittableType.Damageable:
                                
                                switch (state)
                                {
                                    //TODO
                                    case ShrapnelState.Miss:
                                        break;
                                    case ShrapnelState.InArmour:
                                        break;
                                    case ShrapnelState.InInner:
                                        break;
                                    default: throw new Exception("Invalid shrapnel state");
                                }
                                break;
                            default: throw new Exception("Invalid IHittable type");
                        }
                    }
        }
        
        


    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info)
    {
        Instance._SimulateDamage(info);
    }
    
}
