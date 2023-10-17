using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestScr : MonoBehaviour
{

    public float HMin = 270;
    public float HMax = 90;

    public TankTurret.VerticalConstraint[] Constraints;

    public float[] ValsTest;

    public bool Run = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if(Run)
        {
            //if (ValsTest == null)
            //    return;
            //foreach (var val in ValsTest)
            //{
            //    print($"val: {val}, clamp: {Utils.ClampAngleLPositive(val, HMin, HMax)}");
            //}
            //Run = false;
        }
    }

}
