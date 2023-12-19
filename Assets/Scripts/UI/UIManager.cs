using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject Canvas;

    public GameObject SniperModeObjects;

    public GameObject AimingObjects;

    public GameObject DoneDmgMsgItemsHolder;

    public GameObject ReceivedDmgMsgItemsHolder;

    public RectTransform AimingWhiteCircle;

    public RectTransform GuideanceWhiteCircle;

    public GameObject TextMsgItem;

    static UIManager instance;

    public TankHealthUI healthUI;

    public Slider ReloadSlider;

    public GameObject YouDiedText;

    public Slider SuicideSlider;

    public GameObject DebugTankTextPrefab;

    public Text PingStateText;

    public InputField GameRoomNameInput;

    public GameObject GameRoomPanelObject;

    public Toggle ConstantForwardToggle;

    public Text NetworkStateText;

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

    public static void SetHealthItemState(PlayerTankController.TankHealthBits bit, bool alive)
    {
        Instance.healthUI.SetItemState(bit, alive);
    }

    public static void SetHealthItemStates(PlayerTankController.TankHealthBits[] bits, bool alive)
    {
        if (bits == null)
            return;
        foreach (var bit in bits)
        {
            Instance.healthUI.SetItemState(bit, alive);
        }
    }

    public static void SetHealthItemHP(PlayerTankController.TankHealthBits bit, float val)
    {
        Instance.healthUI.SetItemHP(bit, val);
    }

    public static void AddDoneDmgTextMsgItem(string text)
    {
        if (Instance.DoneDmgMsgItemsHolder.activeSelf)
        {
            var o = Instantiate(Instance.TextMsgItem, Instance.DoneDmgMsgItemsHolder.transform);
            o.GetComponent<Text>().text = text;
        }
    }

    public static void AddReceivedDmgTextMsgItem(string text)
    {
        if (Instance.ReceivedDmgMsgItemsHolder.activeSelf)
        {
            var o = Instantiate(Instance.TextMsgItem, Instance.ReceivedDmgMsgItemsHolder.transform);
            o.GetComponent<Text>().text = text;
        }
    }

    public static void SetReloadProgress(float progress)
    {
        Instance.ReloadSlider.value = progress;
    }

    public static void SetSuicideProgress(float progress)
    {
        Instance.SuicideSlider.value = progress;
    }

    public static void SetYouDiedTextEnabled(bool enabled)
    {
        Instance.YouDiedText.SetActive(enabled);
    }

    public static void SetPingStateText(string text)
    {
        Instance.PingStateText.text = text;
    }

    public static string GetGameRoomNameInput()
    {
        string ret = "room";

        if(Instance && Instance.GameRoomNameInput)
        {
            ret = Instance.GameRoomNameInput.text;
        }

        return ret;
    }

    public static void SetMPGameWindowActive(bool active)
    {
        Instance.GameRoomPanelObject.SetActive(active);
    }

    public static void SetNetworkStateText(string text)
    {
        Instance.NetworkStateText.text = text;
    }

    public static bool ConstantForward => Instance.ConstantForwardToggle.isOn;

    public static Vector3 GetAimingCirclePosition() => instance.AimingWhiteCircle.position;

    public static Vector3 GetGuideanceCirclePositoin() => instance.GuideanceWhiteCircle.position;

}
