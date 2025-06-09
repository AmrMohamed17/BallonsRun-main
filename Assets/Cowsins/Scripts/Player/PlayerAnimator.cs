using System;
using UnityEngine;

namespace cowsins2D
{

    public class PlayerAnimator : MonoBehaviour
    {
        private PlayerMovement player;

        [SerializeField] private Animator animator;

        private void Start()
        {
            // Initial settings
            player = GetComponent<PlayerMovement>();

            animator.SetTrigger("Idle");
        }

        private void Update()
        {
            // Set animations for the ladder state
            if (player.ladder)
            {
                animator.SetTrigger("LadderIdle");
                return;
            }
            if (player.IsFalling() && !player.wallSliding && !player.gliding && PlayerStats.controllable)
            {
                JumpAnimations();
                return;
            }
            if (player.jumping && !player.IsFalling())
            {
                animator.SetTrigger("Idle");
                return;
            }
            // Set animations for the crouching state
            if (player.crouching) CrouchAnimations();
            // Set animations for the gliding state
            else if (player.gliding) GlidingAnimations();
            else if (player.wallSliding) WallSlidingAnimations();
            // Set animations for the default state
            else if (player.IsGrounded()) DefaultAnimations();
        }

        private void DefaultAnimations()
        {
            // Idle anim
            if (player.rb.velocity.magnitude < .1f)
            {
                animator.SetTrigger("Idle");
                return;
            }
            // Walk Anim
            if (player.currentSpeed <= player.walkSpeed && player.rb.velocity.magnitude > .5f) animator.SetTrigger("Walk");
            else if (player.currentSpeed > player.walkSpeed) animator.SetTrigger("Run"); // Run anim 
        }

        public void LadderAnimations()
        {
            // Idle on ladder
            if (player.rb.velocity.magnitude < .1f)
            {
                animator.SetTrigger("LadderIdle");
                return;
            }

            // Moving on ladder
            animator.SetTrigger("LadderMove");
        }

        private void JumpAnimations()
        {
            // Jump
            animator.SetTrigger("Jump");
        }

        private void GlidingAnimations()
        {
            // Gliding
            animator.SetTrigger("Glide");
        }

        private void WallSlidingAnimations()
        {
            // Wall Sliding
            animator.SetTrigger("Slide");
        }


        private void CrouchAnimations()
        {
            // Idle on Crouch
            if (player.rb.velocity.magnitude < .1f)
            {
                animator.SetTrigger("Crouch");
                return;
            }

            // Moving while crouched
            animator.SetTrigger("WalkCrouch");
        }
    }

}