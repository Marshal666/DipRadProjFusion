using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public static class DoubleRaycasting
{

    public static RaycastHit[] DoubleRaycastAll(Vector3 origin, Vector3 direction, float distance = Mathf.Infinity, int layerMask = int.MaxValue,
        QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
    {
        Vector3 start2 = origin + direction.normalized * distance;
        RaycastHit[] forwardHits = Physics.RaycastAll(origin, direction, distance, layerMask, query);
        RaycastHit[] backwardHits = Physics.RaycastAll(start2, -direction, distance, layerMask, query);
        
        Array.Sort(forwardHits, (RaycastHit a, RaycastHit b) =>
            Mathf.CeilToInt(a.distance - b.distance));
        
        //Debug.Log($"fwdh: {forwardHits.ToStr(", ")}");
        
        Array.Sort(backwardHits, (RaycastHit a, RaycastHit b) =>
            Mathf.CeilToInt((distance - a.distance) - (distance - b.distance)));
        
        //Debug.Log($"bwdh: {backwardHits.ToStr(", ")}");
        
        RaycastHit[] ret = new RaycastHit[forwardHits.Length + backwardHits.Length];

        int fi = 0;
        int bi = 0;

        for (int i = 0; i < ret.Length; i++)
        {
            if (fi < forwardHits.Length)
            {
                if (bi < backwardHits.Length)
                {
                    if (forwardHits[fi].distance < (distance - backwardHits[bi].distance))
                    {
                        ret[i] = forwardHits[fi++];
                    }
                    else
                    {
                        backwardHits[bi].distance = distance - backwardHits[bi].distance;
                        ret[i] = backwardHits[bi++];
                    }
                }
                else
                {
                    ret[i] = forwardHits[fi++];
                }
            }
            else
            {
                backwardHits[bi].distance = distance - backwardHits[bi].distance;
                ret[i] = backwardHits[bi++];
            }
        }
        
        //Debug.Log(ret.ToStr(", "));
        
        return ret;
    }
    
    static string ToStr(this RaycastHit[] hit, string separator)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        for (int i = 0; i < hit.Length; i++)
        {
            sb.Append($"hit(dist={hit[i].distance})");
            if (i + 1 < hit.Length)
                sb.Append(separator);
        }
        sb.Append("}");
        return sb.ToString();
    }
    
}
