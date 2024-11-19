using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Clouds.Transitions {
	[CreateAssetMenu(menuName = "Transition Presets/Fade")]
	public class FadeOut : Transition {
		//[SerializeField] Color fadeColor = Color.black;

		public override TransitionRenderState CreateMesh (float progress, bool fadingOut, Vector3 vector, ref Camera camera, ref TransitionMaster master) {
			material.SetFloat("_Alpha", math.lerp(0, 1, progress));

			return new TransitionRenderState {
				material = this.material,
				//Create a screen-covering plane to display for the duration of the transition.
				mesh = createPlane(camera),
			};
		}

	}
}