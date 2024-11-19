using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Transitions
{
    public class IrisOutCenterPoint : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            // Get the current transition in progress.
            IrisOut transition = TransitionMaster.Instance.currentTransition as IrisOut;
            // Only continue if it's an IrisOut transition.
            if (transition != null) {
                // It's an IrisOut transition in progress.

                // Find this object's position on the screen.
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                // Pass that to the transition.
                transition.center = new Vector2(screenPosition.x, screenPosition.y);
            }
        }
    }
}
