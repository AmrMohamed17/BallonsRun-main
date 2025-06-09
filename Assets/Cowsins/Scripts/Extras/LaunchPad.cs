using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class LaunchPad : MonoBehaviour
    {
        [SerializeField] private float impulseForce;

        public UnityEvent onTrigger;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return; // Check if its the player. In case the player is not colliding with this object, do not keep going.

            // Apply a force ( vertically ) on the player.
            other.GetComponent<Rigidbody2D>().AddForce(transform.up * impulseForce, ForceMode2D.Impulse);

            // Custom methods
            onTrigger?.Invoke();
        }
    }

}