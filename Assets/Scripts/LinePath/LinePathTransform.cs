using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LinePathTransform : MonoBehaviour
{

    public LinePath Path;

    public float Distance = 0f;

    public int CurrentPoint = 1;
    public float CurrentPointDistance = 0f;

    private void Start()
    {
        (CurrentPoint, CurrentPointDistance) = Path.GetPointDistance(Distance);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            (CurrentPoint, CurrentPointDistance) = Path.MarchDeltaDistance(CurrentPoint, 0.5f);
        }

        if (Path && Path.Count > 1)
        {
            transform.position = Path[CurrentPoint - 1] + (Path[CurrentPoint] - Path[CurrentPoint - 1]) * CurrentPointDistance;
        }
    }
}
