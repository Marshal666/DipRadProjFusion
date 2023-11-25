using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tester : MonoBehaviour
{

    public Vector3 StartPoint = default;

    public Vector3 EndPoint = new Vector3(0, 0, 3f);
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector3 start = transform.TransformPoint(StartPoint);
        Vector3 end = transform.TransformPoint(EndPoint);
        
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(start, end);

        float dist = Vector3.Distance(start, end);

        var hits = DoubleRaycasting.DoubleRaycastAll(start, end - start, dist, Int32.MaxValue, QueryTriggerInteraction.Ignore);
        
        //var hits = Physics.RaycastAll(start, end - start, dist);

        if (hits != null)
        {
            Gizmos.color = Color.magenta;
            int i = 0;
            foreach (var hit in hits)
            {
                Gizmos.DrawSphere(hit.point, 0.1f);
                #if UNITY_EDITOR
                Handles.Label(hit.point + Vector3.up * 0.3f, (i++).ToString());
                #endif
            }
        }
    }
}
