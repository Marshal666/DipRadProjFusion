using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablerObject : Enabler
{

    public override void SetEnabled(bool val)
    {
        gameObject.SetActive(val);
    }
}
