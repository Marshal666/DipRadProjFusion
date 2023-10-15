using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{

    public const byte FORWARD_BUTTON = 1;
    public const byte BACK_BUTTON = 2;
    public const byte LEFT_BUTTON = 4;
    public const byte RIGHT_BUTTON = 8;

    public byte ArrowsInput;

    public bool ForwardPressed => (ArrowsInput & FORWARD_BUTTON) == FORWARD_BUTTON;
    public bool BackPressed => (ArrowsInput & BACK_BUTTON) == BACK_BUTTON;
    public bool LeftPressed => (ArrowsInput & LEFT_BUTTON) == LEFT_BUTTON;
    public bool RightPressed => (ArrowsInput & RIGHT_BUTTON) == RIGHT_BUTTON;

    public void SetButton(byte button) => ArrowsInput |= button;

}