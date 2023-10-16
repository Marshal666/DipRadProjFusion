using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerTankController : NetworkBehaviour
{

    private NetworkRigidbody rig;

    private NetworkObject nobj;

    /// <summary>
    /// Even indexes - left wheels,
    /// Odd indexes - right wheels
    /// </summary>
    [Tooltip("Even indexes - left wheels, Odd indexes - right wheels")]
    public WheelCollider[] Wheels;

    [Tooltip("Even indexes - left wheels, Odd indexes - right wheels")]
    public Transform[] ExtraWheels;

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

    private void Awake()
    {
        rig = GetComponent<NetworkRigidbody>();
        nobj = GetComponent<NetworkObject>();
    }

    public override void Spawned()
    {
        if (Runner && Runner.LocalPlayer == nobj.InputAuthority)
        {
            PlayerCamera.Instance.Target = transform;
        }
    }

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

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
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

        }
    }

}
