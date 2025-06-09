using UnityEngine;
namespace cowsins2D
{
    public class PlayerDefaultState : PlayerBaseState
    {
        public PlayerDefaultState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        public override void EnterState()
        {
            // Get Player References
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            // Reset Jumping
            player.jumping = false;
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            // Avoid running the following code if the player is not controllable
            if (!PlayerStats.controllable) return;
            player.HandleVelocities();
            player.CheckIfJumpingOrWallrunning();
            player.orientatePlayer?.Invoke();
            player.HandleJumpInput();
            player.HandleFootsteps();
            CheckSwitchState();
        }

        public override void FixedUpdateState()
        {
            // Avoid running the following code if the player is not controllable
            if (!PlayerStats.controllable) return;
            // Handles the player movement
            player.Movement(player.wallJumping ? player.wallJumpRunLerp : 1);
        }

        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (player.CheckIfPerformJump()) SwitchState(_factory.Jump());
            else if (player.CheckIfPerformWallJump()) SwitchState(_factory.WallJump());

            if (player.CheckSlideStatus()) SwitchState(_factory.WallSlide());

            if (player.PlayerInput.Crouch && player.allowCrouch) SwitchState(_factory.Crouch());

            if (player.CanDash()) SwitchState(_factory.Dash());

            if (stats.health <= 0) SwitchState(_factory.Die());

            if (player.PlayerInput.Jump && player.currentJumps <= 0 && player.LastOnGroundTime <= 0 && player.LastPressedJumpTime > 0 && player.canGlide) SwitchState(_factory.Glide());

            if (player.ladder && player.PlayerInput.VerticalMovement != 0) SwitchState(_factory.Ladder());
        }

        public override void InitializeSubState() { }

    }

}