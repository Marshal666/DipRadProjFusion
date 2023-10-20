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

    public bool HasHorizontalConstraints = false;
    public float HorizontalConstraintMin = 0f;
    public float HorizontalConstraintMax = 360f;

    public bool HasVerticalConstraints = true;
    public VerticalConstraint[] VerticalConstraints;

    public float HorizontalRotationSpeedMax = 270f;
    public float HorizontalRotationAcceleration = 180f;
    public float CurrentHorizontalRotationSpeed = 0f;
    public Axis HorizontalRotationAxis = Axis.Y;

    public float VerticalRotationSpeedMax = 90f;
    public float VerticalRotationAcceleration = 90f;
    public float CurrentVerticalRotationSpeed = 0f;
    public float VerticalRotationOffset = 90f;
    public float VerticalPlacementOffset = 30f;
    public Axis VerticalRotationAxis = Axis.X;

    public bool MainTurret = true;

    [Networked]
    public float TurretMx { get; set; }
    [Networked]
    public float TurretMy { get; set; }

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

    public override void FixedUpdateNetwork()
    {
        SetRotation(VerticalRotatePart, VerticalRotationAxis, TurretMx);
        SetRotation(HorizontalRotatePart, HorizontalRotationAxis, TurretMy);
        if (GetInput(out NetworkInputData data))
        {
            if (float.IsNaN(data.MX) || float.IsNaN(data.MY))
            {
                CurrentHorizontalRotationSpeed = 0f;
                CurrentVerticalRotationSpeed = 0f;
                return;
            }

            float currentHorizontal = Utils.NormalizeAngle360(AxisValueFromQuaternion(HorizontalRotationAxis, HorizontalRotatePart.localRotation));
            float currentVertical = Utils.NormalizeAngle360(-AxisValueFromQuaternion(VerticalRotationAxis, VerticalRotatePart.transform.localRotation)/* - VerticalRotationOffset*/);

            //Vertical rotation part
            float my = Utils.NormalizeAngle360(VerticalPlacementOffset - data.MY);
            int constraintIndex = VerticalConstraintAngleIndex(currentHorizontal);
            float vmin = VerticalConstraints[constraintIndex].VerticalMin;
            float vmax = VerticalConstraints[constraintIndex].VerticalMax;
            my = Utils.NormalizeAngle360(Utils.ClampAngleLPositive(my, vmin, vmax));
            
            if (currentVertical != my)
            {
                CurrentVerticalRotationSpeed = Mathf.Clamp(CurrentVerticalRotationSpeed + VerticalRotationAcceleration * Runner.DeltaTime, 0, VerticalRotationSpeedMax);
                currentVertical = Mathf.MoveTowardsAngle(currentVertical, my, CurrentVerticalRotationSpeed * Runner.DeltaTime);
                SetRotation(VerticalRotatePart, VerticalRotationAxis, -currentVertical);
                TurretMx = -currentVertical;
            } else
            {
                CurrentVerticalRotationSpeed = 0f;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Horizontal Rotation part...
            float baseRotation = Base ? Base.transform.eulerAngles.y : Tank.transform.eulerAngles.y;
            //float targetDirAngle = Utils.ClampAngleLPositive(data.MX, HorizontalConstraintMin, HorizontalConstraintMax);
            float mx = Utils.NormalizeAngle360(data.MX - baseRotation);
            if (HasHorizontalConstraints)
                mx = Utils.ClampAngleLPositive(mx, HorizontalConstraintMin, HorizontalConstraintMax);
            //print($"my: {mx}, cy: {cy}");
            //float localCurrent = Utils.NormalizeAngle360(currentHorizontal + baseRotation);
            //float localMX = Utils.NormalizeAngle360(mx + baseRotation);
            //print($"currentHor: {currentHorizontal}, mx: {mx}, localCur: {localCurrent}, localMX: {localMX}");
            if(currentHorizontal != mx)
            {
                CurrentHorizontalRotationSpeed = Mathf.Clamp(CurrentHorizontalRotationSpeed + HorizontalRotationAcceleration * Runner.DeltaTime, 0, HorizontalRotationSpeedMax);
                float angleMovement = Utils.NormalizeAngle360(Mathf.MoveTowardsAngle(currentHorizontal, mx, CurrentHorizontalRotationSpeed * Runner.DeltaTime));
                //float am1 = angleMovement;
                if (HasHorizontalConstraints)
                {
                    float delta = Utils.NormalizeAngle360(angleMovement - currentHorizontal);
                    float nearEndStep = Utils.NormalizeAngle360(Mathf.LerpAngle(currentHorizontal, mx, 0.99f));
                    //bool fv = true;
                    if (nearEndStep != Utils.ClampAngleLPositive(nearEndStep, HorizontalConstraintMin, HorizontalConstraintMax))
                    {
                        angleMovement = Utils.NormalizeAngle360(angleMovement - 2 * delta);
                        //fv = false;
                    }
                    //print($"Target: {mx}, aM1: {am1}, aM2: {angleMovement}, delta: {delta}, 1st valid: {fv}, 2nd valid: {angleMovement == Utils.ClampAngleLPositive(angleMovement, HorizontalConstraintMin, HorizontalConstraintMax)}");
                }
                if (!HasHorizontalConstraints || angleMovement == Utils.ClampAngleLPositive(angleMovement, HorizontalConstraintMin, HorizontalConstraintMax))
                {
                    currentHorizontal = angleMovement;
                }
                SetRotation(HorizontalRotatePart, HorizontalRotationAxis, currentHorizontal);
                TurretMy = currentHorizontal;
            } else
            {
                CurrentHorizontalRotationSpeed = 0f;
            }
        }
    }
}
