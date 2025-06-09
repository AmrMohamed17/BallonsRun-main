using UnityEngine;
namespace cowsins2D
{
    public class PlayerGlideState : PlayerBaseState
    {
        public PlayerGlideState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        Rigidbody2D rb;

        private float timer;

        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            rb = _ctx.GetComponent<Rigidbody2D>();
            player.StartGlide();

            if (player.glideDurationMethod == GlideDurationMethod.None) return;

            timer = player.maximumGlideTime;

        }

        public override void UpdateState()
        {

            if (!PlayerStats.controllable) return;

            player.events.onStartGlide?.Invoke();

            player.GlideVerticalMovement();
            player.CheckCollisions();
            player.HandleJumpInput();
            CheckSwitchState();

            if (player.handleOrientationWhileGliding) player.orientatePlayer?.Invoke();

            if (player.glideDurationMethod == GlideDurationMethod.None) return;

            RunGlideTimer();
        }

        public override void FixedUpdateState()
        {
            if (!PlayerStats.controllable) return;
            player.Movement(1);

        }

        public override void ExitState() { player.StopGlide(); }

        public override void CheckSwitchState()
        {
            if (player.LastOnGroundTime > 0 || !player.PlayerInput.Jump) SwitchState(_factory.Default());

            if (stats.health <= 0) SwitchState(_factory.Die());

            if (player.CanDash()) SwitchState(_factory.Dash());

            if (player.glideDurationMethod == GlideDurationMethod.None) return;

            if (timer <= 0) SwitchState(_factory.Default());

        }

        public override void InitializeSubState() { }


        private void RunGlideTimer() => timer -= Time.deltaTime;

    }

}