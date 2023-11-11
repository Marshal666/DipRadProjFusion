using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
[System.Serializable]
public class LinePath : MonoBehaviour
{

    [SerializeField]
    Vector3[] _BasePoints;

    [DoNotSerialize]
    Vector3[] Points;

    [DoNotSerialize]
    Vector3[] _PointDirections;

    [DoNotSerialize]
    float[] _Distances;

    [DoNotSerialize]
    float[] _SumDistances;

    [DoNotSerialize]
    float _Length;

    [DoNotSerialize]
    public IReadOnlyList<Vector3> BasePoints;

    [DoNotSerialize]
    public IReadOnlyList<float> Distances;

    [DoNotSerialize]
    public IReadOnlyList<float> SumDistances;

    [DoNotSerialize]
    public IReadOnlyList<Vector3> PointDirections;

    public float Length => _Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RefreshPath()
    {

#if UNITY_EDITOR
        if(Points == null || Points.Length != _BasePoints.Length)
            Points = new Vector3[_BasePoints.Length];
#endif
        transform.TransformPoints(_BasePoints, Points);

        _Length = 0f;
        _Distances[0] = 0f;
        _SumDistances[0] = 0f;

        for (int i = 1; i < _Distances.Length; i++)
        {
            _Distances[i] = Vector3.Distance(_BasePoints[i], _BasePoints[i - 1]);
            _Length += _Distances[i];
            _SumDistances[i] = _SumDistances[i - 1] + _Distances[i];
        }

        for(int i = 0; i < _PointDirections.Length; i++)
        {
            _PointDirections[i] = (this[IncrementIndex(i, _BasePoints.Length)] - this[i]).normalized;
        }

        //print($"length: {_Length} {Length}");
        
    }

    public void Reinit()
    {
        if (_BasePoints != null && _BasePoints.Length > 0)
        {
            Points = new Vector3[_BasePoints.Length];
            BasePoints = _BasePoints;

            _PointDirections = new Vector3[_BasePoints.Length];
            PointDirections = _PointDirections;

            _Distances = new float[_BasePoints.Length];
            _SumDistances = new float[_BasePoints.Length];
            Distances = _Distances;
            SumDistances = _SumDistances;
            RefreshPath();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IncrementIndex(int inx, int max)
    {
        return ++inx % max;
    }

    int DecrementIndex(int inx, int max)
    {
        inx--;
        if ((inx %= max) < 0)
            inx += max;
        return inx;
    }

    public float NormalizeDistance(float distance)
    {
        distance %= _Length;
        if (distance < 0)
            distance += _Length;
        return distance;
    }

    public (int point, float dist) GetPointFromDistance(float distance)
    {
        //normalize distance
        distance = NormalizeDistance(distance);
        //print($"distance: {distance}");

        ///////// O(n) solution
        //int inx = -1;
        //float ds = 0f;
        //while(distance > ds || inx < 0)
        //{
        //    inx = IncrementIndex(inx, _Distances.Length);
        //    ds += _Distances[IncrementIndex(inx, _Distances.Length)];
        //}
        //return (inx, distance - ds + _Distances[IncrementIndex(inx, _Distances.Length)]);
        /////////

        ///////// O(log(n)) solution
        //int inx = Array.BinarySearch(_SumDistances, distance);
        int inx = -1;
        int lo = 0;
        int hi = _SumDistances.Length - 1;
        while (lo <= hi)
        {
            int i = lo + ((hi - lo) >> 1);
            float comp = _SumDistances[i] - distance;
            if(comp == 0f)
            {
                inx = i;
                break;
            } else if(comp < 0f)
            {
                lo = i + 1;
            } else
            {
                hi = i - 1;
            }
        }
        if(inx < 0)
        {
            inx = ~lo;
        }
        if (inx < 0)
        {
            inx = ~inx - 1;
        }
        if (inx == -1)
        {
            inx = 0;
        }
        //print($"inx: {inx}");

        return (inx, distance - _SumDistances[inx]);
        /////////
    }

    public (int point, float dist) MarchDeltaDistance(int inx, float cdist, float deltaDist, out float dist)
    {
        cdist += deltaDist;
        if (deltaDist >= 0f)
        {
            while (cdist > _Distances[IncrementIndex(inx, _BasePoints.Length)])
            {
                inx = IncrementIndex(inx, _Distances.Length);
                cdist -= _Distances[inx];
            }
        } else
        {
            while(cdist < 0f)
            {
                cdist += _Distances[inx];
                inx = DecrementIndex(inx, _BasePoints.Length);
            }
        }
        dist = _SumDistances[inx] + cdist;
        return (inx, cdist);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 GetPosition(int inx, float dist)
    {
        return this[inx] + _PointDirections[inx] * dist;
    }

    public void UpdateBasePoint(int inx, Vector3 val)
    {
        _BasePoints[inx] = val;
    }

    public void UpdateBasePointFromGlobal(int inx, Vector3 val)
    {
        _BasePoints[inx] = transform.InverseTransformPoint(val);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void ParseJSON(string text)
    {
        JsonUtility.FromJsonOverwrite(text, this);
        Reinit();
    }

    private void Awake()
    {
        Reinit();
    }

    private void OnEnable()
    {
        Reinit();
    }

    private void Start()
    {
        Reinit();
    }

    private void Update()
    {
        RefreshPath();
    }

    public int Count => _BasePoints.Length;

    public Vector3 this[int i] => Points[i];

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Matrix4x4 tm = transform.localToWorldMatrix;
        if(_BasePoints != null && _BasePoints.Length > 1)
        {
            Gizmos.DrawSphere(this[0], 0.05f);
            for(int i = 1; i < _BasePoints.Length; i++)
            {
                Gizmos.DrawLine(this[i - 1], this[i]);
                Gizmos.DrawSphere(this[i], 0.05f);
            }
        }
    }
}
