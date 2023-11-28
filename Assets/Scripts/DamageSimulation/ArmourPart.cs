using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourPart : MonoBehaviour, IHittable
{
    public IHittable.HittableType Type => IHittable.HittableType.Armour;
}
