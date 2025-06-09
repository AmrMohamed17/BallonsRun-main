using UnityEngine;
namespace cowsins2D
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;

        private bool slide;
        private float slideTimer;

        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            if (player.rb.velocity.magnitude > player.walkSpeed && player.currentSpeed > player.walkSpeed && (player.LastOnGroundTime > 0 || player.LastOnGroundTime <= 0 && player.canCrouchSlideMidAir))
            {
                slide = true;
                slideTimer = player.crouchSlideDuration;
            }
            player.StartCrouch();
        }

        public override void UpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.events.onCrouching?.Invoke();
            player.HandleJumpInput();
            player.CheckCollisions();
            player.orientatePlayer?.Invoke();
            player.HandleJumpCutInput();
            CheckSwitchState();

            if (!slide) return;
            slideTimer -= Time.deltaTime;

            if (!player.PlayerInput.Crouch || slideTimer <= 0) slide = false;
            else player.CrouchSlide();
        }

        public override void FixedUpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.Movement(player.wallJumping ? player.wallJumpRunLerp : 1);
        }

        public override void ExitState() { player.StopCrouch(); }

        public override void CheckSwitchState()
        {
            if (player.CheckIfPerformJump()) SwitchState(_factory.Jump());

            if (player.jumping && player.rb.velocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }

            if (!player.PlayerInput.Crouch && !player.CheckCeiling()) SwitchState(_factory.Default());

            if (player.CanDash()) SwitchState(_factory.Dash());

            if (stats.health <= 0) SwitchState(_factory.Die());

            if (player.CheckSlideStatus()) SwitchState(_factory.WallSlide());
        }

        public override void InitializeSubState() { }

    }

}