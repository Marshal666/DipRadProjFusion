using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObject : MonoBehaviour
{

    public GameObject TrackPart;
    public float PartLength;
    public LinePath TrackShape;
    public int AdditionalCount = 0;

    public bool FixSpacing = true;

    public int[] GroundPoints;
    public float GroundPointSpringLength = 0.3f;
    public float GroundPointSpringDownOffset = 0.1f;

    public float TrackGroundPointMoveSpeed = 10f;

    Vector3[] GroundPointsInitial;
    Vector3[] GroundPointTargets;

    [SerializeField]
    //[HideInInspector]
    LinePathTransform[] Parts;

    public void Init()
    {
        int Count = Mathf.CeilToInt(TrackShape.Length / PartLength) + AdditionalCount;

        if (Count <= 0)
            return;

        Parts = new LinePathTransform[Count];

        if(FixSpacing)
        {
            PartLength = TrackShape.Length / Count;
        }

        TrackShape.Reinit();

        for(int i = 0; i < Count; i++)
        {
            GameObject obj = Instantiate(TrackPart);
            obj.transform.SetParent(transform);

            LinePathTransform lpt = obj.GetComponent<LinePathTransform>();
            lpt.Path = TrackShape;
            lpt.Distance = i * PartLength;
            Parts[i] = lpt;
        }

        if (GroundPoints != null)
        {
            GroundPointsInitial = new Vector3[GroundPoints.Length];
            GroundPointTargets = new Vector3[GroundPoints.Length];
            for (int i = 0; i < GroundPointsInitial.Length; i++)
            {
                GroundPointsInitial[i] = TrackShape.BasePoints[GroundPoints[i]];
                GroundPointTargets[i] = TrackShape[GroundPoints[i]];
            }
        }
    }

    public void MarchDistance(float deltaDist)
    {
        if(Parts != null)
        {
            for(int i = 0; i < Parts.Length; i++)
            {
                Parts[i].MarchDistance(deltaDist);
            }
        }
    }

    public void MarchGirstOffsetOthers(float deltaDist)
    {
        if(Parts != null && Parts.Length > 0)
        {
            Parts[0].MarchDistance(deltaDist);
            float stDist = Parts[0].Distance;
            for(int i = 1; i < Parts.Length; i++)
            {
                Parts[i].SetPositionByDistance(stDist + i * PartLength);
            }
        }
    }

    public void Clear()
    {
        if(Parts != null)
        {
            for(int i = 0; i < Parts.Length; i++)
            {
                if(Parts[i] != null)
                    Utils.DestroyObject(Parts[i].gameObject);
            }
            Parts = null;
        }
    }

    private void Start()
    {
        Clear();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.G))
        //{
        //    MarchDistance(Time.deltaTime);
        //}
        //if (Input.GetKey(KeyCode.H))
        //{
        //    MarchDistance(-Time.deltaTime);
        //}

        if (GroundPoints != null)
        {
            for (int i = 0; i < GroundPoints.Length; i++)
            {
                Transform ts = TrackShape.transform;
                Vector3 origin = ts.TransformPoint(GroundPointsInitial[i]) + ts.up * GroundPointSpringLength;
                Vector3 dir = -ts.up;

                RaycastHit hit;
                if (Physics.Raycast(origin, dir, out hit, GroundPointSpringLength + GroundPointSpringDownOffset, StaticConsts.GroundLayers))
                {
                    GroundPointTargets[i] = hit.point;
                }
                else
                {
                    GroundPointTargets[i] = ts.TransformPoint(GroundPointsInitial[i]);
                }
            }

            for (int i = 0; i < GroundPoints.Length; i++)
            {
                Vector3 val = Vector3.MoveTowards(TrackShape[GroundPoints[i]], GroundPointTargets[i], TrackGroundPointMoveSpeed * Time.deltaTime);
                TrackShape.UpdateBasePointFromGlobal(GroundPoints[i], val);
            }
        }

    }

    private void FixedUpdate()
    {
        //if (GroundPoints != null)
        //{
        //    for (int i = 0; i < GroundPoints.Length; i++)
        //    {
        //        Transform ts = TrackShape.transform;
        //        Vector3 origin = ts.TransformPoint(GroundPointsInitial[i]) + ts.up * GroundPointSpringLength;
        //        Vector3 dir = -ts.up;

        //        RaycastHit hit;
        //        if (Physics.Raycast(origin, dir, out hit, GroundPointSpringLength + GroundPointSpringDownOffset, StaticConsts.GroundLayers))
        //        {
        //            GroundPointTargets[i] = hit.point;
        //        }
        //        else
        //        {
        //            GroundPointTargets[i] = ts.TransformPoint(GroundPointsInitial[i]);
        //        }
        //    }
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if (GroundPoints != null)
        {
            for (int i = 0; i < GroundPoints.Length; i++)
            {
                Gizmos.DrawLine(TrackShape[GroundPoints[i]] - Vector3.up * GroundPointSpringDownOffset, TrackShape[GroundPoints[i]] + Vector3.up * GroundPointSpringLength);
            }
        }
    }

}
