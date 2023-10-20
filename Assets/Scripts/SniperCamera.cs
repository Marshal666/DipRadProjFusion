using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperCamera : MonoBehaviour
{

    public float ZoomOutFocalLength = 40f;
    public float ZoomInFocalLength = 400f;

    public float TargetZoom = 40f;

    public float ZoomInSpeed = 800f;

    public ZoomType CurrentZoomType = ZoomType.Out;

    public Camera cam;

    public enum ZoomType
    {
        In = 0,
        Out = 1
    }

    public void Zoom(ZoomType type)
    {
        switch (type)
        {
            case ZoomType.In:
                break;
            case ZoomType.Out:
                break;
            default:
                throw new System.ArgumentException("type");
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        TargetZoom = ZoomOutFocalLength;
        cam.focalLength = ZoomOutFocalLength;
        CurrentZoomType = ZoomType.Out;
    }

    private void OnDisable()
    {
        TargetZoom = ZoomOutFocalLength;
        cam.focalLength = ZoomOutFocalLength;
    }

    public void ToggleZoom()
    {
        CurrentZoomType = 1 - CurrentZoomType;
        switch (CurrentZoomType)
        {
            case ZoomType.In:
                TargetZoom = ZoomInFocalLength;
                break;
            case ZoomType.Out:
                TargetZoom = ZoomOutFocalLength;
                break;
            default:
                throw new System.ArgumentException("CurrentZoomType");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            ToggleZoom();
        }
        if(cam.focalLength != TargetZoom)
        {
            cam.focalLength = Mathf.MoveTowards(cam.focalLength, TargetZoom, ZoomInSpeed * Time.deltaTime);
        }
    }
}
