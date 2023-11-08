using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class LinePath : MonoBehaviour
{

    [SerializeField]
    Vector3[] _BasePoints;

    Vector3[] Points;

    float[] _Distances;

    float _Length;

    public IReadOnlyList<Vector3> BasePoints;

    public IReadOnlyList<float> Distances;

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

        for (int i = 1; i < _Distances.Length; i++)
        {
            _Distances[i - 1] = Vector3.Distance(_BasePoints[i], _BasePoints[i - 1]);
            _Length += _Distances[i - 1];
        }
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IncrementIndex(int inx)
    {
        return ++inx % _BasePoints.Length;
    }

    int DecrementIndex(int inx)
    {
        inx--;
        if ((inx %= _BasePoints.Length) < 0)
            inx += _BasePoints.Length;
        return inx;
    }

    public (int point, float dist) GetPointDistance(float distance)
    {
        distance %= Length;
        if(distance < 0)
            distance += Length;
        int inx = 0;
        float dp = 0f;
        while(dp < distance)
        {
            inx = IncrementIndex(inx);
            dp += _Distances[inx];
        }
        return (inx + 1, dp - distance);
    }

    public (int point, float dist) MarchDeltaDistance(int inx, float deltaDist)
    {
        while(deltaDist > 0f)
        {
            inx = IncrementIndex(inx);
            deltaDist -= _Distances[inx];
        }
        return (inx, deltaDist + _Distances[inx]);
    }

    public void UpdateBasePoint(int inx, Vector3 val)
    {
        _BasePoints[inx] = val;
    }

    private void Awake()
    {
        if (_BasePoints != null && _BasePoints.Length > 0)
        {
            Points = new Vector3[_BasePoints.Length];
            BasePoints = _BasePoints;

            _Distances = new float[_BasePoints.Length - 1];
            Distances = _Distances;
            RefreshPath();
        }
    }

    private void Start()
    {
        RefreshPath();
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
