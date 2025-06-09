using UnityEngine; 

namespace cowsins2D
{
    public struct PlayerInputs
    {
        public float HorizontalMovement;
        public float VerticalMovement;
        public bool CrouchingDown; 
        public bool Crouch;
        public bool Jump;
        public bool Gliding;
        public bool JumpingDown;
        public bool JumpingUp;
        public bool Run;
        public bool Dash; 

        public bool Interact;
        public bool OpenInventory; 

        public bool Reload;

        public bool Drop; 

        public bool Shoot;
        public bool ShootHold; 

        public Vector2 MousePos;
        public Vector2 MouseWheel;

        public bool NextWeapon;
        public bool PreviousWeapon;
        public bool pausing; 
    }
}

