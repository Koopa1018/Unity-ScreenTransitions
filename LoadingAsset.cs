#if UNITY_ADDRESSABLES
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;

	namespace Clouds.Transitions
	{
		//@TODO: This should absolutely be part of the Utility package,
		//but I don't feel like doing that right now.
		public struct LoadingAsset <T> where T : UnityEngine.Object {
			public AssetReferenceT<T> reference;
			public AsyncOperationHandle<T> handle;

			public LoadingAsset(AssetReferenceT<T> assetReference) {
				reference = assetReference;
				handle = reference.LoadAssetAsync();
			}

			public LoadingAsset(AssetReferenceT<T> assetReference, AsyncOperationHandle<T> loadHandle) {
				reference = assetReference;
				handle = loadHandle;
			}
		}
	}
#endif