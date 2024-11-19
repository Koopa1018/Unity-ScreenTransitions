using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

using UnityEngine;

namespace Clouds.Transitions {
	/// <summary>
	/// Structures a vertex array for iris generation.
	/// </summary>
	[BurstCompile(CompileSynchronously = true)]
	//@TODO: IJobParallelFor. Will have to selection between two variables depending on iterator parity, though.
	internal struct UpdateIrisJob : IJobParallelFor {
		[ReadOnly]
		float _radius;
		[ReadOnly]
		float _outerRadius;
		[ReadOnly]
		float4 _center; //Bizarrely, float4 is more efficient than float3.
		[ReadOnly]
		float4 _cameraPos;
		[ReadOnly]
		int _resolution;
		[ReadOnly]
		float _angleBetweenVertices;

		[WriteOnly]
		public NativeArray<float3> verts;

		public UpdateIrisJob (
			float radius,
			float outerRadius,
			Vector3 center,
			Vector3 cameraPosition,
			int resolution
		) {
			_radius = radius;
			_outerRadius = outerRadius;
			_center = new float4(center,0);
			_cameraPos = new float4(cameraPosition,0);
			_resolution = resolution;
			_angleBetweenVertices = 360/resolution;

			verts = new NativeArray<float3>(
				_resolution * 2,
				Allocator.TempJob
			);
		}

		public void Execute (int i) {
			//Parity of current iteration.
			bool isOuterVertex = i % 2 == 1;
			
			float degInRad = math.radians(math.floor(i / 2f));
			//Convert this to the distance between verts at this res.
			degInRad *= _angleBetweenVertices;

			//Calculate for either an inner or an outer vertex, depending on parity.
			float radius = math.select(_radius, _outerRadius, isOuterVertex);
			float2 center = math.select(_center, _cameraPos, isOuterVertex).xy;

			float2 sincos = new float2(math.cos(degInRad), math.sin(degInRad));

			//WRITE: Vertex position to array.
			verts[i] = new float3(sincos * radius + center, 0);
		}
		
	}
}