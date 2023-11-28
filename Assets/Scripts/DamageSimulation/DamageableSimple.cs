using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageableSimple : MonoBehaviour, IDamageable
{

    public float _HP = 100f;
    public float HP { get => _HP; set => _HP = value; }

    public DamageableRoot _Root;
    public DamageableRoot Root { get => _Root; set => _Root = value; }

    public IHittable.HittableType Type => IHittable.HittableType.Damageable;
    
    public void TakeDamage(float damage)
    {
        HP -= damage;
    }

    public UnityEvent<IDamageable> ApplyDamage;

    public void ApplyDoneDamage()
    {
        ApplyDamage?.Invoke(this);
    }
}
