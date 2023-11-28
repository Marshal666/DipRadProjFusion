using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnerArea : MonoBehaviour, IHittable
{

    public IHittable.HittableType Type => IHittable.HittableType.InnerArea;

}
