using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageModel : MonoBehaviour
{

    [Serializable]
    public struct DamageModelPart
    {
        public int HitboxId;
        public Transform Target;
    }

    public DamageModelPart[] Parts;

    public void SetTargets((Quaternion Rotation, int HitboxId)[] LagCompensatedPositions)
    {
        if (Parts == null)
            return;

        if (LagCompensatedPositions == null)
            return;

        if (Parts.Length != LagCompensatedPositions.Length) 
            return;

        for(int i = 0; i < LagCompensatedPositions.Length; i++)
        {
            Parts[i].Target.rotation = LagCompensatedPositions[i].Rotation;
            //for(int j = 0; j < Parts.Length; j++)
            //{
            //    if (Parts[j].HitboxId == LagCompensatedPositions[i].HitboxId)
            //    {
            //        Parts[j].Target.rotation = LagCompensatedPositions[i].Rotation;
            //        break;
            //    }
            //}
        }
    }



}
