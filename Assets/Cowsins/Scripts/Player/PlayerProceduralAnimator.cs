using UnityEngine;

namespace cowsins2D
{
    public class PlayerProceduralAnimator : MonoBehaviour
    {
        private PlayerMovement mov;
        private Animator anim;


        [SerializeField, Tooltip("Target object to apply the animations.")] private Transform graphics;
        [SerializeField, Tooltip("Speed to adjust the scale.")] private float scaleLerpSpeed;
        [SerializeField, Tooltip("Adjust the scale when jumping.")] private Vector2 jumpScale;
        [SerializeField, Tooltip("Adjust the scale when landing.")] private Vector2 landScale;

        [Header("Movement Tilt")]
        [SerializeField, Tooltip("Maximum rotation allowed when moving.")] private float maxTilt;
        [SerializeField, Tooltip("Speed to adjust the rotation. ")] [Range(0, 1)] private float tiltSpeed;

        public bool startedJumping { private get; set; }
        public bool justLanded { private get; set; }

        public float currentVelY { private get; set; }

        private Vector3 targetScale;

        private void Start()
        {
            // Initial references
            mov = GetComponent<PlayerMovement>();
            anim = graphics.GetComponentInChildren<Animator>();

            // Set events
            mov.onJump = OnJump;
            mov.onLand = OnLand;

            // Grabs the idle scale. The scale will always lerp to this. 
            targetScale = graphics.localScale;
        }

        private void OnJump() => graphics.localScale = (Vector3)jumpScale;

        private void OnLand() => graphics.localScale = (Vector3)landScale;

        private void LateUpdate()
        {
            #region Tilt
            float tiltProgress;

            int mult = -1;

            // If the player is wall sliding tilt the graphics
            if (mov.wallSliding)
            {
                tiltProgress = 0.25f;
            }
            else
            {
                tiltProgress = Mathf.InverseLerp(-mov.runSpeed, mov.runSpeed, mov.rb.velocity.x);
                mult = (mov.facingRight) ? 1 : -1;
            }

            // handle tilting / rotation
            float newRot = ((tiltProgress * maxTilt * 2) - maxTilt);
            float rot = Mathf.LerpAngle(graphics.transform.localRotation.eulerAngles.z * mult, newRot, tiltSpeed);
            graphics.transform.localRotation = Quaternion.Euler(0, 0, rot * mult);
            #endregion

            CheckAnimationState();

        }

        private void Update()
        {
            // handle the scale
            if (graphics.localScale != targetScale) graphics.localScale = Vector3.Lerp(graphics.localScale, targetScale, scaleLerpSpeed * Time.deltaTime);
        }

        private void CheckAnimationState()
        {
            // Run animations on jumping
            if (startedJumping)
            {
                anim.SetTrigger("Jump");

                startedJumping = false;
                return;
            }

            // Run landing animations
            if (justLanded)
            {
                anim.SetTrigger("Land");
                justLanded = false;
                return;
            }
        }
    }
}