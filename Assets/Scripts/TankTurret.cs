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
        if (GetInput(out NetworkInputData data))
        {
            if (float.IsNaN(data.MX) || float.IsNaN(data.MY))
            {
                CurrentHorizontalRotationSpeed = 0f;
                CurrentVerticalRotationSpeed = 0f;
                return;
            }

            float currentHorizontal = Utils.NormalizeAngle360(AxisValueFromQuaternion(HorizontalRotationAxis, HorizontalRotatePart.localRotation));
            float currentVertical = Utils.NormalizeAngle360(AxisValueFromQuaternion(VerticalRotationAxis, VerticalRotatePart.transform.rotation) - VerticalRotationOffset);
            float currentv0 = currentVertical;

            //Vertical rotation part
            float my = Utils.NormalizeAngle360(VerticalPlacementOffset - data.MY);
            //TODO: map "my" variable so that it's properly aligned with view for shooting
            int constraintIndex = VerticalConstraintAngleIndex(currentHorizontal);
            float vmin = VerticalConstraints[constraintIndex].VerticalMin;
            float vmax = VerticalConstraints[constraintIndex].VerticalMax;
            my = Utils.NormalizeAngle360(Utils.ClampAngleLPositive(my, vmin, vmax));
            
            if (currentVertical != my)
            {
                //TODO: fix smooth gun lifting..
                CurrentVerticalRotationSpeed = Mathf.Clamp(CurrentVerticalRotationSpeed + VerticalRotationAcceleration * Runner.DeltaTime, 0, VerticalRotationSpeedMax);
                float angleMovement;
                angleMovement = Utils.NormalizeAngle360(Mathf.MoveTowardsAngle(currentVertical, my, CurrentVerticalRotationSpeed * Runner.DeltaTime));
                float delta = Utils.NormalizeAngle360(Mathf.DeltaAngle(angleMovement, currentVertical));
                //print($"my0: {Utils.NormalizeAngle360(VerticalPlacementOffset - data.MY)}, my: {my}, currentV: {currentVertical}, GunRot: {AxisValueFromQuaternion(VerticalRotationAxis, VerticalRotatePart.transform.rotation)}");
                //if (HasVerticalConstraints)
                //{
                    
                //    float nearEndStep = Utils.NormalizeAngle360(Mathf.LerpAngle(currentVertical, my, 0.99f));
                //    if (nearEndStep != Utils.ClampAngleLPositive(nearEndStep, vmin, vmax))
                //    {
                //        angleMovement = Utils.NormalizeAngle360(angleMovement - 2 * delta);
                //    }
                //}
                //if (!HasVerticalConstraints || angleMovement == Utils.ClampAngleLPositive(angleMovement, vmin, vmax))
                //{
                    currentVertical = angleMovement;
                //}
                //currentVertical = my; //works fine
                print($"Target: {my}, current: {currentVertical}, current0: {currentv0}, am: {angleMovement}, delta: {delta}, cv: {CurrentVerticalRotationSpeed}, acRot: {Utils.NormalizeAngle360(AxisValueFromQuaternion(VerticalRotationAxis, VerticalRotatePart.transform.rotation) - VerticalRotationOffset)}");
                SetRotation(VerticalRotatePart, VerticalRotationAxis, -currentVertical + VerticalRotationOffset);
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
            } else
            {
                CurrentHorizontalRotationSpeed = 0f;
            }
        }
    }
}
