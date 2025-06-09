using UnityEngine;
namespace cowsins2D
{
    public class PlayerWallSlideState : PlayerBaseState
    {
        public PlayerWallSlideState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            if (player.wallSlidingResetsJumps) player.ResetJumpAmounts();
            if (player.jumping) player.rb.velocity = Vector2.zero;

            player.wallSlideVFXTimer = player.wallSlideVFXInterval;


        }

        public override void UpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.HandleJumpInput();
            player.CheckCollisions(); 
            CheckSwitchState();
            player.HandleVelocities();
        }

        public override void FixedUpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.Movement(player.wallJumping ? player.wallJumpRunLerp : 1);
            player.Slide();
        }

        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (player.jumping && player.rb.velocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }
            if (player.LastOnGroundTime > 0) SwitchState(_factory.Default());

            if (!player.CheckSlideStatus()) SwitchState(_factory.Default());

            if (player.CheckIfPerformWallJump()) SwitchState(_factory.WallJump());

            if (player.CanDash()) SwitchState(_factory.Dash());

            if (stats.health <= 0) SwitchState(_factory.Die());
        }

        public override void InitializeSubState() { }

    }

}