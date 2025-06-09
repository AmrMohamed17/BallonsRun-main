using UnityEngine;
namespace cowsins2D
{
    public class PlayerDashState : PlayerBaseState
    {
        public PlayerDashState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            player.InitializeDash();
        }

        public override void UpdateState()
        {
            if (!PlayerStats.controllable) return;
            CheckSwitchState();
            player.PerformDash();
        }

        public override void FixedUpdateState()
        {
        }

        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (player.LastOnGroundTime > 0 && !player.dashing) SwitchState(_factory.Default());

            if (!player.dashing) SwitchState(_factory.Default());

            if (stats.health <= 0) SwitchState(_factory.Die());
        }

        public override void InitializeSubState() { }

    }

}