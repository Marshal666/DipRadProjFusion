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
    public float HP { 
        get 
        {
            if(Offline)
            {
                return OfflineHP;
            } else
            {
                return _HP;
            }
        }
        set 
        { 
            if(Offline)
            {
                OfflineHP = value;
            } else
            {
                _HP = value;
            }
        }
    }

    public DamageableRoot _Root;
    public DamageableRoot Root { get => _Root; set => _Root = value; }

    public IHittable.HittableType Type => IHittable.HittableType.Damageable;

    public float _OfflineHP;

    public float OfflineHP { get => _OfflineHP; set => _OfflineHP = value; }

    bool _Offline = false;

    public bool Offline { get => _Offline; set => _Offline = value; }

    public override void Spawned()
    {
        HP = InitHP;
        OfflineHP = InitHP;
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

    public void Restore()
    {
        HP = InitHP;
        OfflineHP = InitHP;
    }
}
