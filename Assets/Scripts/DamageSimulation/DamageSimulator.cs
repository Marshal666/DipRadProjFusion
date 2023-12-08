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

    //for small energy shrapnels
    public float BounceOffAltMaxEnergy = 30f;
    public float BounceOffSubAngleProbabilityMin = 0f;
    public float BounceOffSubAngleMin = 40f;
    public float BounceOffSubAngleProbabilityMax = 0.8f;
    public float BounceOffSubAngleMax = 90f;

    public float BounceOffEnergyCost = 40f;

    public float EnergyPerShrapnelCost = 30f;

    public float DispersionEnergyCoeff = 20f;

    public float SubShrapnelEnergyCoeff = 0.7f;

    public float SubShrapnelEnergyCost = 30f;

    public float ArmourEnergyCoeff = 10f;

    public float MinAngleNerfLimit = 0.01f;

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

        [Serializable]
        public struct ShrapnelSpawnData
        {
            public float Distance;
            public int Count;
        }

        public Vector3 Start;
        public Vector3 End;

        public List<(Vector3 point, float dist)> HitPoints;

        public List<(Vector3 pt, bool end)> ArmourPoints;
        
        public List<VisualDamageNode> Children;

        public VisualDamageNode Parent;

        public List<ShrapnelSpawnData> SpawnData;

        [NonSerialized]
        public float Dist;
        [NonSerialized]
        public Vector3 Dir;

        public VisualDamageNode(Vector3 start, Vector3 end, VisualDamageNode parent = null)
        {
            Start = start;
            End = end;

            Parent = parent;

            Children = new List<VisualDamageNode>(8);
            HitPoints = new List<(Vector3, float)>(8);
            ArmourPoints = new List<(Vector3 pt, bool end)>(8);
            SpawnData = new List<ShrapnelSpawnData>(4);
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

    void _SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info, VisualDamageNode visual, int seed, ref PlayerTankController tank)
    {
        
        Random.InitState(seed);

        GameObject objg = null;
        
        void SendMultipleShrapnel(Vector3 position, Vector3 direction, float energy, int count, float dispersion, 
            VisualDamageNode node, ShrapnelState state)
        {
            if(count <= 0)
                return;
            if(energy < SubShrapnelEnergyCost)
                return;

            //print($"SendMultipleShrapnel: energy: {energy}, count: {count}, disp: {dispersion}");

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
            
            Vector3 ArmourBegin, ArmourEnd, ArmourNormal;

            void HandleArmourHit(Vector3 point, Vector3 direction, Vector3 normal)
            {
                //print($"Enter piercing: {energy}");
                float angle = GetHitAngle(direction, normal);
                float angleProb = Utils.MapInterval(angle, BounceOffSubAngleMin, BounceOffSubAngleMax, BounceOffSubAngleProbabilityMin, BounceOffSubAngleProbabilityMax);
                bool bounceOffAlt = 
                    energy <= BounceOffAltMaxEnergy 
                    && Random.value >= angleProb;
                //print($"Bounce off alt: {bounceOffAlt}, aprob: {angleProb}, angle: {angle}");
                if (angle >= BounceOffAngle || bounceOffAlt)
                {
                    if (energy / 2 >= BounceOffEnergyCost)
                    {
                        //bounce off - create a new shrapnel with two times less energy
                        Vector3 refl = Vector3.Reflect(direction, normal);
                        Vector3 spoint = point + normal.normalized * VectorKEps;
                        ShrapnelRaycast(spoint,
                            refl,
                            energy / 2, 
                            node.AddChild(spoint, spoint + refl), 
                            state);
                        energy = 0f;
                        node.SpawnData.Add(new VisualDamageNode.ShrapnelSpawnData() { Count = 1, Distance = Vector3.Distance(node.Start, node.End) });
                    }
                    else
                    {
                        //try to go through some armour again..
                        state = ShrapnelState.InArmour;
                        ArmourBegin = point;
                        ArmourNormal = normal;
                        node.ArmourPoints.Add((ArmourBegin, false));
                    }
                }
                else
                {
                    //try to go through some armour again..
                    state = ShrapnelState.InArmour;
                    ArmourBegin = point;
                    ArmourNormal = normal;
                    node.ArmourPoints.Add((ArmourBegin, false));
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
                node.ArmourPoints.Add((ArmourEnd, true));
                float armourThickness = Vector3.Distance(ArmourBegin, ArmourEnd);
                float angle = GetHitAngle(direction, ArmourNormal);
                float oe = energy;
                float energyLoss = armourThickness * ArmourEnergyCoeff * Mathf.Max(MinAngleNerfLimit, Mathf.Cos(angle * Mathf.Deg2Rad));
                energy -= energyLoss;

                //print($"End piercing: thickness: {armourThickness}, energy before: {oe}, energy now: {energy}, loss: {energyLoss}");

                ArmourBegin = ArmourEnd = ArmourNormal = new Vector3(float.NaN, float.NaN, float.NaN);
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

            //print($"New shrapnel: position={position}, direction={direction}, energy={energy}, hitsL: {hits.Length}");

            if (hits == null || hits.Length < 1)
                return;

            if(objg == null)
            {
                objg = hits[0].collider.gameObject;
            }
            
            ArmourBegin = ArmourNormal = ArmourEnd = new Vector3(float.NaN, float.NaN, float.NaN);
            
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

                                if (energy <= 0f)
                                    //absorbed
                                    return;
                                
                                state = ShrapnelState.InInner;

                                //create additional shrapnels - if armour was penned
                                int count = Mathf.FloorToInt(energy / EnergyPerShrapnelCost);
                                SendMultipleShrapnel(hits[i].point, direction,
                                    energy * SubShrapnelEnergyCoeff,
                                    count,
                                    energy / DispersionEnergyCoeff,
                                    node,
                                    state
                                );
                                node.SpawnData.Add(new VisualDamageNode.ShrapnelSpawnData() { Count = count, Distance = Vector3.Distance(node.Start, node.End) });
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
                                node.ArmourPoints.Add((ArmourEnd, true));
                                break;
                            case ShrapnelState.InInner:
                                //either bounce off or get absorbed (later)
                                HandleArmourHit(hits[i].point, direction, hits[i].normal);
                                break;
                            default: throw new Exception("Invalid shrapnel state");
                        }

                        break;
                    case IHittable.HittableType.Damageable:
                        node.HitPoints.Add((hits[i].point, Vector3.Distance(node.Start, node.End)));
                        switch (state)
                        {
                            case ShrapnelState.Miss:
                                
                                DamageDamageable(g);

                                break;
                            case ShrapnelState.InArmour:
                                
                                EndPiercingPart(hits[i].point);

                                if (energy <= 0f)
                                    return;

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

        if(objg && !tank)
        {
            tank = objg.GetComponentInParent<PlayerTankController>();
            if(tank == null)
            {
                tank = objg.GetComponent<PlayerTankController>();
            }
        }
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SimulateDamage(KinematicProjectileDataBuffer.ProjectileHitInfo info, VisualDamageNode visual,
        int seed, ref PlayerTankController tank)
    {
        Instance._SimulateDamage(info, visual, seed, ref tank);
    }
}