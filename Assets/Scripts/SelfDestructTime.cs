using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructTime : MonoBehaviour
{

    public float LifeTime = 5f;

    private void OnEnable()
    {
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(LifeTime);
        Destroy(gameObject);
    }

}
