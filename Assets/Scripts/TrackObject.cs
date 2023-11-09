using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObject : MonoBehaviour
{

    public GameObject TrackPart;
    public float PartLength;
    public LinePath TrackShape;

    public bool FixSpacing = true;

    [SerializeField]
    //[HideInInspector]
    LinePathTransform[] Parts;

    public void Init()
    {
        int Count = Mathf.CeilToInt(TrackShape.Length / PartLength);

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
    //void Update()
    //{
    //    if(Input.GetKey(KeyCode.G))
    //    {
    //        MarchDistance(Time.deltaTime);
    //    }
    //    if(Input.GetKey(KeyCode.H))
    //    {
    //        MarchDistance(-Time.deltaTime);
    //    }
    //}
}
