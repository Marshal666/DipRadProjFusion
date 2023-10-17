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

    public struct VerticalConstraint
    {
        public float HorizontalStart;
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

    public float HorizontalConstraintMin = 0f;
    public float HorizontalConstraintMax = 360f;

    public float HorizontalRotationSpeedMax = 270f;
    public float HorizontalRotationAcceleration = 180f;
    public float CurrentHorizontalRotationSpeed = 0f;
    public Axis HorizontalRotationAxis = Axis.Y;
    
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

    public void SetRotation(Transform rot, Axis axis, float angle)
    {
        Vector3 v = Axis2Vector3(axis);
        rot.localRotation = Quaternion.Euler(v * angle);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (float.IsNaN(data.MX) || float.IsNaN(data.MY))
                return;
            //float targetDirAngle = Utils.ClampAngleLPositive(data.MX, HorizontalConstraintMin, HorizontalConstraintMax);
            float targetDirAngle = data.MX;
            float mx = Utils.NormalizeAngle360(targetDirAngle - Tank.transform.eulerAngles.y);
            float currentHorizontal = Utils.NormalizeAngle360(AxisValueFromQuaternion(HorizontalRotationAxis, HorizontalRotatePart.localRotation));
            //print($"my: {mx}, cy: {cy}");
            print($"currentHor: {currentHorizontal}, mx: {mx}");
            if(currentHorizontal != mx)
            {
                CurrentHorizontalRotationSpeed = Mathf.Clamp(CurrentHorizontalRotationSpeed + HorizontalRotationAcceleration * Runner.DeltaTime, 0, HorizontalRotationSpeedMax);
                float angleMovement = Utils.NormalizeAngle360(Mathf.MoveTowardsAngle(currentHorizontal, mx, CurrentHorizontalRotationSpeed * Runner.DeltaTime));
                //float delta = CurrentHorizontalRotationSpeed * Runner.DeltaTime;
                //bool fv = true;
                //if(angleMovement != Utils.ClampAngleLPositive(angleMovement, HorizontalConstraintMin, HorizontalConstraintMax))
                //{
                //    angleMovement = Utils.NormalizeAngle360(angleMovement - 2 * delta);
                //    //fv = false;
                //}
                //print($"Target: {targetDirAngle}, angleMovement: {angleMovement}, delta: {delta}, 1st valid: {fv}, 2nd valid: {angleMovement == Utils.ClampAngleLPositive(angleMovement, HorizontalConstraintMin, HorizontalConstraintMax)}");
                //if (angleMovement == Utils.ClampAngleLPositive(angleMovement, HorizontalConstraintMin, HorizontalConstraintMax))
                //{
                currentHorizontal = angleMovement;
                //}
                //currentHorizontal = Utils.ClampAngleLPositive(currentHorizontal, HorizontalConstraintMin, HorizontalConstraintMax); //TODO: handle angles
                SetRotation(HorizontalRotatePart, HorizontalRotationAxis, currentHorizontal);
            } else
            {
                CurrentHorizontalRotationSpeed = 0f;
            }
        }
    }
}
