using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace cowsins2D

{
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        #region variables

        [System.Serializable]
        public enum HealthRegenerationMethod
        {
            None, AlwaysRegenarates
        }

        [ReadOnly] public float health, shield;

        [SerializeField, Tooltip("Initial health & shield values. ")] private float maxHealth, maxShield;

        public float MaxHealth
        {
            get { return maxHealth; }
        }

        public float MaxShield
        {
            get { return maxShield; }
        }

        private bool usedJumpPad = false;
        public bool _UsedJumpPadUsedJumpPad
        {
            get { return usedJumpPad; }
        }


        [SerializeField, Tooltip("Time to revive when dead ( if there is any enabled checkpoint )")] private float timeToRevive;

        [Tooltip("Set the health regeneration method. None: Won´t regenerate." +
            " AlwaysRegenerates: Regenerate if the health is below the maximum value. ")]
        public HealthRegenerationMethod healthRegenerationMethod;

        [SerializeField, Tooltip("Amount of regeneration. ")] private float healthRegenerationAmount;

        [SerializeField, Tooltip("How fast the regeneration occurs. The lower this number is, the faster the regeneration will be. ")] private float healthRegenerationRate;

        [Tooltip("takeFallDamage: Enable if the player should be able to take damage.")] public bool takeFallDamage;

        [SerializeField, Tooltip("Enable to disallow fall damage when wall sliding.")] private bool dontTakeFallDamageIfWallSliding;

        [SerializeField, Tooltip("Enable to disallow fall damage when gliding. ")] private bool dontTakeFallDamageIfGliding;

        [SerializeField, Tooltip("Minimum distance to apply fall damage when landing.")] private float minimumHeightDifferenceToApplyDamage;

        [SerializeField, Tooltip("Damage applied. This depends on the height of landing.")] private float fallDamageMultiplier;

        public static bool controllable { get; private set; } = true;

        public bool isDead { get; private set; } = false;


        [Tooltip("Set to true if you want the player to blink/flash on hit.")] public bool flashesOnDamage;

        [Tooltip("Set to true if your player is a 3D model")] public bool is3D;

        [SerializeField, Tooltip("How long the effect is played.")] private float flashDuration;

        [SerializeField, Tooltip("How fast the effect is played.")] private float flashSpeed;

        [SerializeField, Tooltip("Array that contains all the spriteRenderers that will blink on damage. These must have the Blink material assigned.")]
        protected Renderer
            [] graphics;

        private float blinkValue;

        [System.Serializable]
        public class Sounds
        {
            public AudioClip getHurtSFX, getHealedSFX, reviveSFX;
        }

        [SerializeField] private Sounds sounds;

        [System.Serializable]
        public class Events
        {
            public UnityEvent onDamage,
                onHeal,
                onDie,
                onRevive;
        }

        public Events events;


        private PlayerMovement player;

        private Rigidbody2D rb;

        private InteractionManager interactManager;
        #endregion

        private void Start()
        {
            // Getting initial references
            player = GetComponent<PlayerMovement>();
            rb = GetComponent<Rigidbody2D>();
            interactManager = GetComponent<InteractionManager>();
            // Reseting health and shield
            health = maxHealth;
            shield = maxShield;

            // Set the health regeneration method logic
            if (healthRegenerationMethod == HealthRegenerationMethod.AlwaysRegenarates) StartCoroutine(ConstantHealthRegeneration());

            UIController.instance.onUpdateHealth?.Invoke(health, shield);

            GrantControl();
        }

        public float? peakHeight = null;
        private void Update()
        {
            // Turn the drag up if the player is not controllable to avoid sliding
            if (!controllable)
            {
                rb.drag = 2;
            }
            else rb.drag = 0;

            HandleFallDamage();

            // Handles death
            if (health <= 0)
            {
                isDead = true;
                LoseControl();
            }
        }
        #region IDamageable
        public void Damage(float damage)
        {
            if (isDead || GetComponent<PlayerMultipliers>().invincible || GetComponent<PlayerMovement>().dashing && GetComponent<PlayerMovement>().invincibleWhileDashing) return;
            // Calculate damage after applying damageReceivedModifier
            float modifiedDamage = damage * PlayerMultipliers.Instance.damageReceivedModifier;

            // Reduce shield by the modified damage
            shield -= modifiedDamage;

            // Check if the shield was able to absorb the entire modified damage
            if (shield >= 0)
            {
                // If the shield absorbed all the damage, the player takes no health damage
                UIController.instance.SetVignetteColor(UIController.instance.hurtVignetteColor);
            }
            else
            {
                // If the shield was not enough to absorb the entire modified damage, 
                // calculate the remaining damage after the shield is depleted.
                float damageRemaining = -shield;

                // Set shield to zero since it's fully depleted
                shield = 0;

                // Reduce the player's health by the remaining damage
                health -= damageRemaining;

                // Set vignette color to indicate that the player is hurt
                UIController.instance.SetVignetteColor(UIController.instance.hurtVignetteColor);

                // Check if the player's health is depleted
                if (health <= 0)
                {
                    health = 0;
                    Die(true);
                }
            }

            if (flashesOnDamage)
                FlashDamage();

            SoundManager.Instance.PlaySound(sounds.getHurtSFX, 1);
            UIController.instance.onUpdateHealth?.Invoke(health, shield);

            events.onDamage?.Invoke();
        }

        public void Heal(float healAmount)
        {
            if (health >= maxHealth && maxShield <= 0 || shield >= maxShield) return;
            // Apply healing to shield and health
            shield += healAmount * PlayerMultipliers.Instance.healingReceivedModifier;
            health += healAmount;

            // Make sure shield and health don't exceed their maximum values
            if (shield > maxShield) shield = maxShield;
            if (health > maxHealth) health = maxHealth;

            UIController.instance.SetVignetteColor(UIController.instance.healVignetteColor);
            SoundManager.Instance.PlaySound(sounds.getHealedSFX, 1);
            UIController.instance.onUpdateHealth?.Invoke(health, shield);

            events.onHeal?.Invoke();
        }


        public void Die(bool condition)
        {
            events.onDie?.Invoke();
            // if there is no checkpoint you cannot revive.
            // You can call custom methods here.
            if (CheckPointManager.lastCheckpoint == null) return;

            Invoke(nameof(Revive), timeToRevive);
        }

        private void Revive()
        {
            if (!isDead) return; // if you try to revive a player that is not dead, do not proceed.

            // Make sure no more Revive methods are being called at the same time
            CancelInvoke(nameof(Revive));

            // Bring the player back to life
            isDead = false;
            health = maxHealth;
            shield = maxShield;

            // Allows the player to perform abilities, movement etc...
            GrantControl();

            // Change the position of the player to match the last saved checkpoint.
            transform.position = (Vector3)CheckPointManager.lastCheckpoint;

            // PlayerStates reference
            PlayerStates state = GetComponent<PlayerStates>();

            // Changing the state to default manually.
            state.CurrentState.ExitState();
            state.CurrentState = state._States.Default();
            state.CurrentState.EnterState();

            // SFX, UI & Custom methods
            SoundManager.Instance.PlaySound(sounds.reviveSFX, 1);
            UIController.instance.onUpdateHealth?.Invoke(health, shield);

            events.onRevive?.Invoke();
        }

        public void FlashDamage()
        {
            // Stop flashing and reset the flash value
            StopCoroutine(IFlashDamage());
            blinkValue = 0;

            // Restart flash
            StartCoroutine(IFlashDamage());
        }
        public IEnumerator IFlashDamage()
        {
            // While the time is not over, iterate through each object in the sprites array
            // Change the blink value for each of these.
            var elapsedTime = 0f;
            while (elapsedTime <= flashDuration)
            {
                for (int i = 0; i < graphics.Length; i++)
                {
                    var material = is3D ? graphics[i].GetComponent<MeshRenderer>().material : graphics[i].GetComponent<Renderer>().material;
                    material.SetFloat("_FlashAmount", blinkValue);
                    blinkValue = Mathf.PingPong(elapsedTime * flashSpeed, 1f);
                    elapsedTime += Time.deltaTime;
                }
                yield return null;
            }
            // re iterate through each object and reset the material value
            for (int i = 0; i < graphics.Length; i++)
            {
                var material = graphics[i].GetComponent<Renderer>().material;
                material.SetFloat("_FlashAmount", 0);
            }

        }
        #endregion

        #region control
        public static void GrantControl() => controllable = true;

        public static void LoseControl() => controllable = false;

        public static void ToggleControl() => controllable = !controllable;

        public void CheckIfCanGrantControl()
        {
            if (isDead || interactManager.InventoryOpen) return;

            controllable = true;
        }
        #endregion

        private void HandleFallDamage()
        {

            if (!takeFallDamage || usedJumpPad)
            {
                if (player.IsGrounded()) usedJumpPad = false;
                return;
            }

            // Grab current player height
            if (player.LastOnGroundTime <= 0 && transform.position.y > peakHeight || player.LastOnGroundTime <= 0 && peakHeight == null) peakHeight = transform.position.y;

            if (player.wallSliding && dontTakeFallDamageIfWallSliding) peakHeight = null;

            if (player.gliding && dontTakeFallDamageIfGliding) peakHeight = null;

            // Check if we landed, as well if our current height is lower than the original height. If so, check if we should apply damage
            if (player.LastOnGroundTime > 0 && peakHeight != null && transform.position.y < peakHeight)
            {
                float currentHeight = transform.position.y;

                // Transform nullable variable into a non nullable float for later operations
                float noNullHeight = peakHeight ?? default(float);

                float heightDifference = noNullHeight - currentHeight;

                // If the height difference is enough, apply damage
                if (heightDifference > minimumHeightDifferenceToApplyDamage) Damage(heightDifference * fallDamageMultiplier);

                // Reset height
                peakHeight = null;
            }

        }

        private IEnumerator ConstantHealthRegeneration()
        {
            yield return new WaitForSeconds(healthRegenerationRate);
            Heal(healthRegenerationAmount);
            StartCoroutine(ConstantHealthRegeneration());
        }

        /// <summary>
        /// Inflicts fire damage on the target over multiple iterations.
        /// </summary>
        /// <param name="damage">The amount of damage inflicted by the fire with each iteration.</param>
        /// <param name="iterations">The total number of iterations the fire attack will go through.</param>
        /// <param name="iterationDuration">The duration of each iteration of the fire attack (in seconds).</param>
        /// <param name="timeBetweenIterations">The time interval between consecutive iterations of the fire attack (in seconds).</param>
        public void FireDamage(float damage, float iterations, float iterationDuration, float timeBetweenIterations)
        {
            StartCoroutine(HandleFireDamage(damage, iterations, iterationDuration, timeBetweenIterations));
        }

        private IEnumerator HandleFireDamage(float damage, float iterations, float iterationDuration, float timeBetweenIterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                float timeElapsed = 0f;

                // Apply damage repeatedly for each iteration duration
                while (timeElapsed < iterationDuration)
                {
                    Damage(damage * Time.deltaTime / iterationDuration);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }

                // Wait for the specified time between iterations
                yield return new WaitForSeconds(timeBetweenIterations);
            }
        }

        public void UsedJumpPad()
        {
            usedJumpPad = true;
        }

    }

}