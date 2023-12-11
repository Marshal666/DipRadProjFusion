using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisableAndReturn : MonoBehaviour, IHoldable
{

    public ObjectHolder Owner;

    public float LifeTime = 5f;

    public ObjectHolder Holder { get => Owner; set => Owner = value; }

    private void OnEnable()
    {
        StartCoroutine(Return());
    }

    IEnumerator Return()
    {
        yield return new WaitForSeconds(LifeTime);
        if(Holder)
        {
            Holder.ReturnObject(gameObject);
        }
    }
}
