using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

using UnityEngine;

namespace Clouds.Transitions {
	/// <summary>
	/// Configures the triangle array.
	/// </summary>
	[BurstCompile(CompileSynchronously = true)]
	public struct CreateTriangles : IJob {
		[ReadOnly]
		int _resolution;

		[WriteOnly]
		NativeArray<int> tris;
		public NativeArray<int> outputTris {
			get {
				return tris;
			}
		}

		public CreateTriangles (int resolution) {
			_resolution = resolution;
			tris = new NativeArray<int> (
				_resolution * 6,
				Allocator.TempJob
			);
		}

		public void Execute() {
			int multiplied;
			int twiceRes = 2 * _resolution;
			
			for (int i = 0; i < _resolution; i++) {
				//Iterator will be incremented each vertex.
				multiplied = i;
				multiplied *= 6;

				//Tri 1 of 2
				//	TL
				tris[multiplied] = i*2;
				//	BL - should always be left of the array end.
				multiplied++;
				tris[multiplied] = i*2+1;
				//	TR
				multiplied++;
				tris[multiplied] = (i*2+2) % twiceRes;

				//Tri 2 of 2
				//	BL - should always be left of the array end.
				multiplied++;
				tris[multiplied] = i*2+1;
				//	BR
				multiplied++;
				tris[multiplied] = (i*2+3) % twiceRes;
				//	TR
				multiplied++;
				tris[multiplied] = (i*2+2) % twiceRes;
			}
		}

	}

}