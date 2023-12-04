using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEffectObject : MonoBehaviour
{

    public Material OriginalMaterial;
    public Material GhostMaterial;

    public Renderer[] Renderers;

    public enum MaterialType
    {
        Original,
        Ghost
    }

    public void SetMaterial(MaterialType type)
    {
        Material mat = OriginalMaterial;
        if(type == MaterialType.Ghost)
        {
            mat = GhostMaterial;
        }
        if(Renderers != null)
        {
            for(int i = 0;i < Renderers.Length; i++)
            {
                Renderer ren = Renderers[i];
                ren.material = mat;
            }
        }
    }

}
