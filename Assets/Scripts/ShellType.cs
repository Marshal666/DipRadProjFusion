using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Shells/CreateNewShell", order = 1)]
public class ShellType : ScriptableObject
{
    
    public enum TrajectoryType
    {
        Laser,
        Parabolic
    }

    public TrajectoryType Trajectory;

    public float Caliber = 120f;

    public float InitialVelocity = 1000f;

    public string Name = "ShellName_";

    public Vector3? HitTest(Vector3 start, Vector3 direction)
    {
        Vector3? ret = null;
        switch (Trajectory)
        {
            case TrajectoryType.Laser:
                RaycastHit hit;
                if (Physics.Raycast(start, direction, out hit, StaticConsts.MaxShellRaycastLength, StaticConsts.ShellHitLayers))
                {
                    return hit.point;
                }
                break;
            case TrajectoryType.Parabolic:

                break;
            default:
                break;
        }
        
        return ret;
    }

}
