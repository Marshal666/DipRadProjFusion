using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEffectObject : MonoBehaviour
{

    public Material OriginalMaterial;
    public Material GhostMaterial;
    public Material DestroyedMaterial;

    public Renderer[] Renderers;

    public enum MaterialType
    {
        Original,
        Ghost,
        Destroyed
    }

    public void SetMaterial(MaterialType type)
    {
        Material mat = OriginalMaterial;
        switch (type)
        {
            case MaterialType.Original:
                break;
            case MaterialType.Ghost:
                mat = GhostMaterial;
                break;
            case MaterialType.Destroyed:
                mat = DestroyedMaterial;
                break;
            default:
                throw new System.ArgumentException("MaterialType");
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
