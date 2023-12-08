using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageableSimple : NetworkBehaviour, IDamageable
{

    public float InitHP = 10f;

    [Networked]
    public float _HP { get; set; }
    public float HP { get => _HP; set => _HP = value; }

    public DamageableRoot _Root;
    public DamageableRoot Root { get => _Root; set => _Root = value; }

    public IHittable.HittableType Type => IHittable.HittableType.Damageable;

    public override void Spawned()
    {
        _HP = InitHP;
    }

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
