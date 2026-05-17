using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

    // State
    private bool isAttacking = false;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (animator == null || playerController == null) return;
        if (isDead) return;

        // Check if currently in an attack animation
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        isAttacking = stateInfo.IsTag("Attack");

        // ATTACK INPUT (GROUND + AIR / POGO)
        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
        {
            TriggerAttack();
        }

        // Don't override animations while attacking
        if (!isAttacking)
        {
            UpdateAnimationParameters();
            HandleSpriteFlip();
        }
    }

    // ======================
    // ATTACK LOGIC
    // ======================
    private void TriggerAttack()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        // Reset attack triggers
        animator.ResetTrigger("attackRight");
        animator.ResetTrigger("attackUp");
        animator.ResetTrigger("attackDown");

        // AIRBORNE → FORCE DOWN ATTACK (POGO)
        if (!playerController.IsGrounded)
        {
            animator.SetTrigger("attackDown");
            return;
        }

        // GROUNDED ATTACKS
        if (vertical > 0.5f)
        {
            animator.SetTrigger("attackUp");
        }
        else if (vertical < -0.5f)
        {
            animator.SetTrigger("attackDown");
        }
        else
        {
            animator.SetTrigger("attackRight");
        }
    }

    // ======================
    // DEATH
    // ======================
    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("death");

        if (playerController != null)
            playerController.enabled = false;
    }

    public void ResetDeath()
    {
        isDead = false;
        animator.ResetTrigger("death");
        animator.Play("Idle");

        if (playerController != null)
            playerController.enabled = true;
    }

    // ======================
    // ANIMATION PARAMETERS
    // ======================
    private void UpdateAnimationParameters()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.01f);
        animator.SetBool("isGrounded", playerController.IsGrounded);
        animator.SetBool("isJumping", playerController.IsJumping);
        animator.SetBool("isDashing", playerController.IsDashing);
        animator.SetBool("isFloating", playerController.IsFloating);
        animator.SetBool("isWallHolding",
            playerController.IsWallHoldUnlocked && !playerController.IsGrounded);

        animator.SetFloat("velocityY", playerController.CurrentVelocity.y);
    }

    // ======================
    // SPRITE FLIP
    // ======================
    private void HandleSpriteFlip()
    {
        if (isAttacking || isDead) return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;
    }
}
