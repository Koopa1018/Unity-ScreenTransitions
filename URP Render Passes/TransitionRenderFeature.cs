#if UNITY_URP
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Clouds.Transitions.URP {
    public class TransitionRenderFeature : ScriptableRendererFeature
    {
        [System.Flags] public enum TransitionTypes {
            None,
            Fade = 1 << 0, 
            OpaqueMesh = 1 << 1,
            TransparentMesh = 1 << 2,
        }

        [SerializeField] TransitionTypes transitionsSupported = ~TransitionTypes.None;

        // FadeTransitionRenderPass fadePass;
        MeshTransitionRenderPass opaquePass;
        // MeshTransitionRenderPass transparentPass;

        public override void Create()
        {
            opaquePass = new MeshTransitionRenderPass();
            opaquePass.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game) {
                renderer.EnqueuePass(opaquePass);
            }
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            RenderBufferStoreAction storeAction = RenderBufferStoreAction.Resolve;
            opaquePass.ConfigureColorStoreAction(storeAction);
            opaquePass.ConfigureDepthStoreAction(storeAction);
            opaquePass.ConfigureClear(ClearFlag.DepthStencil, Color.black);
        }
    }
}
#endif