using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittable
{
    
    public enum HittableType
    {
        Armour,
        Damageable,
        InnerArea,
    }
    public HittableType Type { get; }
    
}
