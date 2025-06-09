using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class DestructiblePlatform : MonoBehaviour
    {
        [SerializeField, Tooltip("After a player triggers it, time to destroy in seconds.")] private float timeToDestroyAfterActivation;

        [SerializeField] private UnityEvent OnActivation, OnDestroyed;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.transform.CompareTag("Player")) return; // If the collision is not the player, do not keep going

            // Destroy after activation.
            Destroy(this.gameObject, timeToDestroyAfterActivation);
            OnActivation?.Invoke();
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }

    }

}