using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankHealthUI : MonoBehaviour
{

    [Serializable]
    public struct HealthItemUI
    {
        public PlayerTankController.TankHealthBits id;
        public Image image;
    }

    public HealthItemUI[] _Items;

    public Color HealthyColor = Color.white;
    public Color DeadColor = Color.red;

    Dictionary<PlayerTankController.TankHealthBits, Image> Items;

    private void Awake()
    {
        if (_Items == null)
            return;
        if (_Items.Length == 0)
            return;
        Items = new Dictionary<PlayerTankController.TankHealthBits, Image>();
        foreach(var item in _Items)
        {
            Items[item.id] = item.image;
        }
    }

    public void SetItemState(PlayerTankController.TankHealthBits bit, bool alive)
    {
        Color col = alive ? HealthyColor : DeadColor;
        Items[bit].color = col;
    }

}
