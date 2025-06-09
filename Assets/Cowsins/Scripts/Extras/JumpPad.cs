using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class JumpPad : MonoBehaviour
    {
        [SerializeField, Tooltip("Force to eject the player in the vertical axis.")] private float jumpForce;

        [SerializeField, Tooltip("Sound played on the player triggering the jump pad.")] private AudioClip bounceSFX;

        public UnityEvent onTrigger;

        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag != "Player") return;// Check if its the player. In case the player is not colliding with this object, do not keep going.

            // Play required SFX on bouncing.
            source.PlayOneShot(bounceSFX);

            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            PlayerStats player = other.GetComponent<PlayerStats>();

            player.UsedJumpPad();

            // Reset the player velocity to avoid glitchy or jittery movement
            rb.velocity = Vector2.zero;

            // Apply a force ( vertically ) on the player.
            rb.AddForce(other.transform.up * jumpForce, ForceMode2D.Impulse);

            // Custom events
            onTrigger?.Invoke();
        }
    }

}