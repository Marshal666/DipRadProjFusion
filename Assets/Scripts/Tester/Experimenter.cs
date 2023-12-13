using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experimenter : MonoBehaviour
{

    public Vector3 Origin = Vector3.zero;
    public Vector3 Direction = Vector3.forward;

    public float RaycastDist = 5f;

    public Transform Cube;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Vector3 o = transform.TransformPoint(Origin);
            Vector3 d = transform.TransformDirection(Direction);

            if(Cube)
            {
                Vector3 old = Cube.position;
                Cube.position = o + d.normalized;

                if(Physics.Raycast(o, d, RaycastDist))
                {
                    print("Hit!");
                } else
                {
                    print("No hit!");
                }

                Cube.position = old;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Vector3 o = transform.TransformPoint(Origin);
        Vector3 d = transform.TransformDirection(Direction);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(o, o + d.normalized * RaycastDist);
    }

}
