namespace cowsins2D
{
    public class PlayerStateFactory
    {
        PlayerStates _context;

        public PlayerStateFactory(PlayerStates currentContext) { _context = currentContext; }

        public PlayerBaseState Default() { return new PlayerDefaultState(_context, this); }

        public PlayerCrouchState Crouch() { return new PlayerCrouchState(_context, this); }

        public PlayerBaseState Jump() { return new PlayerJumpState(_context, this); }

        public PlayerBaseState WallSlide() { return new PlayerWallSlideState(_context, this); }

        public PlayerBaseState WallJump() { return new PlayerWallJumpState(_context, this); }

        public PlayerBaseState Dash() { return new PlayerDashState(_context, this); }

        public PlayerBaseState Die() { return new PlayerDieState(_context, this); }

        public PlayerBaseState Ladder() { return new PlayerLadderState(_context, this); }

        public PlayerBaseState Glide() { return new PlayerGlideState(_context, this); }


    }

}