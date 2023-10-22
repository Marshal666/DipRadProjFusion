using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{

    public const byte FORWARD_BUTTON = 1;
    public const byte BACK_BUTTON = 2;
    public const byte LEFT_BUTTON = 4;
    public const byte RIGHT_BUTTON = 8;
    public const byte FIRE_BUTTON = 16;
    public const byte SECONDARY_FIRE_BUTTON = 32;

    public byte ArrowsInput;

    public float MX;
    public float MY;

    public bool ForwardPressed => (ArrowsInput & FORWARD_BUTTON) == FORWARD_BUTTON;
    public bool BackPressed => (ArrowsInput & BACK_BUTTON) == BACK_BUTTON;
    public bool LeftPressed => (ArrowsInput & LEFT_BUTTON) == LEFT_BUTTON;
    public bool RightPressed => (ArrowsInput & RIGHT_BUTTON) == RIGHT_BUTTON;
    public bool FirePressed => (ArrowsInput & FIRE_BUTTON) == FIRE_BUTTON;
    public bool SecondaryFirePressed => (ArrowsInput & SECONDARY_FIRE_BUTTON) == SECONDARY_FIRE_BUTTON;

    public void SetButton(byte button) => ArrowsInput |= button;

}