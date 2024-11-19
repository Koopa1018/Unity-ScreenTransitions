using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Clouds.Transitions
{
	public enum TransitionState {
		Open = 0,
		Opening,
		Closing,
		Closed
	}

	//@TODO: Confirm whether being Jobified does anything to the performance.
	public partial class TransitionMaster : MonoBehaviour {
		//Singleton data.
		protected static TransitionMaster _instance;
		public static TransitionMaster Instance {
			get {
				if (_instance == null) {
					_instance = FindObjectOfType<TransitionMaster>();
					if (_instance == null) {
						_instance = createMasterObject();
						//Can't get components in this method--Awake calls first!
						DontDestroyOnLoad(_instance);
					}
				}

				return _instance;
			}
		}
		private static TransitionMaster createMasterObject () {
			GameObject owner = new GameObject(
				"Transition Master",
				typeof(MeshFilter),
				typeof(MeshRenderer),
				typeof(TransitionMaster)
			);
			owner.layer = MASTER_OBJECT_LAYER;
			return owner.GetComponent<TransitionMaster>();
		}

		//Constants
		const float zDepth = 5;
		internal const float outerRadius = 20;
		internal readonly static Vector3 normal = Vector3.back;
		const int MASTER_OBJECT_LAYER = 0; //"Default"

		static MaterialPropertyBlock _materialProperties;
		internal static MaterialPropertyBlock materialProperties {get => _materialProperties;}

		//Serialized fields.
		//[SerializeField] internal Transition[] transitionTypes;
		[SerializeField] internal MeshFilter myFilter;
		[SerializeField] internal MeshRenderer myRenderer;

		//Outgoing events.
		/// <summary>
		/// Event that will be run when a transition finishes.
		/// Subscribers will be forgotten on transition finish; if this is not desired, use <see cref="OnTransitionFinishedPersistent"/>.
		/// </summary>
		[HideInInspector] public UnityEvent OnTransitionFinished = new UnityEvent();
		/// <summary>
		/// Event that will be run when a transition finishes.
		/// Subscribers will persist after the transition finishes; if this is not desired, use <see cref="OnTransitionFinished"/>.
		/// </summary>
		[HideInInspector] public UnityEvent OnTransitionFinishedPersistent = new UnityEvent();
		/// <summary>
		/// Event that will be run when the active scene switches.
		/// This can be used to e.g. update the center of an iris-in effect.
		/// </summary>
		[HideInInspector] public UnityEvent OnNewSceneBecameActive = new UnityEvent();

		//Public state.
		TransitionState _transitionState = TransitionState.Open;
		public TransitionState transitionState {get => _transitionState; internal set => _transitionState = value;}

		public float transitionProgress {get; private set;}
		public bool transitionIsFadingOut {get; private set;}
		/// <summary>
		/// An arbitrary vector which transitions can use in arbitrary ways.
		/// </summary>
		public Vector2 transitionVector {get; set;}

		public Vector3 lastVector {get; private set;}
		//public float lastDuration {get; private set;}
		public bool lastWasFading  {get; private set;}
		internal Transition currentTransition;
		public Transition lastTransition {get; private set;}

		//Private state.
		Mesh myMesh;
		
		// Use this for initialization
		void Awake () {
			//Fetch mesh components.
			myFilter = GetComponent<MeshFilter>();
			myRenderer = GetComponent<MeshRenderer>();

			//Assert if we don't have them.
			Debug.Assert(myFilter != null, "No mesh filter to render a transition into.", this.gameObject);
			Debug.Assert(myRenderer != null, "No mesh renderer for fade/iris generation.", this.gameObject);

			//Put code in place to run when active scene changes.
			SceneManager.activeSceneChanged += _activeSceneChanged;
			//@TODO: For a game when active scene can change mid-gameplay,
			//you'd need a totally different system.

			//EDITOR ONLY: Register for cleanup on close play mode.
#if UNITY_EDITOR
			UnityEditor.EditorApplication.playModeStateChanged += OnExitPlaymode;
		}

		void OnExitPlaymode (UnityEditor.PlayModeStateChange newstate) {
			if (newstate == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
				//Destroy the transition master's instance.
				Destroy(_instance);
				_instance = null;
			}

#endif
		}

		/// <summary>
		/// Events that are always run when the active scene changes.
		/// </summary>
		/// <param name="sceneBefore">Unused; here to allow SceneManager.sceneLoaded to register the method.</param>
		/// <param name="sceneAfter">Unused; here to allow SceneManager.sceneLoaded to register the method.</param>
		void _activeSceneChanged(Scene sceneBefore, Scene sceneAfter) {
			//Run registered events.
			OnNewSceneBecameActive.Invoke();
		}

		/*
		//For debugging;
		float timeElapsed = 0;

		void Update () {
			updateIrisMesh(ref verts, Vector3.zero, math.sin(timeElapsed) * 8);
			myMesh.vertices = verts;
			timeElapsed += Time.deltaTime * 0.6f;
			timeElapsed %= 3.2f;
		}
		*/ //Causes a pulsating Opening to show, centered at (0,0,0).
		//Must add a createIris command to Awake.

		/// <summary>
		/// Perform a transition of some kind.
		/// </summary>
		/// <param name="settings">Configuration details on how the transition is to occur, including its type.</param>
		/// <param name="fadeOut">Should this transition fade to black (or the type's equivalent)? If false, will fade in.</param>
		public static void DoTransition (TransitionSettings settings, bool fadeOut) {
			DoTransition(settings.transitionType, settings.vector, fadeOut);
		}

		/// <summary>
		/// Perform a screen transition of some kind.
		/// </summary>
		/// <param name="transitionAsset">A reference to a transition asset.</param>
		/// <param name="vector">Arbitrary vector value used by some transitions, often iris center or wipe orientation.</param>
		/// <param name="fadeOut">Should this transition fade to black (or the type's equivalent)? If false, will fade in.</param>
		public static void DoTransition (
			Transition transition,
			Vector3 vector,
			bool fadeOut
		) {
			//VALIDATE: Is the passed transition valid?
			if (transition == null) {
				Debug.LogWarning("Transition loading came up blank.");
				Instance.doFinished();
				return;
			}

			// Post the requested transition in a place transition rendering can
			// access it.
			Instance.currentTransition = transition;
			Instance.transitionIsFadingOut = fadeOut;
			Instance.transitionVector = vector;


			//If the last transition is different from this one (and not null), unload it.
			if (transition != Instance.lastTransition && Instance.lastTransition != null) {
				Resources.UnloadAsset(Instance.lastTransition);
			}

			//Save all the parameters to their respective Last fields.
			Instance.lastVector = vector;
			Instance.lastWasFading = fadeOut;
			Instance.lastTransition = transition;

			//Start the perform-animation-and-release-asset coroutine.
			Instance.StartCoroutine(doTransitionAnimation(transition, fadeOut));

			//@TODO: Spin until loading transition is completed.
		}

		static IEnumerator doTransitionAnimation (
			Transition transition,
			bool fadeOut
		) {	
			Instance.transitionProgress = fadeOut? 0 : 1;

			//Set the transition state to whichever one we're to undergo.
			Instance.transitionState = fadeOut ? TransitionState.Closing : TransitionState.Opening;

			if (transition.duration != 0) {
				float invDuration = 1 / transition.duration;

				//Perform the animation.
				while (fadeOut? Instance.transitionProgress < 1 : Instance.transitionProgress > 0) {
					Instance.transitionProgress += (fadeOut? 1 : -1) * Time.unscaledDeltaTime * invDuration;
        			// Debug.Log("Transition progress: " + Instance.transitionProgress);
					yield return null;
				}
			}

			//Set the transition state to whichever one we're in now.
			Instance.transitionState = fadeOut ? TransitionState.Closed : TransitionState.Open;

			//If we've just opened, clean up transition data so we don't have to store it.
			if (!fadeOut) {
				Instance.currentTransition = null;
			}

			Instance.doFinished();
		}

		void doFinished () {
			//Run any events subscribed to the end of a transition.
			OnTransitionFinished?.Invoke();
			OnTransitionFinishedPersistent.Invoke();
			//Wipe all subscriptions to the non-persistent event.
			OnTransitionFinished = new UnityEvent();
		}
		
	}
}