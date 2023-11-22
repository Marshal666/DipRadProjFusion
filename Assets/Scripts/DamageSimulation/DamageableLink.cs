using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableLink : MonoBehaviour
{

    public IDamageable Damageable;

    private void Awake()
    {
        var d = GetComponentInParent<IDamageable>();
        if (d == null)
            throw new System.Exception("Damageable link has no parent with Damageable component");
        Damageable = d;
    }

}
