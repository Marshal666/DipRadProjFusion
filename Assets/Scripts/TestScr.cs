using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class TestScr : MonoBehaviour
{

    // public float HMin = 270;
    // public float HMax = 90;
    //
    // public TankTurret.VerticalConstraint[] Constraints;
    //
    // public float[] ValsTest;
    //
    // public bool Run = false;
    //
    // // Start is called before the first frame update
    // void Start()
    // {
    //     //Application.targetFrameRate = 60;
    // }
    //
    // public float current = 350f;
    // public float target = 20f;
    //
    // private void Update()
    // {
    //     if(Run)
    //     {
    //         //if (ValsTest == null)
    //         //    return;
    //         //foreach (var val in ValsTest)
    //         //{
    //         //    print($"val: {val}, clamp: {Utils.ClampAngleLPositive(val, HMin, HMax)}");
    //         //}
    //
    //         current = Utils.NormalizeAngle360(Mathf.MoveTowardsAngle(current, target, 20f));
    //         print(current);
    //
    //         Run = false;
    //     }
    // }

    public Vector3 v1 = Vector3.forward;
    public Vector3 v2 = new Vector3(0, 1, 1);

    private void Update()
    {
        print(Vector3.Angle(v1, v2));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, v1);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, v2);
    }
}
