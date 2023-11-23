using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablerRendererAdditional : Enabler
{
    public bool AdditionalRequirement = true;
    
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void SetEnabled(bool val)
    {
        if (_renderer)
        {
            _renderer.enabled = AdditionalRequirement & val;
        }
    }
}
