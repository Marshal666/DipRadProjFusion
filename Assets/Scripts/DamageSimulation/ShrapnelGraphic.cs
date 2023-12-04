using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrapnelGraphic : MonoBehaviour
{
    public LineRenderer Line;
    public GameObject Graphic;

    public void Init(Vector3 pos, Vector3 dir, Quaternion rot, float lineLength)
    {
        transform.position = pos;
        transform.forward = dir;
        if(!rot.HasNaN())
            Graphic.transform.rotation = rot;
        Line.SetPosition(1, Vector3.back * lineLength);
    }

    public void SetPosition(Vector3 pos, float lineLength)
    {
        transform.position = pos;
        Line.SetPosition(1, Vector3.back * lineLength);
    }

}
