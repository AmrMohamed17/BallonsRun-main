using UnityEngine;
using UnityEngine.InputSystem;

namespace cowsins2D
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager inputManager;
      
        public static PlayerActions inputActions;

        public static PlayerInputs playerInputs { get; private set; }
  
        private void Awake()
        {
            if (inputManager == null)
            {
                inputManager = this;
            }
            else Destroy(this.gameObject);


            if (inputActions == null)  inputActions = new PlayerActions();
                
            // Initialize player inputs
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.Disable();
        }

        // Update inputs in realtime
        private void Update() => playerInputs = ReceiveInputs();

        private PlayerInputs ReceiveInputs()
        {
            // Returns all the necessary inputs in-game
            return new PlayerInputs
            {
                HorizontalMovement = inputActions.GameControls.Movement.ReadValue<float>(),
                VerticalMovement = -inputActions.GameControls.VerticalMovement.ReadValue<float>(),
                CrouchingDown = inputActions.GameControls.Crouch.WasPressedThisFrame(),
                Crouch = inputActions.GameControls.Crouch.IsPressed(),
                JumpingDown = inputActions.GameControls.Jumping.WasPressedThisFrame(),
                JumpingUp = inputActions.GameControls.Jumping.WasReleasedThisFrame(),
                Jump = inputActions.GameControls.Jumping.IsPressed(),
                Run = inputActions.GameControls.Sprint.IsPressed(),
                Interact = inputActions.GameControls.Interact.IsPressed(),
                OpenInventory = inputActions.GameControls.OpenInventory.WasPressedThisFrame(),
                MousePos = inputActions.GameControls.MousePosition.ReadValue<Vector2>(),
                Shoot = inputActions.GameControls.Shoot.WasPressedThisFrame(),
                ShootHold = inputActions.GameControls.Shoot.IsPressed(),
                Reload = inputActions.GameControls.Reload.IsPressed(),
                Dash = inputActions.GameControls.Dash.WasPressedThisFrame(),
                MouseWheel = inputActions.GameControls.MouseWheel.ReadValue<Vector2>(),
                Drop = inputActions.GameControls.Drop.WasPressedThisFrame(),
                NextWeapon = inputActions.GameControls.NextWeapon.WasPressedThisFrame(),
                PreviousWeapon = inputActions.GameControls.PreviousWeapon.WasPressedThisFrame(),
                pausing = inputActions.GameControls.Pause.WasPressedThisFrame()
            };
        }
    }
}
