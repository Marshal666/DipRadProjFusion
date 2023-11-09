using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class LinePathTransform : MonoBehaviour
{

    public LinePath Path;

    public float Distance = 0f;

    [Min(0)]
    public int CurrentPoint = 1;
    public float CurrentPointDistance = 0f;

    public bool LookDirection = true;

    //public bool m = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reposition()
    {
        transform.position = Path.GetPosition(CurrentPoint, CurrentPointDistance);
        if(LookDirection)
        {
            transform.rotation = Quaternion.LookRotation(Path.PointDirections[CurrentPoint]);
        }
    }

    public void SetPositionByDistance(float dist)
    {
        Distance = dist;
        (CurrentPoint, CurrentPointDistance) = Path.GetPointDistance(Distance);
        Reposition();
    }

    public void MarchDistance(float deltaDist)
    {
        (CurrentPoint, CurrentPointDistance) = Path.MarchDeltaDistance(CurrentPoint, CurrentPointDistance, deltaDist, out Distance);
        Reposition();
    }

    private void Start()
    {
        (CurrentPoint, CurrentPointDistance) = Path.GetPointDistance(Distance);
    }

    // Update is called once per frame
    void Update()
    {

        //if (m)
        //{
        //    MarchDistance(-1f);
        //    m = false;
        //}

        if (Input.GetKey(KeyCode.G))
        {
            MarchDistance(Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.H))
        {
            MarchDistance(-Time.deltaTime);
        }

        if (Path && Path.Count > 1)
        {
            //(CurrentPoint, CurrentPointDistance) = Path.MarchDeltaDistance(CurrentPoint, CurrentPointDistance, Time.deltaTime, out Distance);
            Reposition();
            //SetPositionByDistance(Distance);
        }
    }
}
