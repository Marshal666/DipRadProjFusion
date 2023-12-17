using ExitGames.Client.Photon;
using Fusion;
using Fusion.Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerTankController : NetworkBehaviour
{

    #region HEALTH_PARTS

    public enum TankHealthBits
    {
        Driver = 0,
        Gunner = 1,
        Loader = 2,
        Commander = 3,
        Engine = 4,
        GunBreech = 5,
        GunBarrel = 6,
        TrackL = 7,
        TrackR = 8,
    }

    [Serializable]
    public struct HealthDamageables
    {
        public TankHealthBits Part;
        public GameObject Object;
    }

    public HealthDamageables[] _DamageableParts;

    Dictionary<TankHealthBits, IDamageable> DamageableParts;

    [Networked, Capacity(16)]
    public NetworkArray<NetworkBool> TankHealth { get; }

    public ObjectHolder ExplosionsHolder;

    public void SetTankHealth(TankHealthBits bit, bool val)
    {
        TankHealth.Set((int)bit, val);
        if (val)
        {
            DamageableParts[bit].Restore();
        } else
        {
            DamageableParts[bit].HP = 0f;
        }
        if (IsCurrentPlayer)
        {
            UIManager.SetHealthItemState(bit, val);
        }
    }

    public bool GetTankHealth(TankHealthBits bit)
    {
        return TankHealth.Get((int)bit);
    }

    public TankHealthBits? GetAliveNonEssentialMemeber()
    {
        if (HasCommander)
            return TankHealthBits.Commander;
        if (HasLoader)
            return TankHealthBits.Loader;
        return null;
    }

    TankHealthBits[] HealthValues = (TankHealthBits[])Enum.GetValues(typeof(TankHealthBits));

    TankHealthBits[] FixableComponents = new TankHealthBits[] { TankHealthBits.Engine, TankHealthBits.GunBreech, TankHealthBits.GunBarrel, TankHealthBits.TrackL, TankHealthBits.TrackR };

    public float ComponentFixTime = 5f;

    public float CrewmanRecoveryTime = 4f;

    public float RespawnTime = 10f;

    public void SetHealthStats(bool h)
    {
        var vals = HealthValues;
        foreach (var val in vals)
        {
            SetTankHealth(val, h);
        }
        droot.RestoreAll();
    }

    public void FullyHealTank()
    {
        SetHealthStats(true);
        Dead = false;
        JustDied = false;
        Ghost.SetMaterial(GhostEffectObject.MaterialType.Original);
    }

    public void KillTank()
    {
        SetHealthStats(false);
        Dead = true;
        JustDied = true;
    }

    [Networked, Capacity(16)]
    public NetworkArray<TickTimer> RepairTimes { get; } = MakeInitializer(Enumerable.Repeat(TickTimer.None, 16).ToArray());

    [Networked]
    public TickTimer DriverRecovery { get; set; } = TickTimer.None;
    [Networked]
    public NetworkBool DriverRecovering { get; set; } = false;

    [Networked]
    public TickTimer GunnerRecovery { get; set; } = TickTimer.None;
    [Networked]
    public NetworkBool GunnerRecovering { get; set; } = false;

    [Networked]
    public TickTimer RespawnTimer { get; set;} = TickTimer.None;
    [Networked]
    public NetworkBool Respawning { get; set; } = false;

    [Networked]
    public TickTimer SuicideTimer { get; set; } = TickTimer.None;

    public bool ComponentRepaired(TankHealthBits bit)
    {
        return RepairTimes.Get((int)bit).Expired(Runner);
    }

    public void SetRepairTime(TankHealthBits bit, float val)
    {
        RepairTimes.Set((int)bit, TickTimer.CreateFromSeconds(Runner, val));
    }

    public TickTimer GetRepairTimer(TankHealthBits bit)
    {
        return RepairTimes.Get((int)bit);
    }

    public bool HasDriver => TankHealth[0];
    public bool HasGunner => TankHealth[1];
    public bool HasLoader => TankHealth[2];
    public bool HasCommander => TankHealth[3];
    public bool HasEngine => TankHealth[4];
    public bool HasGunBreech => TankHealth[5];
    public bool HasGunBarrel => TankHealth[6];
    public bool HasTracks => TankHealth[7] & TankHealth[8];

    [Networked]
    NetworkBool Dead { get; set; } = false;

    public bool JustDied = false;

    public int AliveCrewCount
    {
        get
        {
            int ret = 0;
            if (HasDriver) ret++;
            if (HasGunner) ret++;
            if (HasLoader) ret++;
            if (HasCommander) ret++;
            return ret;
        }
    }

    public bool IsDeadWorthy()
    {
        if (AliveCrewCount < 2)
            return true; 
        return false;
    }

    public void DieIfNeeded()
    {
        if(IsDeadWorthy())
        {
            Die();
        }
    }

    public void Die()
    {
        if (Dead)
            return;
        KillTank();
    }

    public void Respawn()
    {
        FullyHealTank();
        CurrentRotationSpeed = 0;
        CurrentTraverseSpeed = 0;
        MaxLeftWheelRotation = 0;
        MaxRightWheelRotation = 0;
        rig.WriteVelocity(Vector3.zero);
        var point = BasicSpawner.Instance.GetSpawnPointRespawn();
        //rig.WritePosition(point.Position);
        //rig.TeleportToPosition(point.Position);
        //rig.WriteRotation(Quaternion.Euler(point.Rotation));
        rig.TeleportToPositionRotation(point.Position, Quaternion.Euler(point.Rotation));
        foreach(var turret in Turrets)
        {
            turret.ResetRotation();
        }
        UIManager.SetYouDiedTextEnabled(false);
    }

    #endregion

    private NetworkRigidbody rig;

    private NetworkObject nobj;

    /// <summary>
    /// Even indexes - left wheels,
    /// Odd indexes - right wheels
    /// </summary>
    [Tooltip("Even indexes - left wheels, Odd indexes - right wheels")]
    public WheelCollider[] Wheels;

    public TankTurret[] Turrets;

    public TankTurret MainTurret;

    public Enabler[] Renderers;

    public Enabler[] XRayRenderers;

    public Transform[] ObjectTransforms;

    public GhostEffectObject Ghost;

    public DamageModel DamageModel;
    
    /// <summary>
    /// Even indexes - left tracks,
    /// Odd indexes - right tracks
    /// </summary>
    public TrackObject[] Tracks;

    /// <summary>
    /// Even indexes - left wheels,
    /// Odd indexes - right wheels
    /// </summary>
    [Tooltip("Even indexes - left wheels, Odd indexes - right wheels")]
    public WheelCollider[] Sprockets;

    public enum WheelSide
    {
        Left = 0,
        Right = 1
    }

    public float ForwardRotationSpeedMax = 1440f;
    public float BackRotationSpeedMax = -720f;
    public float ForwardRotationAcceleration = 360f;
    public float BackRotationAcceleration = 360f;
    public float BrakeAccelerationRot = 1440f;
    [Networked]
    public float CurrentRotationSpeed { get; set; }

    public float BrakeTorque = 3000f;
    public float SteerTorque = 1200f;

    public float TraverseRotationSpeedMax = 720f;
    public float TraverseRotationAcceleration = 360f;
    public float TraverseRotationAccelerationMoving = 360f;
    public float TraverseRotationDecceleration = 720f;
    [Range(0f, 1f)]
    public float TraverseMovingCoef = 0.5f;
    [Networked]
    public float CurrentTraverseSpeed { get; set; }


    public float SleepVelocity = 0.1f;

    public float TrackRotatoionCoeff = 2f;

    public float SuicideTime = 3f;

    public bool IsCurrentPlayer => Object.HasInputAuthority;

    public bool IsHostPlayer => Object.HasStateAuthority;

    public bool Debug = false;

    DamageableRoot droot;

    HitboxRoot hroot;

    int SpawnedTick = 0;

    [Networked]
    int rngSeed { get; set; }
    UnityEngine.Random.State rngState;

    Transform DebugTextTransform;
    UnityEngine.UI.Text DebugText;

    public string GetDebugString()
    {
        string txt = $"{Object.InputAuthority}{(IsHostPlayer ? "H" : "")}\n";
        if (HasDriver) txt += "D";
        if (HasGunner) txt += "G";
        if (HasLoader) txt += "L";
        if (HasCommander) txt += "C";

        txt += "\n";
        if (HasEngine) txt += "E";
        if (HasGunBarrel) txt += "B";
        if (HasGunBreech) txt += "G";
        if (HasTracks) txt += "T";
        return txt ;
    }

    public void UpdateDebugText()
    {
        string txt = GetDebugString();

        DebugTextTransform.position = PlayerCamera.CurrentCamera.WorldToScreenPoint(transform.position);
        DebugText.text = txt;
    }

    private void Awake()
    {
        rig = GetComponent<NetworkRigidbody>();
        nobj = GetComponent<NetworkObject>();
        droot = GetComponent<DamageableRoot>();
        hroot = GetComponent<HitboxRoot>();
        if (EffectsContainer.Initialized)
        {
            ExplosionsHolder = EffectsContainer.ExplosionsHolder;
        }
        if (UIManager.Instance)
        {
            DebugTextTransform = Instantiate(UIManager.Instance.DebugTankTextPrefab, UIManager.Instance.Canvas.transform).transform;
            DebugText = DebugTextTransform.GetComponent<UnityEngine.UI.Text>();
        }
        if(!Debug && DebugText)
        {
            DebugText.gameObject.SetActive(false);
        }
        if (!Ghost)
            Ghost = GetComponent<GhostEffectObject>();
        if(_DamageableParts != null)
        {
            DamageableParts = new Dictionary<TankHealthBits, IDamageable>(_DamageableParts.Length + 4);
            foreach(var part in _DamageableParts)
            {
                DamageableParts[part.Part] = part.Object.GetComponent<IDamageable>();
            }
        }
    }

    public override void Spawned()
    {
        if (Runner && Runner.LocalPlayer == nobj.InputAuthority)
        {
            PlayerCamera.Instance.Target = transform;
            TankUIStats.Init(this);
        }
        FullyHealTank();
        rngSeed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(rngSeed);
        rngState = Random.state;

        Transform dmgmot = DamageModel.transform;
        dmgmot.SetParent(StaticConsts.RootObject.transform);
        dmgmot.Translate(StaticConsts.DamageModelSpawnOffset);

        SpawnedTick = Runner.Tick;
    }

    #region CREW_EVENTS

    public void OnDriverDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Driver");
            } 
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Driver is dead");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.Driver, false);
                DieIfNeeded();
            }
        }
    }

    public void OnGunnerDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Gunner");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Gunner is dead");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.Gunner, false);
                DieIfNeeded();
            }
        }
    }

    public void OnLoaderDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Loader");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Loader is dead");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.Loader, false);
                DieIfNeeded();
            }
        }
    }

    public void OnCommanderDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Commander");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Commander is dead");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.Commander, false);
                DieIfNeeded();
            }
        }
    }

    public void OnEngineDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Engine");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Engine got destroyed");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.Engine, false);
                SetRepairTime(TankHealthBits.Engine, ComponentFixTime);
            }
        }
    }

    public void OnGunBreechDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Gun Breech");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Gun breech is broken");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.GunBreech, false);
                SetRepairTime(TankHealthBits.GunBreech, ComponentFixTime);
            }
        }
    }

    public void OnGunBarrelDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Gun Barrel");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Gun Barrel is broken");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.GunBarrel, false);
                SetRepairTime(TankHealthBits.GunBarrel, ComponentFixTime);
            }
        }
    }

    public void OnTrackLDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Left Track");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Left Track is destroyed");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.TrackL, false);
                SetRepairTime(TankHealthBits.TrackL, ComponentFixTime);
            }
        }
    }

    public void OnTrackRDamaged(IDamageable d)
    {
        if (d.HP <= 0f)
        {
            if (!IsCurrentPlayer)
            {
                UIManager.AddDoneDmgTextMsgItem("Right Track");
            }
            else
            {
                UIManager.AddReceivedDmgTextMsgItem("Right Track is destroyed");
            }
            if (IsHostPlayer)
            {
                SetTankHealth(TankHealthBits.TrackR, false);
                SetRepairTime(TankHealthBits.TrackR, ComponentFixTime);
            }
        }
    }

    public void OnAmmoDamaged(IDamageable d)
    {
        if(d.HP <= 0f)
        {
            Random.state = rngState;
            float val = Random.value;
            if(val >= StaticConsts.ShellDeathDetonationProb)
            {
                if (!IsCurrentPlayer)
                {
                    UIManager.AddDoneDmgTextMsgItem("Ammo");
                }
                else
                {
                    UIManager.AddReceivedDmgTextMsgItem("Ammo racked");
                }
                if (IsHostPlayer)
                {
                    Die();
                }
            }
            rngState = Random.state;
        }
    }

    #endregion

    #region INFO_STUFF

    public class TransformInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public TransformInfo(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public TransformInfo(Transform t)
        {
            Position = t.position;
            Rotation = t.rotation;
            Scale = t.localScale;
        }

        public void SetToTransform(Transform t)
        {
            t.position = Position;
            t.rotation = Rotation;
            t.localScale = Scale;
        }

    }

    public TransformInfo[] GetTransformInfos()
    {
        if (ObjectTransforms == null)
            return null;
        if(ObjectTransforms.Length <= 0)
            return null;
        TransformInfo[] ret = new TransformInfo[ObjectTransforms.Length];
        for(int i = 0; i < ObjectTransforms.Length; i++)
        {
            ret[i] = new TransformInfo(ObjectTransforms[i]);
        }
        return ret;
    }

    public void SetTransformInfos(TransformInfo[] transformInfos)
    {
        if(transformInfos == null)
            return;
        if (ObjectTransforms == null)
            return;
        if (transformInfos.Length != ObjectTransforms.Length)
            throw new ArgumentException("transfromInfos.Length != ObjectTransforms.Length");
        for (int i = 0; i < ObjectTransforms.Length; i++)
        {
            transformInfos[i].SetToTransform(ObjectTransforms[i]);
        }
    }

    #endregion

    #region UI_INFO_PARAMS


    public Vector3 Velocity => rig.ReadVelocity();

    public float Speed => rig.ReadVelocity().magnitude;

    public float VelocitySign => Mathf.Sign(Vector3.Dot(transform.forward, Velocity));

    #endregion

    #region SET_METHODS

    public void SetTorque(float val)
    {
        foreach (var whell in Wheels)
        {
            whell.motorTorque = val;
        }
    }

    public void SetTorque(float val, WheelSide wheelSide)
    {
        int start = (int)wheelSide;
        for (int i = start; i < Wheels.Length; i += 2)
        {
            Wheels[i].motorTorque = val;
        }
    }

    public void SetRotation(float val)
    {
        foreach (var whell in Wheels)
        {
            whell.rotationSpeed = val;
        }
    }

    public void SetSprocketRotation(float val, WheelSide wheelSide)
    {
        int start = (int)wheelSide;
        for(int i = start; i < Sprockets.Length; i += 2)
        {
            Sprockets[i].rotationSpeed = val;
        }
    }

    public void SetRotation(float val, WheelSide wheelSide)
    {
        int start = (int)wheelSide;
        for (int i = start; i < Wheels.Length; i += 2)
        {
            Wheels[i].rotationSpeed = val;
        }
    }

    public void SetBrake(float val)
    {
        foreach(var wheel in Wheels)
        {
            wheel.brakeTorque = val;
        }
    }

    public void SetBrake(float val, WheelSide wheelSide)
    {
        int start = (int)wheelSide;
        for (int i = start; i < Wheels.Length; i += 2)
        {
            Wheels[i].brakeTorque = val;
        }
    }

    #endregion

    #region CONTROL_METHODS

    void MoveForward()
    {
        SetBrake(0);
        CurrentRotationSpeed = Mathf.Clamp(CurrentRotationSpeed + ForwardRotationAcceleration * Runner.DeltaTime, BackRotationSpeedMax, ForwardRotationSpeedMax);
        SetRotation(CurrentRotationSpeed);
        //print("Forward!");
    }

    void Brake()
    {
        CurrentRotationSpeed = Mathf.MoveTowards(CurrentRotationSpeed, 0f, BrakeAccelerationRot * Runner.DeltaTime);
        SetBrake(BrakeTorque);
        //print("Brake");
    }

    void Reverse()
    {
        SetBrake(0);
        CurrentRotationSpeed = Mathf.Clamp(CurrentRotationSpeed - BackRotationAcceleration * Runner.DeltaTime, BackRotationSpeedMax, ForwardRotationSpeedMax);
        SetRotation(CurrentRotationSpeed);
        //print("Reverse");
    }

    void SteerWhileMovingSetBrakes(WheelSide direction)
    {
        SetRotation(0, 1 - direction);
    }

    void SteerWhileMoving(bool left, bool right, bool moving = false)
    {
        if(left)
        {
            SteerWhileMovingSetBrakes(WheelSide.Right);
            CurrentTraverseSpeed = Mathf.Clamp(CurrentTraverseSpeed + (moving ? TraverseRotationAccelerationMoving : TraverseRotationAcceleration) * Runner.DeltaTime, -TraverseRotationSpeedMax, TraverseRotationSpeedMax);
        }
        else if(right)
        {
            SteerWhileMovingSetBrakes(WheelSide.Left);
            CurrentTraverseSpeed = Mathf.Clamp(CurrentTraverseSpeed - (moving ? TraverseRotationAccelerationMoving : TraverseRotationAcceleration) * Runner.DeltaTime, -TraverseRotationSpeedMax, TraverseRotationSpeedMax);
        }
    }

    void TraverseLeft(bool moving = false)
    {
        CurrentTraverseSpeed = Mathf.Clamp(CurrentTraverseSpeed + (moving ? TraverseRotationAccelerationMoving : TraverseRotationAcceleration) * Runner.DeltaTime, -TraverseRotationSpeedMax, TraverseRotationSpeedMax);
        if (CurrentTraverseSpeed > 0)
        {
            SetBrake(0);
            SetRotation(CurrentTraverseSpeed, WheelSide.Right);
            SetRotation(-CurrentTraverseSpeed, WheelSide.Left);
        }
    }

    void TraverseRight(bool moving = false)
    {
        CurrentTraverseSpeed = Mathf.Clamp(CurrentTraverseSpeed - (moving ? TraverseRotationAccelerationMoving : TraverseRotationAcceleration) * Runner.DeltaTime, -TraverseRotationSpeedMax, TraverseRotationSpeedMax);
        if (CurrentTraverseSpeed < 0)
        {
            SetBrake(0);
            SetRotation(CurrentTraverseSpeed, WheelSide.Right);
            SetRotation(-CurrentTraverseSpeed, WheelSide.Left);
        }
    }

    void TraverseLeftMoving()
    {
        CurrentRotationSpeed = Mathf.MoveTowards(CurrentRotationSpeed, 0f, BrakeAccelerationRot * Runner.DeltaTime);
        if (CurrentTraverseSpeed >= TraverseRotationSpeedMax * TraverseMovingCoef)
        {
            TraverseLeft(true);
        }
        else
        {
            SteerWhileMoving(true, false, true);
        }
    }

    void TraverseRightMoving()
    {
        CurrentRotationSpeed = Mathf.MoveTowards(CurrentRotationSpeed, 0f, BrakeAccelerationRot * Runner.DeltaTime);
        if (CurrentTraverseSpeed <= -TraverseRotationSpeedMax * TraverseMovingCoef)
        {
            TraverseRight(true);
        }
        else
        {
            SteerWhileMoving(false, true, true);
        }
    }

    void PostTraverse()
    {
        CurrentTraverseSpeed = Mathf.MoveTowards(CurrentTraverseSpeed, 0f, TraverseRotationDecceleration * Runner.DeltaTime);
        if (CurrentTraverseSpeed > 0)
        {
            SetRotation(CurrentTraverseSpeed, WheelSide.Right);
            SetRotation(-CurrentTraverseSpeed, WheelSide.Left);
        }
        else if (CurrentTraverseSpeed < 0)
        {
            SetRotation(CurrentTraverseSpeed, WheelSide.Right);
            SetRotation(-CurrentTraverseSpeed, WheelSide.Left);
        }
    }

    #endregion

    #region CONTROL_UTILS

    public void SetRenderersEnabled(bool state)
    {
        if(Renderers != null)
        {
            for(int i = 0; i < Renderers.Length; i++)
            {
                if (Renderers[i] != null)
                    Renderers[i].SetEnabled(state);
            }
        }
    }

    public void SetEnablersArrayEnabled(Enabler[] arr, bool val)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].SetEnabled(val);
            }
        }
    }

    public Transform GetHitboxDamageModel(Hitbox h)
    {
        return DamageModel.Parts[h.HitboxIndex].Target;
    }

    #endregion

    #region TRACK_CONTROL

    [Networked]
    public float MaxLeftWheelRotation { get; set; } = 0f;
    [Networked]
    public float MaxRightWheelRotation { get; set; } = 0f;

    float GetMaxWheelRotation(WheelSide side)
    {
        int start = (int)side;
        float max = 0f;
        for(int i = start; i < Wheels.Length; i += 2)
        {
            if (Mathf.Abs(Wheels[i].rotationSpeed) > Mathf.Abs(max))
                max = Wheels[i].rotationSpeed;
        }
        return max;
    }

    void UpdateMaxWheelRotation()
    {
        MaxLeftWheelRotation = GetMaxWheelRotation(WheelSide.Left);
        MaxRightWheelRotation = GetMaxWheelRotation(WheelSide.Right);
    }

    void SetTrackRotation(WheelSide side, float deltaDist)
    {
        int start = (int)side;
        for (int i = start; i < Tracks.Length; i += 2)
        {
            Tracks[i].MarchGirstOffsetOthers(deltaDist);
        }
    }

    void RotateTracks(float ls, float rs)
    {
        
        //print($"ls: {ls}, rs {rs}");

        if(ls != 0f)
        {
            SetTrackRotation(WheelSide.Left, ls * TrackRotatoionCoeff * Time.deltaTime);
        }
        if(rs != 0f)
        {
            SetTrackRotation(WheelSide.Right, rs * TrackRotatoionCoeff * Time.deltaTime);
        }
    }

    #endregion

    void RotateSprockets(float ls, float rs)
    {
        if(ls != 0f)
        {
            SetSprocketRotation(ls, WheelSide.Left);
        }
        if(rs != 0f)
        {
            SetSprocketRotation(rs, WheelSide.Right);
        }
    }

    public void SetGhostingEnabled(bool enabled, bool offline = false)
    {
        SetEnablersArrayEnabled(XRayRenderers, enabled);
        if (enabled)
        {
            Ghost.SetMaterial(GhostEffectObject.MaterialType.Ghost);
        } else
        {
            if(offline)
            {
                Ghost.SetMaterial(GhostEffectObject.MaterialType.Original);
                return;
            }
            if(IsDeadWorthy())
            {
                Ghost.SetMaterial(GhostEffectObject.MaterialType.Destroyed);
            } else
            {
                Ghost.SetMaterial(GhostEffectObject.MaterialType.Original);
            }
        }
    }

    void Update()
    {
        float ls, rs;
        ls = rs = 0f;
        if (Runner)
        {
            ls = MaxLeftWheelRotation;
            rs = MaxRightWheelRotation;
        }
        RotateTracks(ls, rs);
        RotateSprockets(ls, rs);
        
        if(Object && !Object.HasInputAuthority)
            return;
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            SetGhostingEnabled(true);
        }

        if (Input.GetKeyUp(KeyCode.O))
        {
            SetGhostingEnabled(false);
        }
    }

    public override void FixedUpdateNetwork()
    {

        if (Debug)
            UpdateDebugText();

        //updated when needed (for damage simulation)
        //if (Runner.Tick > SpawnedTick + 64)
        //    Utils.SetTankInnerColliderTargets(Runner, this, hroot, Object.InputAuthority);

        if (Dead)
        {
            //start regenerating (for respawn)
            if (!Respawning)
            {
                JustDied = true;
                Respawning = true;
                RespawnTimer = TickTimer.CreateFromSeconds(Runner, RespawnTime);
            }
            //when regeneration has ended
            if (Respawning && RespawnTimer.Expired(Runner))
            {
                Respawning = false;
                RespawnTimer = TickTimer.None;
                Respawn();
            }
            return;
        }

        if (GetInput(out NetworkInputData data))
        {

            if(data.SuicideButtonPressed && !Dead)
            {
                if(!SuicideTimer.IsRunning)
                {
                    SuicideTimer = TickTimer.CreateFromSeconds(Runner, SuicideTime);
                } else if(SuicideTimer.Expired(Runner))
                {
                    Die();
                    SuicideTimer = TickTimer.None;
                    return;
                } else if(IsCurrentPlayer)
                {
                    //Set suicide graphic
                    UIManager.SetSuicideProgress((SuicideTime - SuicideTimer.RemainingTime(Runner).Value) / SuicideTime);
                }
            } else
            {
                SuicideTimer = TickTimer.None;
                UIManager.SetSuicideProgress(0f);
            }

            if(!HasGunner && !GunnerRecovering)
            {
                GunnerRecovering = true;
                GunnerRecovery = TickTimer.CreateFromSeconds(Runner, CrewmanRecoveryTime);
            }
            if(GunnerRecovering && GunnerRecovery.Expired(Runner))
            {
                SetTankHealth(TankHealthBits.Gunner, true);
                SetTankHealth(GetAliveNonEssentialMemeber().Value, false);
                GunnerRecovering = false;
                GunnerRecovery = TickTimer.None;
            }

            if(!HasDriver && !DriverRecovering)
            {
                DriverRecovering = true;
                DriverRecovery = TickTimer.CreateFromSeconds(Runner, CrewmanRecoveryTime);
            }
            if(DriverRecovering && DriverRecovery.Expired(Runner))
            {
                SetTankHealth(TankHealthBits.Driver, true);
                SetTankHealth(GetAliveNonEssentialMemeber().Value, false);
                DriverRecovering = false;
                DriverRecovery = TickTimer.None;
            }

            //repairs
            foreach (var comp in FixableComponents)
            {
                bool h = GetTankHealth(comp);
                bool r = ComponentRepaired(comp);
                if (!h && r)
                {
                    SetTankHealth(comp, true);
                } else if(!h && IsCurrentPlayer)
                {
                    TickTimer t = GetRepairTimer(comp);
                    if (!t.ExpiredOrNotRunning(Runner))
                    {
                        float v = (ComponentFixTime - t.RemainingTime(Runner).Value) / ComponentFixTime;
                        UIManager.SetHealthItemHP(comp, v);
                    }
                }
            }

            //print($"d: {HasDriver}, e: {HasEngine}, t: {HasTracks}");
            if (!(HasDriver && HasEngine && HasTracks))
            {
                return;
            }
            
            //print($"data: {data.ArrowsInput} back: {data.ArrowsInput & NetworkInputData.BACK_BUTTON}");
            Vector3 velocity = rig.ReadVelocity();
            float cspeed = velocity.magnitude;
            Vector3 tvel = transform.worldToLocalMatrix * (velocity);
            //0 - forward, 1 - back
            int dir = Vector3.Dot(tvel, Vector3.forward) >= 0 ? 0 : 1;
            //print($"Dir: {(dir == 0 ? "Forward" : "Back")}");
            
            if(data.ForwardPressed)
            {
                CurrentRotationSpeed = Mathf.Clamp(CurrentRotationSpeed + ForwardRotationAcceleration * Runner.DeltaTime, BackRotationSpeedMax, ForwardRotationSpeedMax);
            }
            else if(data.BackPressed)
            {
                CurrentRotationSpeed = Mathf.Clamp(CurrentRotationSpeed - BackRotationAcceleration * Runner.DeltaTime, BackRotationSpeedMax, ForwardRotationSpeedMax);
            }

            //Idle state
            if(cspeed <= SleepVelocity)
            {
                //Left Rotation
                if (data.LeftPressed)
                {
                    TraverseLeft();
                }
                //Right Rotation
                else if (data.RightPressed)
                {
                    TraverseRight();
                }
                //Post rotation
                else
                {
                    PostTraverse();
                }

                //Move forward - start
                if (data.ForwardPressed)
                {
                    if (CurrentRotationSpeed >= 0)
                    {
                        MoveForward();
                    }
                    else
                    {
                        Brake();
                    }
                }
                //Move back - start
                else if (data.BackPressed)
                {
                    if (CurrentRotationSpeed <= 0)
                    {
                        Reverse();
                    }
                    else
                    {
                        Brake();
                    }
                }
                else
                {
                    CurrentRotationSpeed = Mathf.MoveTowards(CurrentRotationSpeed, 0f, BackRotationAcceleration * Runner.DeltaTime);
                }
            } else
            //Moving states
            {
                if(data.ForwardPressed)
                {
                    if (CurrentRotationSpeed >= 0)
                    {
                        MoveForward();
                    } else
                    {
                        Brake();
                    }
                    CurrentTraverseSpeed = Mathf.MoveTowards(CurrentTraverseSpeed, 0f, TraverseRotationDecceleration * Runner.DeltaTime);
                    SteerWhileMoving(data.LeftPressed, data.RightPressed);
                } else if(data.BackPressed)
                {
                    if (CurrentRotationSpeed <= 0)
                    {
                        Reverse();
                    } else
                    {
                        Brake();
                    }
                    CurrentTraverseSpeed = Mathf.MoveTowards(CurrentTraverseSpeed, 0f, TraverseRotationDecceleration * Runner.DeltaTime);
                    SteerWhileMoving(data.LeftPressed, data.RightPressed);
                } else
                {
                    CurrentRotationSpeed = Mathf.MoveTowards(CurrentRotationSpeed, 0f, BackRotationAcceleration * Runner.DeltaTime);

                    if (dir == 0)
                    {
                        if (data.LeftPressed)
                        {
                            TraverseLeftMoving();
                        }
                        //Right Rotation
                        else if (data.RightPressed)
                        {
                            TraverseRightMoving();
                        }
                    }
                    //Post rotation
                    else
                    {
                        if (CurrentTraverseSpeed > 0)
                        {
                            PostTraverse();
                        }
                    }
                }
                
            }

            UpdateMaxWheelRotation();

        } 
    }

    public override void Render()
    {
        if(JustDied)
        {
            ExplosionsHolder.GetObject().transform.position = transform.position;
            JustDied = false;
        }
        if(Dead)
        {
            Ghost.SetMaterial(GhostEffectObject.MaterialType.Destroyed);
        }
        if (IsCurrentPlayer)
        {
            foreach (var comp in HealthValues)
            {
                UIManager.SetHealthItemState(comp, TankHealth.Get((int)comp));
            }
            foreach (var comp in FixableComponents)
            {
                bool h = GetTankHealth(comp);
                bool r = ComponentRepaired(comp);
                if (!h && !r)
                {
                    TickTimer tt = GetRepairTimer(comp);
                    float? t = tt.RemainingTime(Runner);
                    if (t.HasValue)
                    {
                        float v = (ComponentFixTime - t.Value) / ComponentFixTime;
                        UIManager.SetHealthItemHP(comp, v);
                    }
                }
            }
        }
    }

}
