using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankWheel : MonoBehaviour
{

    public Transform TargetWheel;

    public Vector3 PositioningOffset = default;

    Vector3 initialRotation;
    WheelCollider wheelCollider;

    private void Awake()
    {
        wheelCollider = GetComponent<WheelCollider>();
        initialRotation = TargetWheel.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        TargetWheel.transform.position = pos + PositioningOffset;
        TargetWheel.transform.rotation = Quaternion.Euler(rot.eulerAngles + initialRotation);
    }
}
