using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Clouds.Transitions {
	[CreateAssetMenu(menuName = "Transition Presets/Iris", fileName = "New Transition Iris")]
	public class IrisOut : Transition {
		[Range(3,360)]
		[SerializeField] int irisResolution = 180;
		[Range(1, 16)]
		[SerializeField] float irisOutPower = 2;

		public Vector2 center {get; set;}

		//Local state.
		//No need to instantiate transitions, as there's only ever one at a time.
		float anglePerCircleVertex;
		Vector3 lastCameraPos;

		public override TransitionRenderState CreateMesh (float progress, bool fadingOut, Vector3 vector, ref Camera camera, ref TransitionMaster master) {
			vector.z = 0;
			
			return new TransitionRenderState {
				material = material,
				mesh = generateIrisMesh(progress, center, DistToFurthestCameraCorner(vector, camera))
			};
		}

		Mesh generateIrisMesh (float progress, Vector2 center, float cameraRadius) {
			CreateTriangles triJob = new CreateTriangles(irisResolution);
			JobHandle triJobHandle = triJob.Schedule();
			

			//Verts will describe the two circles of the Opening.
			int twiceRes = irisResolution * 2;
			Vector3[] verts = new Vector3[twiceRes /*+ 2*/];
			//One normal per vertex, of course.
			Vector3[] norms = new Vector3[verts.Length];

			//Here's the camera position.
			Vector3 cameraPos = Vector3.zero;
			lastCameraPos = cameraPos;
			//To avoid a divide.
			anglePerCircleVertex = 360 / irisResolution;

			// Fill in vertices, alternating outer and inner verts.
			for (int i = 0; i < irisResolution; i++) {
				float degInRad = math.radians(i) * anglePerCircleVertex;

				//Outer vertex.
				verts[2*i] = new Vector3(
					math.cos(degInRad) * cameraRadius + cameraPos.x,
					math.sin(degInRad) * cameraRadius + cameraPos.y,
					0
				);
				//Set its normal.
				norms[2*i] = Vector3.forward; //-normal, which is (0,0,--1)

				// Translate progress factor into a distance from center.
				float innerRadius = math.lerp(cameraRadius, 0, math.pow(progress, irisOutPower));

				//Inner vertex.
				verts[2*i + 1] = new Vector3(
					math.cos(degInRad) * innerRadius + center.x,
					math.sin(degInRad) * innerRadius + center.y,
					0
				);
				//Set its normal.
				norms[2*i + 1] = Transition.NORMAL; //-normal, which is (0,0,--1)
			}


			Mesh returner = new Mesh();
			returner.vertices = verts;
			//@TODO: No idea if I'm better off without jobs.
			//Totes unneeded, prolly.
			triJobHandle.Complete();
			returner.triangles = triJob.outputTris.ToArray();
			triJob.outputTris.Dispose();
			returner.normals = norms;
			
			return returner;
		}
	}
}