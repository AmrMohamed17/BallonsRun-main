using UnityEngine;
namespace cowsins2D
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            player.Jump();
            player.ReduceJumpAmount();
            player.transform.up = Vector3.up;
        }

        public override void UpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.orientatePlayer?.Invoke();
            player.HandleJumpCutInput();
            player.HandleVelocities();
            CheckSwitchState();
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

            if (player.CheckIfPerformJump()) SwitchState(_factory.Jump());

            if (player.CanDash()) SwitchState(_factory.Dash());

            if (stats.health <= 0) SwitchState(_factory.Die());
        }

        public override void InitializeSubState() { }

    }

}