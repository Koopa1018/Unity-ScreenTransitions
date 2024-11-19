#if UNITY_URP
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Clouds.Transitions.URP {
    public class MeshTransitionRenderPass : ScriptableRenderPass
    {
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Skip for editor cameras, reflection cameras, etc.
            if (renderingData.cameraData.cameraType != CameraType.Game) {
                return;
            }
    #if UNITY_EDITOR
            // Skip if viewing a game camera outside playmode.
            // Otherwise, problems crop up where a TransitionMaster instance
            // gets created, but can't be DontDestroyOnLoad'd...
            if (!UnityEditor.EditorApplication.isPlaying) {
                return;
            }
    #endif
            
            // Fetch transition master.
            TransitionMaster tm = TransitionMaster.Instance;
            //If none exists, abort.
            if (tm == null) {
                return;
            }

            // Fetch the current transition as well. If none exists, likewise abort.
            Transition transition = tm.currentTransition;
            if (transition == null) {
                return;
            }

            float progress = tm.transitionProgress;
            Vector2 vector = tm.transitionVector;
            bool isFadingOut = tm.transitionIsFadingOut;
            
            Camera camera = renderingData.cameraData.camera;

            var transitionDisplay = transition.CreateMesh(progress, isFadingOut, vector, ref camera, ref tm);

            // Put the mesh right in front of the camera's near clip plane.
            Vector3 meshPos = camera.transform.position + camera.transform.forward * (camera.nearClipPlane + float.Epsilon);
            // Create it a transform matrix.
            Matrix4x4 posMatrix = Matrix4x4.identity;
            posMatrix.SetColumn(3, new Vector4(meshPos.x, meshPos.y, meshPos.z, 1));

            // Draw the transition mesh.
            CommandBuffer cmd = CommandBufferPool.Get(name: "Screen Transitions Opaque");
            cmd.DrawMesh(
                transitionDisplay.mesh,
                posMatrix,
                transitionDisplay.material
            );
            context.ExecuteCommandBuffer(cmd);
            // Clean up the used command.
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
#endif