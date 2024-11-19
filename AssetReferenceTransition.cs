#if UNITY_ADDRESSABLES
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	using UnityEngine.AddressableAssets;

	namespace Clouds.Transitions {
		[System.Serializable]
		public class AssetReferenceTransition : AssetReferenceT<Transition> {
			public AssetReferenceTransition(string s) : base(s) {}
		}
	}
#endif