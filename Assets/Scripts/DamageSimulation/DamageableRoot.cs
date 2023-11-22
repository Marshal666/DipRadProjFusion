using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{

    public void TakeDamage(float damage);

    public float HP { get; set; }

    public DamageableRoot Root { get; set; }

    public void ApplyDoneDamage();

}

public class DamageableRoot : MonoBehaviour
{

    public IDamageable[] Items;

    private void Awake()
    {
        Items = GetComponentsInChildren<IDamageable>();
        for(int i = 0; i < Items.Length; i++)
        {
            Items[i].Root = this;
        }
    }

    public float[] GetState()
    {
        float[] ret = null;
        if(Items == null)
            return ret;

        ret = new float[Items.Length];

        for(int i = 0; i < ret.Length; i++)
        {
            ret[i] = Items[i].HP;
        }

        return ret;
    }

    public void SetState(float[] state)
    {
        if(Items == null) return;
        if(Items.Length != state.Length)
        {
            throw new System.ArgumentException("state mismatch");
        }
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].HP = state[i];
        }
    }

}
