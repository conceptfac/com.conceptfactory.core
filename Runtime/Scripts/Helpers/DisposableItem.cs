using Concept.UI;
using System.Collections;
using UnityEngine;

namespace Concept.Helpers
{
    /// <summary>
    /// This class handles the disposal of a GameObject after a customizable delay or triggered by Animation Events.
    /// It allows for setting a specific time before the GameObject is destroyed.
    /// </summary>
    public class DisposableItem : MonoBehaviour
    {
        public bool selfDestroy;

        [ShowIf("selfDestroy")]
        [Tooltip("Choose 'selfDestroy' enabled to destroy by animation event.")]
        [SerializeField] private float _destroyAfter = 10f;

        /// <summary>
        /// Initializes the object and starts the destruction process if selfDestroy is enabled.
        /// </summary>
        private void Start()
        {
            if (selfDestroy) DisposeAfter(_destroyAfter);
        }

        /// <summary>
        /// This method disposes of an item after a predefined delay in seconds.
        /// It can be called by animation events also.
        /// </summary>
        /// <param name="delay">The time in seconds to wait before disposing of the item.</param>
        public void DisposeAfter(float delay)
        {
            StartCoroutine(DisposeAfterCOR(delay));
        }

        /// <summary>
        /// Coroutine that waits for the specified delay and then destroys the GameObject.
        /// </summary>
        /// <param name="delay">The time in seconds to wait before disposing of the item.</param>
        /// <returns>An IEnumerator that handles the delay before destroying the GameObject.</returns>
        private IEnumerator DisposeAfterCOR(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
