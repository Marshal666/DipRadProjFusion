using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject SniperModeObjects;

    public GameObject AimingObjects;


    public RectTransform AimingWhiteCircle;

    public RectTransform GuideanceWhiteCircle;

    static UIManager instance;
    public static UIManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public void SetSniperModeUIObjectsEnabled(bool val)
    {
        SniperModeObjects.SetActive(val);
    }

    public static void SetSniperModeUIEnabled(bool val)
    {
        instance.SetSniperModeUIObjectsEnabled(val);
    }

    public static void SetAimingObjectsActive(bool val)
    {
        instance.AimingObjects.SetActive(val);
    }

    public static void PositionAimingCircle(Vector3 newPos)
    {
        instance.AimingWhiteCircle.position = newPos;
    }

    public static void ScaleAimingCircle(Vector3 newScale)
    {
        instance.AimingWhiteCircle.localScale = newScale;
    }

    public static void PositionGuideanceCircle(Vector3 newPos)
    {
        instance.GuideanceWhiteCircle.position = newPos;
    }

    public static void SetAimingCircleEnabled(bool val)
    {
        instance.AimingWhiteCircle.gameObject.SetActive(val);
    }

    public static Vector3 GetAimingCirclePosition() => instance.AimingWhiteCircle.position;

    public static Vector3 GetGuideanceCirclePositoin() => instance.GuideanceWhiteCircle.position;

}
