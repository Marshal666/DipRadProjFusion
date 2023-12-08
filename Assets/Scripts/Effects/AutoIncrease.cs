using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoIncrease : MonoBehaviour
{

    public Vector3 StartScale = new Vector3(0.1f, 0.1f, 0.1f);

    public Vector3 EndScale = new Vector3(1f, 1f, 1f);

    public float ScaleSpeed = 10f;

    private void OnEnable()
    {
        transform.localScale = StartScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, EndScale, ScaleSpeed * Time.deltaTime);
    }

}
