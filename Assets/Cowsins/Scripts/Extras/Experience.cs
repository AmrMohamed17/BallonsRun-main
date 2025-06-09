using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace cowsins2D
{
    public class Experience : MonoBehaviour, ITriggerable
    {
        [SerializeField] private float minXp, maxXp;

        [SerializeField, Tooltip("Sound on picking up ")] private AudioClip pickUpSFX;

        public UnityEvent onCollect;

        [SerializeField] private float moveSpeed = 1.0f; // Speed of the movement
        [SerializeField] private float moveDistance = 1.0f; // Distance to move up and down
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
            StartCoroutine(StartMovingWithDelay());
        }
        private IEnumerator StartMovingWithDelay()
        {
            // Generate a random delay before starting movement
            float offset = Random.Range(.9f, 1.4f);

            // Start moving
            while (true)
            {
                // Calculate vertical movement using a sine wave
                float verticalMovement = Mathf.Sin(Time.time * moveSpeed * offset) * moveDistance;
                transform.position = startPos + new Vector3(0, verticalMovement, 0);
                yield return null;
            }
        }

        public void Trigger(GameObject target)
        {
            if (ExperienceManager.instance == null) return; // If we are not using XP in our game we should not pick up XP or we should not be able to.

            onCollect?.Invoke();

            // Generate a random amount of XP.
            float amount = Random.Range(minXp, maxXp);
            // Add the experience to the player.
            ExperienceManager.instance.AddExperience(amount);
            UIController.addXP?.Invoke();

            // Play SFX
            SoundManager.Instance.PlaySound(pickUpSFX, 1);

            // Destroy the XP.
            Destroy(this.gameObject);
        }

        public void StayTrigger(GameObject target) { }

        public void ExitTrigger(GameObject target) { }

    }
}
