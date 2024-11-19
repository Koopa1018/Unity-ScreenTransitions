using UnityEngine;

namespace Clouds.Transitions {
	public struct TransitionSettings {
		public Transition transitionType;
		//public float transitionDuration;
		public Vector3 vector;

		public TransitionSettings (Transition transition, Vector3 arbitraryVector) {
			transitionType = transition;
			//transitionDuration = duration;
			vector = arbitraryVector;
		}
	}
#if UNITY_ADDRESSABLES
	public struct TransitionSettings_Addressable {
		public AssetReferenceTransition transitionType;
		//public float transitionDuration;
		public Vector3 vector;

		public TransitionSettings_Addressable (AssetReferenceTransition transition, Vector3 arbitraryVector) {
			transitionType = transition;
			//transitionDuration = duration;
			vector = arbitraryVector;
		}
	}
#endif
}