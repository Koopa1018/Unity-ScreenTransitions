using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Transitions {
	public class GlobalTransitionList : ScriptableObject {
    	[SerializeField] Transition[] _transitions;

		public Transition this [int i] {
			get {
				return _transitions[i];
			}
		}
		
	}
}