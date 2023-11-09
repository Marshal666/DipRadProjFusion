using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablerRenderer : Enabler
{

    Renderer m_MeshRenderer;

    private void Awake()
    {
        m_MeshRenderer = GetComponent<Renderer>();
    }

    public override void SetEnabled(bool val)
    {
        m_MeshRenderer.enabled = val;
    }
}
