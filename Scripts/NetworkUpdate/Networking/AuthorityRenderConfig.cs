using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class AuthorityRenderConfig : NetworkBehaviour
{
    [System.Serializable]
    public struct RendererConfig
    {
        //public ShadowCastingMode shadowCastingMode;
        public bool shadowsOnly;
        public bool updateOffscreen;
    }
    public RendererConfig ownerConfig, remoteConfig;
    public MeshRenderer meshRenderer;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SetRendererConfig(IsOwner ? ownerConfig : remoteConfig);
    }
    void SetRendererConfig(RendererConfig config)
    {
        if (skinnedMeshRenderer)
        {
            skinnedMeshRenderer.shadowCastingMode = config.shadowsOnly ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            skinnedMeshRenderer.updateWhenOffscreen = config.updateOffscreen;
        }
        if (meshRenderer)
        {
            meshRenderer.shadowCastingMode = config.shadowsOnly ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
        }
    }
}
