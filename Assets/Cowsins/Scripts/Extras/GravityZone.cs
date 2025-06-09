using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class GravityZone : MonoBehaviour
    {
        [SerializeField, Tooltip("Override the gravity scale value. Notice that the default gravity scale value is 9.8.")] private float gravityScale;


        public UnityEvent onStay, onLeave;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return; // If this is the player, perform the operations

            // Grab a reference for the player
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            // Enable player external gravity and set the value.
            player.externalGravityScale = true;
            player.SetGravityScale(gravityScale);
            if (gravityScale == 0) player.rb.velocity = Vector2.zero;

            onStay?.Invoke();
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return; // If this is the player, perform the operations

            // Grab a reference for the player
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            // Disable external gravity
            player.externalGravityScale = false;

            onLeave?.Invoke();
        }
    }

}