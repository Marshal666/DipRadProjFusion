using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructTime : MonoBehaviour
{

    public float LifeTime = 5f;

    public IDamageable Damageable;

    public float HPCheckStartTime = 0.5f;

    float hpct = 0f;

    void EndThis()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        if(Damageable != null && Damageable.HP > 0)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(LifeTime);
        Destroy(gameObject);
    }

    private void Start()
    {
        if (Damageable != null && Damageable.HP > 0)
        {
            EndThis();
        }
    }

    private void Update()
    {
        hpct += Time.deltaTime;
        if (hpct >= HPCheckStartTime && Damageable != null && Damageable.HP > 0f)
        {
            //print($"HP > 0 for {Damageable}, HP = {Damageable.HP}");
            EndThis();
        }
    }

}
