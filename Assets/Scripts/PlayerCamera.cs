using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    public Transform Target;

    public Vector3 TargetLookAtOffset = new Vector3(0, 3f, 5f);

    public Vector3 PositioningOffset = new Vector3(0, 0, 0);

    public float Distance = 7f;
    public float MinDistance = 5f;
    public float MaxDistance = 12f;

    public float mx = 0f;
    public float my = 0f;

    public float MinYRotation = 5f;
    public float MaxYRotation = 89f;

    public float RotationSpeed = 60f;
    public float ZoomSpeed = 20f;

    public float SmoothTimePosition = 0.25f;
    public float SmoothTimeRotation = 0.25f;

    static PlayerCamera instance;

    public static PlayerCamera Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (!Target)
            return;

        mx += Input.GetAxis("Mouse X");
        my = Mathf.Clamp(my - Input.GetAxis("Mouse Y"), MinYRotation, MaxYRotation);
        Distance = Mathf.Clamp(Distance - Input.mouseScrollDelta.y * Time.deltaTime * ZoomSpeed, MinDistance, MaxDistance);

        //positionTracks[piti] = transform.position;
        //piti = (piti + 1) % positionTracks.Length;
        
    }


    Vector3 velocity = default;
    Quaternion deriv = Quaternion.identity;
    // Update is called once per frame
    void LateUpdate()
    {

        if (!Target)
            return;

        Quaternion lookRotation = Quaternion.Euler(my, mx, 0);

        Vector3 toDir = ((Target.position + lookRotation * TargetLookAtOffset) - transform.position);

        Vector3 targetPos = lookRotation * (PositioningOffset + -Vector3.forward * Distance) + Target.position;
        
        Quaternion targetRot = Quaternion.LookRotation(toDir, Vector3.up);

        Quaternion smoothedRot = Utils.SmoothDampQuaternion(transform.rotation, targetRot, ref deriv, SmoothTimeRotation); ;
        transform.rotation = smoothedRot;

        //transform.position = targetPos;
        Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, SmoothTimePosition);
        transform.position = smoothedPos;

        //positionTracks[piti] = transform.position;
        //piti = (piti + 1) % positionTracks.Length;
    }

    //int piti = 0;
    //Vector3[] positionTracks = new Vector3[2048];
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.blue;
    //    for(int i = 1; i < positionTracks.Length; i++)
    //    {
    //        Gizmos.DrawLine(positionTracks[i - 1], positionTracks[i]);
    //    }
    //}
}
