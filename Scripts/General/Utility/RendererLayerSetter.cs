using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererLayerSetter : MonoBehaviour
{
    public Renderer[] renderers;
    public string layerName = "Character";
    [SerializeField] protected int layerIndex;
    private void Start()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].gameObject.layer = layerIndex;
        }
    }
    private void OnValidate()
    {
        layerIndex = LayerMask.NameToLayer(layerName);
    }
}
