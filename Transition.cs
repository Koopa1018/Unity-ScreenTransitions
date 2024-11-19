using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Transitions {
	public struct TransitionRenderState {
		public Material material;
		public Mesh mesh;
    }

	public abstract class Transition : ScriptableObject {
		/// <summary>
		/// The standard normal vector for 2D screen transitions.
		/// </summary>
		protected static readonly Vector3 NORMAL = Vector3.forward;

		protected static Vector3[] verts, norms;
		protected static int[] tris;

		[SerializeField] Material transitionMaterial;
		public Material material {get => transitionMaterial;}
		
		[UnityEngine.Serialization.FormerlySerializedAs("duration")]
		[SerializeField] float _duration = 1.0f;
		protected internal float duration {get => _duration;}


		public abstract TransitionRenderState CreateMesh (float progress, bool fadingOut, Vector3 vector, ref Camera currentCamera, ref TransitionMaster master);

		/// <summary>
		/// Gets the distance from a point to the furthest-away corner of a camera.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="camera">The camera whose corners to check.</param>
		/// <returns></returns>
		protected static float DistToFurthestCameraCorner (Vector3 center, Camera camera) {
			// Get the corners of the camera in world space.
			Vector3 tl, tr, bl, br;
			bl = camera.ViewportToWorldPoint(Vector3.zero);
			br = camera.ViewportToWorldPoint(Vector3.right);
			tl = camera.ViewportToWorldPoint(Vector3.up);
			tr = camera.ViewportToWorldPoint(new Vector3(1, 1));

			// Find vectors from iris center to the corners.
			bl -= center;
			br -= center;
			tl -= center;
			tr -= center;

			// Take the longest distance of the four vectors.
			float longest;
			// For efficiency, compare square magnitudes...
			longest = Mathf.Max(bl.sqrMagnitude, br.sqrMagnitude);
			longest = Mathf.Max(longest, tl.sqrMagnitude);
			longest = Mathf.Max(longest, tr.sqrMagnitude);
			// ...then only sqrt once we've found The One.
			longest = Mathf.Sqrt(longest);
			
			return longest;
		} //No reason to burst me, as my result is needed almost immediately.

		protected Mesh createPlane (Camera camera) {
			Mesh ediMesh = new Mesh();

			Vector3 cameraPos = camera.transform.position;
			float radius = DistToFurthestCameraCorner(camera.ViewportToWorldPoint(Vector3.one * 0.5f), camera);
			
			verts = new Vector3[4] {
				new Vector3(-radius + cameraPos.x, radius + cameraPos.y, 0),
				new Vector3(radius + cameraPos.x, radius + cameraPos.y, 0),
				new Vector3(-radius + cameraPos.x, -radius + cameraPos.y, 0),
				new Vector3(radius + cameraPos.x, -radius + cameraPos.y, 0),
			};
			norms = new Vector3[4] {NORMAL, NORMAL, NORMAL, NORMAL};
			tris = new int[6] {0,1,2,1,3,2};

			ediMesh.vertices = verts;
			ediMesh.normals = norms;
			ediMesh.triangles = tris;

			return ediMesh;
		}

	}
}