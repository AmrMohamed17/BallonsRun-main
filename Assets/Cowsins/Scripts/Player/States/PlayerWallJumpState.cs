using UnityEngine;
namespace cowsins2D
{
    public class PlayerWallJumpState : PlayerBaseState
    {
        public PlayerWallJumpState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            player.ReduceJumpAmount();
            player.SetGravityScale(0);
        }

        public override void UpdateState()
        {

            if (!PlayerStats.controllable) return;

            player.orientatePlayer?.Invoke();
            player.HandleJumpCutInput();
            CheckSwitchState();
            player.HandleVelocities();
        }

        public override void FixedUpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.Movement(player.wallJumping ? player.wallJumpRunLerp : 1);
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

            if (Time.time - player._wallJumpStartTime > player.wallJumpTime)
            {
                player.StopWallJump();
                SwitchState(_factory.Default());
            }

            if (player.CanDash()) SwitchState(_factory.Dash());

            if (stats.health <= 0) SwitchState(_factory.Die());
        }

        public override void InitializeSubState() { }

    }

}