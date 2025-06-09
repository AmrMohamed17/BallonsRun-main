using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public enum JumpMethod
    {
        Default, HoldToJumpHigher
    }

    [System.Serializable]
    public enum PlayerOrientationMethod
    {
        None,
        HorizontalInput,
        AimBased,
        Mixed
    }
    [System.Serializable]
    public enum DashMethod
    {
        None,
        Default,
        AimBased,
        HorizontalAimBased
    }
    [System.Serializable]
    public enum GlideDurationMethod
    {
        None,
        TimeBased
    }
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the player graphics.")] private Transform graphics;

        public Transform Graphics { get { return graphics; } }

        [Space(5)]
        [Tooltip("Gravity strength.")]
        public float gravityScale;
        [Tooltip("Gravity added. ( As a multiplier ).")] [SerializeField] private float fallGravityMult;
        [Tooltip("Limit the maximum fall speed.")] [SerializeField] private float maxFallSpeed;
        [Tooltip("Limit the maximum vertical speed ( going upwards )")] [SerializeField] private float maxUpwardsSpeed;

        [Space(20)]
        [Tooltip("If enabled, the player will not be able to walk, just to run.")]
        [SerializeField] private bool autoRun;
        [Tooltip("Speed while walking.")] public float walkSpeed;
        [Tooltip("Speed while running.")] public float runSpeed;
        [Tooltip("Speed while crouching.")] public float crouchSpeed;
        [Tooltip("Speed horizontally while in a ladder.")] public float horizontalLadderSpeed;
        [Tooltip("Speed to climb a ladder.")] public float verticalLadderSpeed;
        [Tooltip("Capacity to gain speed and reach the maximum speed.")] [SerializeField] private float runAcceleration;
        [Tooltip("Capacity to lose speed and go idle.	")] [SerializeField] private float runDecceleration;
        [Tooltip("Capacity to gain speed and reach the maximum speed mid-air.")]
        [Space(5)]
        [Range(0f, 1)] public float accelInAir;
        [Tooltip("Capacity to lose speed mid-air.	")] [Range(0f, 1)] public float deccelInAir;
        [Tooltip("remove the deceleration mid-air.")]
        [Space(5)]
        public bool doConserveMomentum = true;
        [Tooltip("Maximum floor angle to consider as a walkable area.")] [SerializeField] private float maxFloorAngle;

        [Space(20)]
        [Tooltip("Configures the way the jump works. Default: Press to jump. You are able to hold. Hold to Jump Higher: Press = Tiny jump. Hold = Higher jumps depending on the held time.")]
        [Header("Jump")]
        [SerializeField] private JumpMethod jumpMethod;
        [Tooltip("How many jumps you can do before landing. 1 is the default value assigned.")] [SerializeField, Min(0)] private int amountOfJumps;
        [Tooltip("How tall you will jump.")] [SerializeField] private float jumpForce;

        [Header("Both Jumps")]
        [Tooltip("Adjust the stiffness of the jump. The higher this value is the sharper it will perform. " +
            "Usually, sharp jumps are more responsive than smooth jumps, but it all depends on the style you are looking for.")]
        [SerializeField] private float apexReachSharpness;
        [Tooltip("Jump Hang is the movement at the apex of the jump. This adjusts the gravity of the jump in that point.")] [Range(0f, 1)] public float jumpHangGravityMult;
        [Tooltip("Enable jump hang depending on the current velocity, so it gets activated before reaching the apex.")] [SerializeField] private float jumpHangTimeThreshold;
        [Tooltip("Acceleration multiplier on the apex of the jump.")]
        [Space(0.5f)]
        [SerializeField] private float jumpHangAccelerationMult;
        [Tooltip("Speed multiplier on the apex of the jump.")] [SerializeField] private float jumpHangMaxSpeedMult;
        [Tooltip("When falling, you can press “S” to fall faster. Adjust the multiplier here.")] [SerializeField] private float fastFallMultiplier;

        [Header("Wall Jump")]
        public bool allowWallJump;
        [Tooltip("impulse of the wall jump.")] [SerializeField] private Vector2 wallJumpForce;
        [Tooltip("Speed to adjust the velocity from walking to running when you wall jump.")]
        [Space(5)]
        [Range(0f, 1f)] public float wallJumpRunLerp;
        [Tooltip("Duration of the wall jump before going back to default state.")] [Range(0f, 1.5f)] public float wallJumpTime;

        public bool allowSlide;
        [Space(20)]
        [Tooltip("Enable to reset the amount of jumps when wall sliding.")]
        public bool wallSlidingResetsJumps;
        [Tooltip("Velocity of wall sliding.")] public float wallSlideSpeed;
        [Tooltip("Interval in seconds to display Wall slide visual effects-")] public float wallSlideVFXInterval;

        [Tooltip("GameObject that represents the visual effects for sliding.")] [SerializeField] private GameObject wallSlideVFX;

        [Header("Dash")]
        [Tooltip("Configure the behaviour of the dash. None: Disables dashing. Default: Based on Horizontal movement. " +
            "AimBased: Dash wherever you are aiming. HorizontalAimBased: Dash Horizontally based on the aim. ")]
        public DashMethod dashMethod;
        [Tooltip("How many dashes you have available.")]
        [SerializeField, Min(0)] private int amountOfDashes;

        public int AmountOfDashes
        {
            get { return amountOfDashes; }
        }


        [Tooltip("Time in seconds to being able to use dashes again.")] [SerializeField] private float dashCooldown;

        [Tooltip("Gravity when dashing.")] [SerializeField] private float dashGravityScale;

        [Tooltip("Duration in seconds of the dash.")] [SerializeField] private float dashDuration;

        [Tooltip("Travel velocity when dashing.")] [SerializeField] private float dashSpeed;

        [Tooltip("When you stopped dashing, time to being able to perform a new dash ( to avoid dash clipping )")] [SerializeField, Min(.1f)] private float dashInterval;

        [Tooltip("If enabled, the player won´t be able to receive damage while dashing.")] public bool invincibleWhileDashing;

        private Vector2 dashDirection;

        public bool dashing { get; private set; } = false;

        public bool canDash { get; private set; } = true;

        public int currentDashes { get; private set; }

        [Header("Glide")]

        public bool canGlide;

        [Tooltip("Speed of movement when gliding.")] [SerializeField] private float glideSpeed;

        [Tooltip("How fast the player goes downwards when gliding.")] [SerializeField] private float glideGravity;

        [Tooltip("Allows dash if gliding.")] [SerializeField] private bool canDashWhileGliding;

        [Tooltip("Adjusts the duration method for the gliding. None: Infinite gliding. " +
            "TimeBased: Adjusts the duration based on a timer in seconds.")]
        public GlideDurationMethod glideDurationMethod;

        [Tooltip("Duration of the glide in seconds.")] public float maximumGlideTime;

        [Tooltip("Allow the player to orientate ( based on the current orientation method ) while gliding.")] public bool handleOrientationWhileGliding;

        public bool gliding { get; private set; } = false;


        [Tooltip("Allow the player to crouch.")]
        [Header("Crouch")]
        public bool allowCrouch;

        [Tooltip("Allow the player to crouch while airborne.")] public bool canCrouchSlideMidAir;

        [Tooltip("if the player has enough speed momentum when crouching, it will perform a slide. Adjust the force of that slide.")] [SerializeField] private float crouchSlideSpeed;

        [Tooltip("Adjust the duration of the crouch slide.")] public float crouchSlideDuration;


        //Stamina
        [Header("Stamina")]
        [Tooltip("You will lose stamina on performing actions when true.")]
        public bool usesStamina;

        public float stamina { get; private set; }

        [Tooltip("Minimum stamina required to being able to run again."), SerializeField]
        private float minStaminaRequiredToRun;

        [Tooltip("Max amount of stamina.")]
        public float maxStamina;

        [SerializeField, Min(1), Tooltip("Speed to regenerate the stamina. ")] private float staminaRegenMultiplier;


        [Tooltip("Amount of stamina lost on jumping."), SerializeField]
        private float staminaLossOnJump;

        [Tooltip("Amount of stamina lost on jumping."), SerializeField]
        private float staminaLossOnWallJump;

        [Tooltip("Amount of stamina lost on sliding."), SerializeField]
        private float staminaLossOnCrouchSlide;

        private bool canRun = true;

        [Header("Others")]
        [Tooltip("Maximum height allowed for a surface to be allowed as a step.")] [SerializeField] private float stepHeight;

        [Tooltip("Handles the way the player is oriented. None: The player cannot orientate itself. HorizontalInput: The player orientates based on the A & D inputs. " +
            "Aim Based, the player looks at the crosshair. Mixed: Mix between Aim Based and HorizontalInput. ")]
        [SerializeField] private PlayerOrientationMethod playerOrientationMethod;

        [Tooltip("Damage applied on an enemy when landing on it.")] [SerializeField] private float landOnEnemyDamage;
        [Tooltip("Upwards impulse applied to the player when landing on an enemy.")] [SerializeField] private float landOnEnemyImpulse;

        [Tooltip("Stores the SurfaceEffects for each surface.")] [SerializeField] private List<SurfaceEffect> step = new List<SurfaceEffect>();

        private Dictionary<int, int> sortedGroundLayer = new Dictionary<int, int>();

        [Tooltip("Speed at which the footsteps are played.")] [SerializeField] private float footstepsInterval;

        [Tooltip("Volume at which the footsteps are played.")] [SerializeField] private float footstepsVolume;

        [Tooltip("Volume at which the land SFX is played.")] [SerializeField] private float landVolume;

        [Tooltip("Camera Shake to apply to the main camera on landing"), SerializeField, Range(0, 2)] private float landCameraShake = .35f;

        public delegate void OrientatePlayer();

        public OrientatePlayer orientatePlayer;

        public delegate void OnJump();

        public OnJump onJump;

        public delegate void OnLand();

        public OnLand onLand;


        [Header("Assists")]
        [Tooltip("Coyote Jump allows the player to jump responsively, even when leaving a ground or surface, there is still a timing that allows the player to jump." +
            "Adjust this number to configure the way that works.")]
        [Range(0.01f, 0.5f)] public float coyoteTime;
        [Tooltip("Allows for a more responsive jump.")] [Range(0.01f, 0.5f)] public float jumpInputBufferTime;


        [System.Serializable]
        public class Events
        {
            public UnityEvent onJump,
                onWallJump,
                onLand,
                onStartCrouch,
                onCrouching,
                onStopCrouch,
                onStartDash,
                onStartGlide,
                onGliding,
                onStopGliding;
        }

        public Events events;

        #region COMPONENTS
        public Rigidbody2D rb { get; private set; }

        private PlayerStats stats;
        #endregion

        #region STATE PARAMETERS

        public float currentSpeed { get; private set; }
        public bool facingRight { get; private set; } = true;

        // This variable allows for external scripts such as GravityZone.cs to modify the gravity scale of the player.
        [HideInInspector] public bool externalGravityScale = false;

        public float groundAngle { get; private set; }
        public int currentJumps { get; private set; }
        public bool jumping;
        public bool wallJumping { get; private set; }
        public bool wallSliding { get; private set; }

        public float wallSlideVFXTimer;

        public bool staminaAllowsJump { get; private set; }

        public bool crouching { get; private set; } = false;

        public float LastOnGroundTime { get; private set; }
        public bool LastOnWallTime;
        public bool WallRight;
        public bool WallLeft;


        public bool ladder { get; private set; } = false;

        private bool jumpCut;
        private bool jumpFalling;

        public float _wallJumpStartTime { get; private set; }
        private int _lastWallJumpDir;

        public int currentLayer { get; private set; }

        private float footstepsTimer;

        #endregion

        #region INPUT PARAMETERS

        public float LastPressedJumpTime { get; private set; }
        public float LastPressedDashTime { get; private set; }
        #endregion

        #region CHECK PARAMETERS
        [Header("Checks")]
        [SerializeField] private Vector3 groundCheckOffset;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
        [Space(10)]
        [SerializeField] private Vector3 ceilingCheckOffset;
        [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.49f, 0.03f);
        [Space(10)]
        [SerializeField] private Vector3 leftWallCheckOffset;
        [SerializeField] private Vector3 rightWallCheckOffset;
        [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
        #endregion

        #region LAYERS & TAGS
        [Header("Layers & Tags")]
        [SerializeField] private LayerMask whatIsGround;
        #endregion

        [System.Serializable]
        public class Sounds
        {
            public AudioClip jumpSFX, wallJumpSFX, dashSFX, startGlideSFX, startCrouchSFX;
        }

        public Sounds sounds;

        public PlayerInputs PlayerInput { get; private set; }

        private PlayerStates state;

        private WeaponController weaponController;

        private void Awake()
        {
            // Initial references and settings
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            state = GetComponent<PlayerStates>();
            weaponController = GetComponent<WeaponController>();
            footstepsTimer = footstepsInterval;
        }

        private void Start()
        {
            // Set the events to use them later
            GatherEvents();
            // Detects all the available ground layers to display appropriate VFX and play the right SFX based on the current surface
            SortGroundLayer();

            ResetStamina();

            // Set the dashes to the max amount
            currentDashes = amountOfDashes;
        }
        private void Update()
        {
            // Handle inputs. Retrieve them from the InputManager.
            PlayerInput = InputManager.playerInputs;
            UpdateTimers();
            // Handles gravity
            ApplyGravity();
        }

        private void FixedUpdate()
        {
            // Stop from sliding without wanting to.
            PreventUnvoluntarySliding();
            // Handles Stamina
            Stamina();
        }

        private void UpdateTimers()
        {
            float deltaTime = Time.deltaTime;
            LastOnGroundTime -= deltaTime;
            LastPressedJumpTime -= deltaTime;
            LastPressedDashTime -= deltaTime;
        }

        public void HorizontalOrientation()
        {
            // Orientate the player horizontally
            if (PlayerInput.HorizontalMovement != 0)
                SetOrientation(PlayerInput.HorizontalMovement > 0);
        }

        // Orientate the player horizontally based on where the player is aiming
        public void AimBasedOrientation()
        {
            Vector3 crosshairPos = Crosshair.Instance.transform.position;
            Vector2 dir = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller ? -Gamepad.current.rightStick.ReadValue().normalized : crosshairPos - transform.position;

            if (dir.x != 0) SetOrientation(dir.x > 0);
        }

        // Auxiliar variables
        Vector3 oldPos = Vector3.zero;

        // It mixes Horizontal and Aim Based Orientations.
        private void MixedOrientation()
        {
            Vector3 crosshairPos = Crosshair.Instance.transform.position;
            if (PlayerInput.HorizontalMovement == 0 && oldPos != crosshairPos)
            {
                AimBasedOrientation();
                return;
            }
            HorizontalOrientation();
            oldPos = crosshairPos;
        }

        public void HandleJumpInput()
        {
            if (PlayerInput.JumpingDown)
                OnJumpInput();
        }
        public void HandleJumpCutInput()
        {
            if (PlayerInput.JumpingUp)
                OnJumpUpInput();
        }

        public void ReduceJumpAmount() => currentJumps--;

        public void ResetJumpAmounts() => currentJumps = amountOfJumps;

        public void CheckCollisions()
        {
            // Get a reference for the actual ground check position
            Vector2 groundCheckBoxPosition = transform.position + groundCheckOffset;

            // Check if there is a collision running.
            Collider2D groundHit;
            if (groundHit = Physics2D.OverlapBox(groundCheckBoxPosition, groundCheckSize, 0, whatIsGround))
            {
                // Determine the ground angle to orientate the player properly.
                groundAngle = Vector2.Angle(groundHit.transform.up, Vector3.up);
                if (groundAngle < maxFloorAngle)
                {
                    transform.up = groundHit.transform.up;
                }

                // If the player is grounded but it was not grounded the previous frame, Land.
                if (LastOnGroundTime <= 0) Land();

                // Handle coyote jump and reset jumps when grounded.
                LastOnGroundTime = coyoteTime;
                currentJumps = amountOfJumps;

                // Determines if colliding with an enemy
                Collider2D hit;
                if ((hit = Physics2D.OverlapBox(groundCheckBoxPosition, groundCheckSize, 0, whatIsGround)) != null)
                {
                    // Check if it is an enemy
                    currentLayer = sortedGroundLayer[hit.gameObject.layer];
                    if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy") && hit.TryGetComponent<IDamageable>(out var damageable))
                    {
                        // Apply damage to the enemy
                        damageable.Damage(landOnEnemyDamage);
                        // Reset the velocity to avoid weird movement behaviour
                        rb.velocity = Vector2.zero;
                        // Apply a force vertically
                        rb.AddForce(transform.up * landOnEnemyImpulse, ForceMode2D.Impulse);
                    }
                }
            }
            else transform.up = Vector3.up;


            // Left Wall Check
            if (Physics2D.OverlapBox(transform.position + leftWallCheckOffset, _wallCheckSize, 0, whatIsGround) && (facingRight || (!facingRight && !wallJumping)))
                WallRight = true;
            else WallRight = false;

            // Right Wall Check
            if (Physics2D.OverlapBox(transform.position + rightWallCheckOffset, _wallCheckSize, 0, whatIsGround) && (!facingRight || (facingRight && !wallJumping)))
                WallLeft = true;
            else WallLeft = false;

            LastOnWallTime = WallRight || WallLeft ? true : false;
        }

        private void PreventUnvoluntarySliding()
        {
            // Removes gravity when grounded and idle on a slope to prevent sliding down.
            if (groundAngle < maxFloorAngle && LastOnGroundTime > 0 && PlayerInput.HorizontalMovement == 0 && !jumping)
            {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
            }
        }
        private void SortGroundLayer()
        {
            int sortedIndex = 0;
            for (int i = 0; i < 32; i++)
            {
                if (whatIsGround == (whatIsGround | (1 << i)))
                {
                    sortedGroundLayer[i] = sortedIndex;
                    sortedIndex++;
                }
            }
        }

        public bool CheckCeiling()
        {
            // Check if there is a ceiling above before uncrouching
            if (Physics2D.OverlapBox(transform.position + ceilingCheckOffset, ceilingCheckSize, 0, whatIsGround))
            {
                return true;
            }
            return false;
        }

        public bool CheckIfPerformJump()
        {
            // If all the conditions are met, the player is able to jump.
            if (CanJump() && LastPressedJumpTime > 0 && currentJumps > 0)
            {
                rb.velocity = Vector2.zero;
                jumping = true;
                wallJumping = false;
                jumpCut = jumpMethod == JumpMethod.Default ? true : false;
                jumpFalling = false;
                return true;
            }
            return false;
        }

        public bool CheckIfPerformWallJump()
        {
            // If all the conditions are met, the player is able to wall jump.
            if (CanWallJump() && LastPressedJumpTime > 0 && allowWallJump)
            {
                wallJumping = true;
                jumping = false;
                jumpCut = false;
                jumpFalling = false;
                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (WallRight) ? -1 : 1;
                WallJump(_lastWallJumpDir);
                return true;
            }
            return false;
        }

        public void StopWallJump() => wallJumping = false;
        public void CheckIfJumpingOrWallrunning()
        {
            if (LastOnGroundTime > 0 && !jumping && !wallJumping)
            {
                jumpCut = false;
                jumpFalling = false;
            }
        }


        public bool CheckSlideStatus()
        {
            // Determine wether the player is sliding or not
            if (CanSlide() && ((WallLeft && PlayerInput.HorizontalMovement < 0) || (WallRight && PlayerInput.HorizontalMovement > 0)) && allowSlide)
            {
                wallSliding = true;
                return true;
            }
            else
            {
                wallSliding = false;
                return false;
            }
        }

        private void ApplyGravity()
        {
            // Return if we want other scripts to manage the gravity scale of our player.
            if (externalGravityScale || ladder) return;

            if (!dashing && !gliding)
            {
                if (wallSliding && !jumping)
                {
                    SetGravityScale(0);
                }
                else if (jumpCut)
                {
                    if (PlayerInput.Crouch && !crouching) SetGravityScale(gravityScale * fastFallMultiplier);
                    else SetGravityScale(gravityScale * apexReachSharpness);
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
                }
                else if ((jumping || wallJumping || jumpFalling) && Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold)
                {
                    if (PlayerInput.Crouch && !crouching) SetGravityScale(gravityScale * fastFallMultiplier);
                    else SetGravityScale(gravityScale * jumpHangGravityMult);
                }
                else if (rb.velocity.y < 0)
                {
                    if (PlayerInput.Crouch && !crouching) SetGravityScale(gravityScale * fastFallMultiplier);
                    else SetGravityScale(gravityScale * fallGravityMult);
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
                }
                else
                {
                    SetGravityScale(gravityScale);
                }
            }
            else
            {
                SetGravityScale(0);
            }
        }
        #region INPUT CALLBACKS
        //Methods which whandle input detected in Update()
        public void OnJumpInput()
        {
            LastPressedJumpTime = jumpInputBufferTime;
        }

        public void OnJumpUpInput()
        {
            if (CanJumpCut() || CanWallJumpCut())
                jumpCut = true;
        }
        #endregion

        #region GENERAL METHODS
        public void SetGravityScale(float scale)
        {
            rb.gravityScale = scale;
        }
        #endregion

        //MOVEMENT METHODS
        #region RUN METHODS

        public void HandleVelocities()
        {
            currentSpeed = autoRun ? runSpeed : (PlayerInput.Run && (canRun || !usesStamina) ? runSpeed : walkSpeed);
            currentSpeed *= PlayerMultipliers.Instance.speedModifier;

            currentSpeed *= weaponController == null || weaponController.weapon == null ? 1 : 1 / weaponController.weapon.weight;
        }

        public void LadderVelocity()
        {
            currentSpeed = horizontalLadderSpeed * PlayerMultipliers.Instance.speedModifier;
        }
        private float GetSpeedModifier()
        {
            return step[currentLayer].speedModifier;
        }
        public void Movement(float lerpAmount)
        {
            // Determine the actual speed at which the player should move
            float targetSpeed = PlayerInput.HorizontalMovement * currentSpeed * GetSpeedModifier();

            // Smoothly lerp the velocity for smoother transitions and movement
            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

            // Calculate capacity of gaining speed
            float accelRate = (LastOnGroundTime > 0) ? (Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration : runDecceleration) : (Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration * accelInAir : runDecceleration * deccelInAir);

            // Acceleration and speed change while airborne for more satisfying movement styles
            if ((jumping || wallJumping || jumpFalling) && Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold && !crouching && !gliding)
            {
                accelRate *= jumpHangAccelerationMult;
                targetSpeed *= jumpHangMaxSpeedMult;
            }

            if (doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
                accelRate = 0;

            float speedDif = targetSpeed - rb.velocity.x;
            float movement = speedDif * accelRate;

            // Apply the forces
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
        }

        // Used for ladders, mainly
        // It allows moving vertically using W & S ( for the default provided inputs by 2D Platformer Engine )
        public void VerticalMovement()
        {
            float movement = PlayerInput.VerticalMovement * GetSpeedModifier();
            rb.velocity = movement * verticalLadderSpeed * Vector2.up * PlayerMultipliers.Instance.speedModifier;
        }

        // Orientate the player wether right, or left side.
        private void Turn()
        {
            graphics.localScale = new Vector3(-graphics.localScale.x, graphics.localScale.y, graphics.localScale.z);
            facingRight = !facingRight;
        }

        public void StartCrouch()
        {
            // Set everything necessary at start crouch.
            // this is only called the movement the player starts crouching
            crouching = true;

            if (currentSpeed > walkSpeed) stamina -= staminaLossOnCrouchSlide;

            currentSpeed = crouchSpeed;

            events.onStartCrouch?.Invoke();
        }
        public void StopCrouch()
        {
            // Set everything necessary at stoop crouch.
            // this is only called the movement the player stops crouching
            crouching = false;
            transform.localScale = Vector3.one;

            events.onStopCrouch?.Invoke();
        }
        public void CrouchSlide()
        {
            // Apply a force in the direction of movement that simulates a slide.
            rb.AddForce(transform.right * PlayerInput.HorizontalMovement * crouchSlideSpeed * 1000 * Time.deltaTime);
        }
        #endregion

        #region JUMP METHODS
        public void Jump()
        {
            // Reset timers
            LastPressedJumpTime = 0;
            LastOnGroundTime = 0;

            // Calculate and apply forces
            float force = jumpForce * PlayerMultipliers.Instance.jumpHeightModifier - Mathf.Max(rb.velocity.y, 0);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

            // Remove Stamina
            stamina -= staminaLossOnJump;

            onJump?.Invoke();

            // Play SFX
            SoundManager.Instance.PlaySound(sounds.jumpSFX, 1);

            events.onJump?.Invoke();
        }
        public void JumpFall()
        {
            // Reset variables
            jumping = false;
            jumpFalling = true;
        }

        private void WallJump(int dir)
        {
            // Reset timers
            LastPressedJumpTime = 0;
            LastOnGroundTime = 0;
            WallRight = false;
            WallLeft = false;

            // Calculate force to apply on wall jumping
            Vector2 force = new Vector2(wallJumpForce.x * dir, wallJumpForce.y);

            if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x))
                force.x -= rb.velocity.x;

            force.y -= Mathf.Max(rb.velocity.y, 0);

            // Apply force
            rb.AddForce(force, ForceMode2D.Impulse);

            // Reduce stamina
            stamina -= staminaLossOnWallJump;

            // Play sounds and events
            SoundManager.Instance.PlaySound(sounds.wallJumpSFX, 1);
            onJump?.Invoke();

            events.onWallJump?.Invoke();
        }
        #endregion
        #region OTHER MOVEMENT METHODS

        private Vector3 wallSlideSide;

        public Vector3 WallSlideSlide
        {
            get
            {
                return wallSlideSide;
            }
        }

        public void Slide()
        {
            // Remove the remaining upwards velocity to prevent upwards sliding
            if (rb.velocity.y > 0)
            {
                rb.AddForce(-rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
            }

            rb.velocity += new Vector2(0, -wallSlideSpeed);

            // Handles VFX
            wallSlideVFXTimer -= Time.deltaTime;

            if (wallSlideVFXTimer > 0) return;

            wallSlideVFXTimer = wallSlideVFXInterval;

            wallSlideSide = facingRight ? leftWallCheckOffset : rightWallCheckOffset;
            Instantiate(wallSlideVFX, transform.position + wallSlideSide, Quaternion.identity);
        }

        public void StartGlide()
        {
            // Only called once when the player starts to glide
            gliding = true;
            // Sets the speed
            currentSpeed = glideSpeed;
            // Stops the player for one frame to avoid weird movement afterwards
            rb.velocity = Vector2.zero;

            events.onStartGlide?.Invoke();
        }

        public void StopGlide()
        {
            // Only called once when the player stops gliding
            gliding = false;

            events.onStopGliding?.Invoke();
        }

        public void GlideVerticalMovement()
        {
            // this acts as lower gravity when gliding
            rb.velocity += new Vector2(0, -glideGravity) * Time.deltaTime;
        }

        public void HandleFootsteps()
        {
            // If the player is idle, no footstep should be played, but the timer must be reset
            if (PlayerInput.HorizontalMovement == 0 || rb.velocity.magnitude < .1f)
            {
                footstepsTimer = footstepsInterval;
                return;
            }

            // If the player is moving calculate an interval within footsteps
            footstepsTimer -= Time.deltaTime;

            // Are we able to play a footstep?
            if (LastOnGroundTime < 0 || footstepsTimer > 0) return;

            // Reset timer
            footstepsTimer = footstepsInterval;

            // Play sounds and Effects
            SoundManager.Instance.PlaySound(step[currentLayer].stepSFX, footstepsVolume);

            Instantiate(step[currentLayer].stepVFX, transform.position + groundCheckOffset, Quaternion.identity);
        }

        private void Land()
        {
            // Play sounds and spawn VFX when  the player lands on a surface
            SoundManager.Instance.PlaySound(step[currentLayer].landSFX, landVolume);

            Instantiate(step[currentLayer].landVFX, transform.position + groundCheckOffset, Quaternion.identity);

            onLand?.Invoke();

            events.onLand?.Invoke();

            CameraShake.Instance.Shake(landCameraShake, 5, 1, 1);
        }
        public bool CanDash()
        {
            // Returns true if the player can Dash
            return canDash && PlayerInput.Dash && dashMethod != DashMethod.None && currentDashes > 0 && (!gliding || gliding && canDashWhileGliding);
        }

        public void InitializeDash()
        {
            // Only called once when the player starts to dash
            // make sure this only gets called once
            CancelInvoke(nameof(ResetDash));
            canDash = false;
            dashing = true;
            dashDirection = GetDashDirection();
            Invoke(nameof(ResetDash), dashDuration);
            UIController.instance.RemoveDash();
            currentDashes--;

            events.onStartDash?.Invoke();
        }

        public void PerformDash()
        {
            // Called everyframe the dash is being performed
            rb.velocity = new Vector2(dashDirection.x, dashDirection.y) * dashSpeed;
        }

        private Vector2 GetDashDirection()
        {
            // Determine the dash direction based on the user variables.
            if (dashMethod == DashMethod.Default)
            {
                if (PlayerInput.HorizontalMovement == 0) return transform.right;
                else return new Vector2(PlayerInput.HorizontalMovement, 0).normalized;
            }
            else if (dashMethod == DashMethod.AimBased)
            {
                Vector3 crosshairPosition = Crosshair.Instance.transform.position;
                return (crosshairPosition - transform.position).normalized;
            }
            else if (dashMethod == DashMethod.HorizontalAimBased)
            {
                Vector3 crosshairPosition = Crosshair.Instance.transform.position;
                Vector3 dir = (crosshairPosition - transform.position);

                if (dir.x >= 0) return new Vector2(1, 0);
                else return new Vector2(-1, 0);
            }
            return Vector2.zero;
        }

        private void ResetDash()
        {
            // Enable dash and cooldown for the dash
            dashing = false;
            rb.gravityScale = gravityScale;
            rb.velocity = Vector3.zero;
            Invoke(nameof(EnableDash), dashInterval);
            Invoke(nameof(CoolDash), dashCooldown);
        }

        private void EnableDash() => canDash = true;

        private void CoolDash()
        {
            currentDashes++; UIController.instance.GainDash();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Check if the player is inside the range of a ladder
            if (!other.CompareTag("Ladder")) return;

            if (jumping) ladder = false;
            else ladder = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Check if the player is outside the range of a ladder
            if (!other.CompareTag("Ladder")) return;

            ladder = false;
        }
        #endregion


        #region CHECK METHODS
        // Handle turning
        public void SetOrientation(bool isMovingRight)
        {
            if (isMovingRight != facingRight)
                Turn();
        }

        private bool CanJump()
        {
            return (LastOnGroundTime > 0 || amountOfJumps > 0) && (!usesStamina || usesStamina && staminaAllowsJump);
        }

        private bool CanWallJump()
        {
            return (LastPressedJumpTime > 0 && LastOnWallTime && LastOnGroundTime <= 0 &&
                (!wallJumping || (WallRight && _lastWallJumpDir == 1)
                || (WallLeft && _lastWallJumpDir == -1)))
                && (!usesStamina || usesStamina && staminaAllowsJump);
        }

        private bool CanJumpCut()
        {
            return jumping && rb.velocity.y > 0;
        }

        private bool CanWallJumpCut()
        {
            return wallJumping && rb.velocity.y > 0;
        }

        public bool CanSlide()
        {
            if (LastOnWallTime && !jumping && !wallJumping && LastOnGroundTime <= 0)
                return true;
            else
                return false;
        }
        #endregion
        #region Stamina
        private void Stamina()
        {
            // Check if we def wanna use stamina
            if (!usesStamina || stats.isDead || !PlayerStats.controllable) return;

            float oldStamina = stamina; // Store stamina before we change its value

            // We ran out of stamina
            if (stamina <= 0)
            {
                canRun = false;
                staminaAllowsJump = false;
                stamina = 0;
            }

            // Wait for stamina to regenerate up to the min value allowed to start running and jumping again
            if (stamina >= minStaminaRequiredToRun)
            {
                canRun = true; staminaAllowsJump = true;
            }

            // Regen stamina
            if (stamina < maxStamina)
            {
                if (currentSpeed <= walkSpeed)
                    stamina += Time.deltaTime * staminaRegenMultiplier;
            }

            // Lose stamina
            if (currentSpeed == runSpeed && canRun && !wallSliding && InputManager.playerInputs.HorizontalMovement != 0) stamina -= Time.deltaTime;


            if (oldStamina != stamina)
                UIController.instance.EnableStaminaSlider(true);
            else
                UIController.instance.EnableStaminaSlider(false);


        }


        void ResetStamina() => stamina = maxStamina;

        #endregion
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Step"))
            {
                ContactPoint2D contact = collision.contacts[0];

                // Check if collision is happening from the sides
                if (contact.normal.y < 0.5f)
                {
                    // Move the player up by the step height
                    transform.position += Vector3.up * stepHeight;
                }
            }

        }

        public bool IsGrounded()
        {
            return LastOnGroundTime > 0;
        }

        public bool IsFalling()
        {
            return LastOnGroundTime <= 0 && rb.velocity.y <= 0;
        }

        private void GatherEvents()
        {
            switch (playerOrientationMethod)
            {
                case PlayerOrientationMethod.None: orientatePlayer = null; break;
                case PlayerOrientationMethod.HorizontalInput: orientatePlayer = HorizontalOrientation; break;
                case PlayerOrientationMethod.AimBased: orientatePlayer = AimBasedOrientation; break;
                case PlayerOrientationMethod.Mixed: orientatePlayer = MixedOrientation; break;
            }
        }
        #region EDITOR METHODS
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + groundCheckOffset, groundCheckSize);
            Gizmos.DrawWireCube(transform.position + ceilingCheckOffset, ceilingCheckSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + leftWallCheckOffset, _wallCheckSize);
            Gizmos.DrawWireCube(transform.position + rightWallCheckOffset, _wallCheckSize);
        }
        #endregion
    }

}