using UnityEngine;
namespace cowsins2D
{
    public class PlayerDieState : PlayerBaseState
    {
        public PlayerDieState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        PlayerMovement player;
        PlayerStats stats;
        Rigidbody2D rb;
        public override void EnterState()
        {
            player = _ctx.GetComponent<PlayerMovement>();
            stats = _ctx.GetComponent<PlayerStats>();
            rb = _ctx.GetComponent<Rigidbody2D>();

            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        public override void UpdateState()
        {
        }

        public override void FixedUpdateState()
        {
        }

        public override void ExitState() { rb.isKinematic = false; }

        public override void CheckSwitchState()
        {
        }

        public override void InitializeSubState() { }

    }

}