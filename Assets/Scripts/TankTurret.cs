using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : NetworkBehaviour
{

    public enum Axis
    {
        X, Y, Z
    }

    [System.Serializable]
    public struct VerticalConstraint
    {
        public float HorizontalStart;
        public float HorizontalEnd;

        public float VerticalMin;
        public float VerticalMax;
    }

    public Transform HorizontalRotatePart;

    public Transform VerticalRotatePart;

    /// <summary>
    /// Turrets that this one contains "above"
    /// </summary>
    public TankTurret[] AboveTurrets;

    /// <summary>
    /// The turret on which this one is placed on
    /// </summary>
    public TankTurret Base = null;

    /// <summary>
    /// Tank this turret belongs to
    /// </summary>
    public PlayerTankController Tank;

    public Camera SniperModeCamera;

    public bool HasHorizontalConstraints = false;
    public float HorizontalConstraintMin = 0f;
    public float HorizontalConstraintMax = 360f;

    public bool HasVerticalConstraints = true;
    public VerticalConstraint[] VerticalConstraints;

    public float HorizontalRotationSpeedMax = 270f;
    public float HorizontalRotationSpeedInit = 5f;
    public float HorizontalRotationAcceleration = 180f;
    public float CurrentHorizontalRotationSpeed = 0f;
    public Axis HorizontalRotationAxis = Axis.Y;

    public float VerticalRotationSpeedMax = 90f;
    public float VerticalRotationSpeedInit = 5f;
    public float VerticalRotationAcceleration = 90f;
    public float CurrentVerticalRotationSpeed = 0f;
    public float VerticalRotationOffset = 90f;
    public float VerticalPlacementOffset = 30f;
    public Axis VerticalRotationAxis = Axis.X;

    public bool HasSniperMode = true;

    public float RotationSmoothingSpeed = 720f;

    public float RotationkEps = 0.00001f;

    public TankWeapon[] Weapons;

    [Networked]
    public float TurretMx { get; set; }
    [Networked]
    public float TurretMy { get; set; }

    public bool IsMainTurret => Tank.MainTurret == this;

    public float lmx = 0f, lmy = 0f;

    public Vector3 Axis2Vector3(Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return Vector3.right;
            case Axis.Y:
                return Vector3.up;
            case Axis.Z:
                return Vector3.forward;
            default:
                throw new System.ArgumentException("axis");
        }
    }

    public float AxisValueFromVector3(Axis axis, Vector3 v)
    {
        switch (axis)
        {
            case Axis.X:
                return v.x;
            case Axis.Y:
                return v.y;
            case Axis.Z:
                return v.z;
            default:
                throw new System.ArgumentException("axis");
        }
    }

    public float AxisValueFromQuaternion(Axis axis, Quaternion q)
    {
        return AxisValueFromVector3(axis, q.eulerAngles);
    }

    public void SetRotation(Transform rot, Axis axis, float angle, bool local = true)
    {
        Vector3 v = Axis2Vector3(axis);
        if (local)
            rot.localRotation = Quaternion.Euler(v * angle);
        else
            rot.rotation = Quaternion.Euler(v * angle);
    }

    public int VerticalConstraintAngleIndex(float a)
    {
        for(int i = 0; i < VerticalConstraints.Length; i++)
        {
            if(a == Utils.ClampAngleLPositive(a, VerticalConstraints[i].HorizontalStart, VerticalConstraints[i].HorizontalEnd))
            {
                return i;
            }
        }
        throw new System.Exception("No vertical constraint for current angle!");
    }

    public override void Spawned()
    {
        
    }

    public void RotateTurretToAngles(float imx, float imy, float cmx, float cmy, float dt, out float omx, out float omy)
    {
        if (float.IsNaN(imx) || float.IsNaN(imy) || !Tank.HasGunner)
        {
            CurrentHorizontalRotationSpeed = 0f;
            CurrentVerticalRotationSpeed = 0f;
            omx = cmx;
            omy = cmy;
            return;
        }

        float currentHorizontal = Utils.NormalizeAngle360(cmy /*AxisValueFromQuaternion(HorizontalRotationAxis, HorizontalRotatePart.localRotation)*/);
        float currentVertical = Utils.NormalizeAngle360(-cmx /*-AxisValueFromQuaternion(VerticalRotationAxis, VerticalRotatePart.transform.localRotation)*/ /* - VerticalRotationOffset*/);

        //Vertical rotation part
        float my = Utils.NormalizeAngle360(VerticalPlacementOffset - imy);
        int constraintIndex = VerticalConstraintAngleIndex(currentHorizontal);
        float vmin = VerticalConstraints[constraintIndex].VerticalMin;
        float vmax = VerticalConstraints[constraintIndex].VerticalMax;
        my = Utils.NormalizeAngle360(Utils.ClampAngleLPositive(my, vmin, vmax));

        if (Mathf.Abs(Mathf.DeltaAngle(currentVertical, my)) > RotationkEps /*currentVertical != my*/)
        {
            if (CurrentVerticalRotationSpeed == 0f)
            {
                CurrentVerticalRotationSpeed = VerticalRotationSpeedInit;
            }
            CurrentVerticalRotationSpeed = Mathf.Clamp(CurrentVerticalRotationSpeed + VerticalRotationAcceleration * dt, 0, VerticalRotationSpeedMax);
            currentVertical = Mathf.MoveTowardsAngle(currentVertical, my, CurrentVerticalRotationSpeed * dt);
            //SetRotation(VerticalRotatePart, VerticalRotationAxis, -currentVertical);
            omx = -currentVertical;
            lmx = -currentVertical;
        }
        else
        {
            CurrentVerticalRotationSpeed = 0f;
            lmxv = 0f;
            omx = cmx;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Horizontal Rotation part...
        float baseRotation = Base ? Base.transform.eulerAngles.y : Tank.transform.eulerAngles.y;
        float mx = Utils.NormalizeAngle360(imx - baseRotation);
        if (HasHorizontalConstraints)
            mx = Utils.ClampAngleLPositive(mx, HorizontalConstraintMin, HorizontalConstraintMax);
        if (Mathf.Abs(Mathf.DeltaAngle(currentHorizontal, mx)) > RotationkEps/*currentHorizontal != mx*/)
        {
            //print($"mx ({mx}) != current ({currentHorizontal})");
            if (CurrentHorizontalRotationSpeed == 0f)
            {
                CurrentHorizontalRotationSpeed = HorizontalRotationSpeedInit;
            }
            CurrentHorizontalRotationSpeed = Mathf.Clamp(CurrentHorizontalRotationSpeed + HorizontalRotationAcceleration * dt, 0, HorizontalRotationSpeedMax);
            float angleMovement = Utils.NormalizeAngle360(Mathf.MoveTowardsAngle(currentHorizontal, mx, CurrentHorizontalRotationSpeed * dt));
            if (HasHorizontalConstraints)
            {
                float delta = Utils.NormalizeAngle360(angleMovement - currentHorizontal);
                float nearEndStep = Utils.NormalizeAngle360(Mathf.LerpAngle(currentHorizontal, mx, 0.99f));
                if (nearEndStep != Utils.ClampAngleLPositive(nearEndStep, HorizontalConstraintMin, HorizontalConstraintMax))
                {
                    angleMovement = Utils.NormalizeAngle360(angleMovement - 2 * delta);
                }
            }
            if (!HasHorizontalConstraints || angleMovement == Utils.ClampAngleLPositive(angleMovement, HorizontalConstraintMin, HorizontalConstraintMax))
            {
                currentHorizontal = angleMovement;
            }
            //SetRotation(HorizontalRotatePart, HorizontalRotationAxis, currentHorizontal);
            omy = currentHorizontal;
            lmy = currentHorizontal;
        }
        else
        {
            omy = cmy;
            CurrentHorizontalRotationSpeed = 0f;
            lmyv = 0f;
        }
    }

    public override void FixedUpdateNetwork()
    {
        //SetRotation(VerticalRotatePart, VerticalRotationAxis, TurretMx);
        //SetRotation(HorizontalRotatePart, HorizontalRotationAxis, TurretMy);
        //print($"player: {Object.InputAuthority} tick: {Runner.Tick}, dt: {Runner.DeltaTime}");
        bool gi = GetInput(out NetworkInputData data);
        //print($"local player: {Runner.LocalPlayer}, ia: {Object.InputAuthority}, sa: {Object.StateAuthority}, tia: {Tank.HasInputAuthority}, gi: {gi}");
        if (gi && Tank.IsHostPlayer)
        {
            float omx, omy;
            RotateTurretToAngles(data.MX, data.MY, TurretMx, TurretMy, Runner.DeltaTime, out omx, out omy);
            TurretMx = omx;
            TurretMy = omy;
            //print($"NFU x_ok: {Mathf.Abs(lmx - TurretMx) <= 5f}, y_ok: {Mathf.Abs(lmy - TurretMy) <= 5f} lmx: {lmx}, lmy: {lmy}, TurretMx: {TurretMx}, TurretMy: {TurretMy}");
        } else
        {
            //lmx = TurretMx;
            //lmy = TurretMy;
        }
    }

    public void ResetRotation()
    {
        TurretMx = 0f;
        TurretMy = 0f;

        if (InSniperMode)
        {
            ToggleSniperMode();
        }

        Rotate();
    }

    public void Rotate(bool interpolate = false, float delta = 0.01f)
    {

        if (interpolate)
        {
            lmx = Mathf.MoveTowardsAngle(lmx, TurretMx, 180f * delta);
            lmy = Mathf.MoveTowardsAngle(lmy, TurretMy, 180f * delta);
        }
        else
        {
            //works fine, but it's not super smooth on clients..
            lmx = TurretMx;
            lmy = TurretMy;
        }

        SetRotation(VerticalRotatePart, VerticalRotationAxis, lmx);
        SetRotation(HorizontalRotatePart, HorizontalRotationAxis, lmy);

    }

    public override void Render()
    {

        //Causes jittering when doing hull rotation
        //lmx = Mathf.SmoothDampAngle(lmx, TurretMx, ref lmxv, RotationSmoothingTime);
        //lmy = Mathf.SmoothDampAngle(lmy, TurretMy, ref lmyv, RotationSmoothingTime);

        //lmx = Mathf.MoveTowardsAngle(lmx, TurretMx, RotationSmoothingSpeed * Time.deltaTime);
        //lmy = Mathf.MoveTowardsAngle(lmy, TurretMy, RotationSmoothingSpeed * Time.deltaTime);

        if (Tank.IsHostPlayer)
        {
            Rotate();
        } else
        {
            Rotate(true, Runner.DeltaTime);
        }
    }

    public void SetSniperCameraActive(bool val)
    {
        if (!SniperModeCamera)
            return;
        SniperModeCamera.gameObject.SetActive(val);
        SniperModeCamera.GetComponent<AudioListener>().enabled = val;
    }

    bool InSniperMode = false;

    public void ToggleSniperMode()
    {
        InSniperMode = !InSniperMode;
        SetSniperCameraActive(InSniperMode);
        PlayerCamera.Instance.SetSniperModeParams(InSniperMode, SniperModeCamera);
        Tank.SetRenderersEnabled(!InSniperMode);
        UIManager.SetSniperModeUIEnabled(InSniperMode);
        //UIManager.SetAimingObjectsActive(!InSniperMode);
    }

    
    float lmxv = 0f, lmyv = 0f;

    private void Update()
    {
        if (Tank.HasInputAuthority && HasSniperMode)
        {
            //print($"U x_ok: {Mathf.Abs(lmx - TurretMx) <= 5f}, y_ok: {Mathf.Abs(lmy - TurretMy) <= 5f} lmx: {lmx}, lmy: {lmy}, TurretMx: {TurretMx}, TurretMy: {TurretMy}");
            if (Input.GetKeyDown(KeyCode.LeftShift) && SniperModeCamera && !Tank.IsDeadWorthy())
            {
                ToggleSniperMode();
            }
        }

        if (Tank && Tank.Object && Tank.IsHostPlayer)
        {
            Rotate();
        }
        else if(Tank && Tank.Object)
        {
            Rotate(true, Time.deltaTime);
        }
    }

}
