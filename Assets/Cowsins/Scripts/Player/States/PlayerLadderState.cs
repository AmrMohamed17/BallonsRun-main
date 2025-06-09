using UnityEngine;
namespace cowsins2D
{
    public class PlayerLadderState : PlayerBaseState
    {
        public PlayerLadderState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        PlayerAnimator anim;

        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            anim = _ctx.GetComponent<PlayerAnimator>();

            if (player.gliding) player.StopGlide();

            player.SetGravityScale(0);
        }

        public override void UpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.LadderVelocity();
            CheckSwitchState();
            anim.LadderAnimations();
        }

        public override void FixedUpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.Movement(player.wallJumping ? player.wallJumpRunLerp : 1);
            player.VerticalMovement();
        }

        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (player.LastOnGroundTime > 0) SwitchState(_factory.Default());

            if (!player.ladder) SwitchState(_factory.Default());

            if (player.ladder && player.PlayerInput.VerticalMovement != 0) SwitchState(_factory.Ladder());

            if (stats.health <= 0) SwitchState(_factory.Die());
        }

        public override void InitializeSubState() { }

    }

}