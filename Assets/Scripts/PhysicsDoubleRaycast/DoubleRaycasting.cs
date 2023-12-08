using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class DoubleRaycasting
{

    public static RaycastHit[] DoubleRaycastAll(Vector3 origin, Vector3 direction, float distance = Mathf.Infinity, 
        int layerMask = int.MaxValue,
        QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
    {
        Vector3 start2 = origin + direction.normalized * distance;
        RaycastHit[] forwardHits = Physics.RaycastAll(origin, direction, distance, layerMask, query);
        RaycastHit[] backwardHits = Physics.RaycastAll(start2, -direction, distance, layerMask, query);

        for (int i = 0; i < backwardHits.Length; i++)
        {
            backwardHits[i].distance = distance - backwardHits[i].distance;
        }
        
        RaycastHit[] ret = new RaycastHit[forwardHits.Length + backwardHits.Length];

        forwardHits.CopyTo(ret, 0);
        backwardHits.CopyTo(ret, forwardHits.Length);
        
        Array.Sort(ret, (RaycastHit a, RaycastHit b) => 
            Mathf.CeilToInt((a.distance - b.distance) * 100000f));
        
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
