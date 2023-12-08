using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable : IHittable
{

    public void TakeDamage(float damage);

    public float HP { get; set; }

    public DamageableRoot Root { get; set; }

    public void ApplyDoneDamage();

}

public class DamageableRoot : NetworkBehaviour
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

    //bool d = true;
    //public override void FixedUpdateNetwork()
    //{
    //    if(d)
    //    {
    //        print($"Items count: {Items.Length}, hash: {GetHash()}, sum: {HP_Sum()}");
    //        d = false;
    //    }
    //}

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

    public int GetHash()
    {
        int ret = 0;
        if (Items == null)
        {
            return ret;
        }
        int mul = 1;
        for(int i = 0; i < Items.Length; i++)
        {
            ret += Items[i].HP.GetHashCode() * mul;
            mul += 1024;
        }
        return ret;
    }

    public float HP_Sum()
    {
        float ret = 0;
        if (Items == null)
        {
            return ret;
        }
        for (int i = 0; i < Items.Length; i++)
        {
            ret += Items[i].HP;
        }
        return ret;
    }

}
