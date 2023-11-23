using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablerGroup : Enabler
{

    public Enabler[] Enablers;
    
    public override void SetEnabled(bool val)
    {
        if (Enablers != null)
        {
            for (int i = 0; i < Enablers.Length; i++)
            {
                Enablers[i].SetEnabled(val);
            }
        }
    }
}
