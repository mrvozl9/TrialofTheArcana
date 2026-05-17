using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Controls player movement, combat mechanics, and health system.
/// Handles jumping, dashing, pogo mechanics, wall holding, invincibility, and respawn logic.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // UI event for health
    public static Action<int, int> OnHealthChanged;

    #region Serialized Fields

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float minJumpForce = 9f;
    [SerializeField] private float maxJumpForce = 15f;
    [SerializeField] private float jumpHoldTime = 0.3f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float gravityScale = 4.5f;
    [SerializeField] private float floatGravity = 0.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.4f;

    [Header("Dash Cooldown")]
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 3f;
    [SerializeField] private float invincibilityCooldown = 5f;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Hearts & Respawn")]
    [SerializeField] private int maxHearts = 5;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Pogo Settings")]
    [SerializeField] private float pogoForce = 12f;
    [SerializeField] private LayerMask pogoLayer;
    [SerializeField] private float pogoCheckDistance = 1.0f;
    [SerializeField] private float pogoBufferTime = 0.1f;
    [SerializeField] private float pogoCooldown = 0.05f;
    [SerializeField] private float pogoSideOffset = 0.3f;

    [Header("Wall Hold Settings")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallSlideSpeed = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip keyCollectSoundClip;
    [SerializeField] private AudioClip checkpointSoundClip;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    #endregion

    #region Private Fields

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource oneShotAudioSource;
    private PlayerAudioController audioController;

    // State
    private int currentHearts;
    private Vector2 respawnPoint;
    private Vector2 lastSafePosition;
    private Vector3 startPoint;

    // Jump state
    private bool isGrounded;
    private bool isJumping;
    private bool jumpButtonHeld;
    private float jumpStartTime;
    private bool canFloat;
    private bool canInvincibility = false;

    // Dash state
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    public bool IsDashing => isDashing;

    // Invincibility state
    private bool isInvincible;
    private float invincibilityCooldownTimer;
    private Coroutine invincibilityCoroutine;

    // Respawn freeze state
    private bool isRespawnFrozen;
    private float respawnFreezeTimer;

    // Ability unlocks
    private bool canDash = false;
    private bool canFloatSkill = false;

    // Pogo state
    private bool canPogo = true;
    private float pogoBufferCounter;

    // Wall state
    private bool isTouchingWall;
    private bool isWallHolding;
    private bool canWallHold;

    // Key system
    public bool hasKey;

    // Cached values
    private float originalGravityScale;
    private const float MinVelocityForFloat = -0.1f;
    private const float MaxPogoVelocity = 0.2f;
    private const float GroundDetectionDistance = 5f;
    private const float RespawnFreezeDuration = 0.8f;

    #endregion

    #region Properties

    public bool HasKey => hasKey;
    public int CurrentHearts => currentHearts;
    public int MaxHearts => maxHearts;
    public bool IsWallHoldUnlocked => canWallHold;
    public bool IsInvincible => isInvincible;

    public bool CanInvincibility => canInvincibility;

    public Vector2 CurrentVelocity => rb.velocity;
    public bool IsJumping => isJumping;
    public bool IsFloating => rb.gravityScale < originalGravityScale;

    // Properties for tutorial access
    public bool IsGrounded => isGrounded;
    public bool CanDash => canDash;
    public bool CanFloatSkill => canFloatSkill;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        InitializeState();
    }

    private void Update()
    {
        if (isRespawnFrozen)
        {
            UpdateRespawnFreeze();
            return; // Skip all other input and movement
        }

        if (isDashing)
        {
            UpdateDash();
            return;
        }

        UpdateTimers();
        CheckGroundStatus();
        HandleJumpInput();
        ApplyBetterJumpPhysics();
        HandleFloatMechanic();
        HandleDashInput();
        HandleInvincibilityInput();
        HandlePogoInput();
        HandleWallHold();
        DetectLastSafePosition();
    }

    private void FixedUpdate()
    {
        if (isRespawnFrozen)
        {
            rb.velocity = Vector2.zero; // Keep player stationary
            return;
        }

        if (!isDashing)
        {
            ApplyHorizontalMovement();
        }

        TryExecutePogoBounce();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other);
    }

    private void OnDrawGizmosSelected()
    {
        DrawDebugGizmos();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        oneShotAudioSource = GetComponent<AudioSource>();
        audioController = GetComponent<PlayerAudioController>();

        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component!");
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning("No SpriteRenderer found - invincibility visual feedback won't work!");
        }

        if (oneShotAudioSource == null)
        {
            Debug.LogWarning("PlayerController: AudioSource bileşeni bulunamadı. Sesler çalınamayacak.");
        }

        if (audioController == null)
        {
            Debug.LogWarning("PlayerController: PlayerAudioController bulunamadı. Hareket sesleri çalınmayacak.");
        }
    }

    private void InitializeState()
    {
        originalGravityScale = gravityScale;
        rb.gravityScale = gravityScale;

        currentHearts = maxHearts;
        startPoint = transform.position;
        respawnPoint = transform.position;
        lastSafePosition = transform.position;

        canPogo = true;
        canFloat = true;
        hasKey = false;
        canWallHold = false;
        jumpButtonHeld = false;
        dashCooldownTimer = 0f;
        isInvincible = false;
        invincibilityCooldownTimer = 0f;
        isRespawnFrozen = false;
        respawnFreezeTimer = 0f;
        canInvincibility = false;

        OnHealthChanged?.Invoke(currentHearts, maxHearts);
    }

    #endregion

    #region Update Methods

    private void UpdateTimers()
    {
        if (pogoBufferCounter > 0)
        {
            pogoBufferCounter -= Time.deltaTime;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (invincibilityCooldownTimer > 0)
        {
            invincibilityCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateDash()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0)
        {
            isDashing = false;
        }
    }

    private void UpdateRespawnFreeze()
    {
        respawnFreezeTimer -= Time.deltaTime;

        if (respawnFreezeTimer <= 0)
        {
            isRespawnFrozen = false;
        }
    }

    private void CheckGroundStatus()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("Ground check transform is not assigned!");
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canPogo = true;
            canFloat = true;
        }
    }

    #endregion

    #region Input Handling

    private void HandleJumpInput()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Z);
        bool jumpHeld = Input.GetKey(KeyCode.Z);
        bool jumpReleased = Input.GetKeyUp(KeyCode.Z);

        jumpButtonHeld = jumpHeld;

        if (jumpPressed && isGrounded)
        {
            StartJump();
        }

        if (jumpReleased && isJumping && rb.velocity.y > 0)
        {
            CutJumpShort();
        }
    }

    private void HandleFloatMechanic()
    {
        if (!canFloatSkill) return;

        bool floatKeyHeld = Input.GetKey(KeyCode.Z);
        bool isFalling = rb.velocity.y < MinVelocityForFloat;

        // Allow floating when falling, regardless of jump state
        if (floatKeyHeld && !isGrounded && canFloat && isFalling)
        {
            rb.gravityScale = floatGravity;
        }
        else
        {
            rb.gravityScale = originalGravityScale;
        }
    }

    private void HandleDashInput()
    {
        if (!canDash) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            TryStartDash(moveInput);
        }
    }

    private void HandleInvincibilityInput()
    {
        if (!canInvincibility) return; // Add this check at the start

        if (Input.GetKeyDown(KeyCode.S))
        {
            TryActivateInvincibility();
        }
    }

    private void HandlePogoInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            pogoBufferCounter = pogoBufferTime;
        }
    }

    #endregion

    #region Movement Methods

    private void ApplyHorizontalMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        rb.velocity = new Vector2(rb.velocity.x, minJumpForce);
    }

    private void ApplyBetterJumpPhysics()
    {
        if (isJumping && jumpButtonHeld)
        {
            float holdDuration = Time.time - jumpStartTime;

            if (holdDuration < jumpHoldTime && rb.velocity.y > 0)
            {
                float holdRatio = holdDuration / jumpHoldTime;
                float additionalForce = Mathf.Lerp(0, maxJumpForce - minJumpForce, holdRatio);
                float forceToApply = (additionalForce / jumpHoldTime) * Time.deltaTime;
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + forceToApply);
            }
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            isJumping = false;
        }
        else if (rb.velocity.y > 0 && !jumpButtonHeld)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        if (isGrounded && rb.velocity.y <= 0)
        {
            isJumping = false;
        }
    }

    private void CutJumpShort()
    {
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
    }

    private void TryStartDash(float direction)
    {
        if (Mathf.Approximately(direction, 0f)) return;
        if (dashCooldownTimer > 0) return;

        isDashing = true;
        dashTimer = dashTime;
        rb.velocity = new Vector2(direction * dashSpeed, 0f);

        dashCooldownTimer = dashCooldown;
    }

    public void ApplyExternalBounce(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.velocity += Vector2.up * force;

        canFloat = true;
        isJumping = false;
        dashCooldownTimer = 0f;
    }

    #endregion

    #region Invincibility Mechanics

    private void TryActivateInvincibility()
    {
        if (isInvincible)
        {
            if (showDebugInfo)
            {
                Debug.Log("Invincibility is already active!");
            }
            return;
        }

        if (invincibilityCooldownTimer > 0)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Invincibility on cooldown! {invincibilityCooldownTimer:F1}s remaining");
            }
            return;
        }

        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        invincibilityCooldownTimer = invincibilityCooldown;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("Invincibility activated!");
        }

        if (showDebugInfo)
        {
            Debug.Log("Invincibility activated for " + invincibilityDuration + " seconds!");
        }

        // Visual feedback: blink the sprite
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibilityDuration)
        {
            elapsed += blinkInterval;
            visible = !visible;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = visible ? 1f : 0.3f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(blinkInterval);
        }

        // Restore full visibility
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isInvincible = false;

        if (showDebugInfo)
        {
            Debug.Log("Invincibility ended!");
        }
    }

    #endregion

    #region Pogo Mechanics

    private void TryExecutePogoBounce()
    {
        if (pogoBufferCounter > 0 && !isDashing)
        {
            if (ExecutePogoBounce())
            {
                pogoBufferCounter = 0;
            }
        }
    }

    private bool ExecutePogoBounce()
    {
        if (!canPogo || rb.velocity.y > MaxPogoVelocity) return false;

        RaycastHit2D hit = GetPogoRaycastHit();

        if (hit.collider != null)
        {
            ApplyPogoBounce(hit);
            return true;
        }

        return false;
    }

    private RaycastHit2D GetPogoRaycastHit()
    {
        Vector2 origin = transform.position;

        RaycastHit2D hitCenter = Physics2D.Raycast(origin, Vector2.down, pogoCheckDistance, pogoLayer);
        if (hitCenter.collider != null) return hitCenter;

        RaycastHit2D hitLeft = Physics2D.Raycast(origin + Vector2.left * pogoSideOffset, Vector2.down, pogoCheckDistance * 0.9f, pogoLayer);
        if (hitLeft.collider != null) return hitLeft;

        RaycastHit2D hitRight = Physics2D.Raycast(origin + Vector2.right * pogoSideOffset, Vector2.down, pogoCheckDistance * 0.9f, pogoLayer);
        return hitRight;
    }

    private void ApplyPogoBounce(RaycastHit2D hit)
    {
        rb.velocity = new Vector2(rb.velocity.x, pogoForce);
        canFloat = true;

        if (audioController != null)
        {
            audioController.PlayPogoSound();
        }

        VanishPlatform vanishPlatform = hit.collider.GetComponent<VanishPlatform>();
        if (vanishPlatform != null)
        {
            vanishPlatform.StartVanishSequence();
        }

        StartCoroutine(PogoCooldownRoutine());
    }

    private IEnumerator PogoCooldownRoutine()
    {
        canPogo = false;
        yield return new WaitForSeconds(pogoCooldown);
        canPogo = true;
    }

    #endregion

    #region Wall Hold Mechanics

    private void HandleWallHold()
    {
        if (!canWallHold || wallCheck == null)
        {
            isWallHolding = false;
            return;
        }

        CheckWallContact();
        ApplyWallSlide();
    }

    private void CheckWallContact()
    {
        Vector2 origin = wallCheck.position;

        bool touchingRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, wallLayer);
        bool touchingLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, wallLayer);

        isTouchingWall = touchingRight || touchingLeft;
    }

    private void ApplyWallSlide()
    {
        bool shouldSlide = isTouchingWall && !isGrounded && rb.velocity.y < 0;

        if (shouldSlide)
        {
            isWallHolding = true;
            float clampedVelocity = Mathf.Max(rb.velocity.y, -wallSlideSpeed);
            rb.velocity = new Vector2(rb.velocity.x, clampedVelocity);
        }
        else
        {
            isWallHolding = false;
        }
    }

    #endregion

    #region Position Tracking

    private void DetectLastSafePosition()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GroundDetectionDistance, groundLayer);

        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            lastSafePosition = transform.position;
        }
    }

    #endregion

    #region Collision Handling

    private void HandleCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("CheckPoint"))
        {
            SetRespawnPoint(collision.transform.position);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Only take damage if not invincible
            if (!isInvincible)
            {
                TakeDamage();
            }
            else if (showDebugInfo)
            {
                Debug.Log("Damage blocked by invincibility!");
            }
        }

        BouncyMushroom mushroom = collision.gameObject.GetComponent<BouncyMushroom>();
        if (mushroom != null)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    ApplyExternalBounce(mushroom.GetBounceForce());
                    return;
                }
            }
        }
    }

    private void HandleTrigger(Collider2D other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            SetRespawnPoint(other.transform.position);
        }
        else if (other.CompareTag("Key"))
        {
            CollectKey(other.gameObject);
        }
    }

    #endregion

    #region Health and Respawn

    private void TakeDamage()
    {
        currentHearts--;
        OnHealthChanged?.Invoke(currentHearts, maxHearts);

        if (showDebugInfo)
        {
            Debug.Log($"Player took damage! Hearts remaining: {currentHearts}");
        }

        if (currentHearts > 0)
        {
            RespawnAtLastSafePosition();
        }
        else
        {
            // Player is out of hearts - trigger death animation
            Die();
        }
    }

    private void RespawnAtLastSafePosition()
    {
        transform.position = lastSafePosition;
        ResetVelocity();
        StartRespawnFreeze();
    }

    private void RespawnAtCheckpoint()
    {
        currentHearts = maxHearts;
        transform.position = respawnPoint;
        ResetVelocity();
        StartRespawnFreeze();

        OnHealthChanged?.Invoke(currentHearts, maxHearts);
    }

    private void ResetVelocity()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void StartRespawnFreeze()
    {
        isRespawnFrozen = true;
        respawnFreezeTimer = RespawnFreezeDuration;

        if (showDebugInfo)
        {
            Debug.Log($"Player frozen for {RespawnFreezeDuration} seconds after respawn");
        }
    }

    private void SetRespawnPoint(Vector2 position)
    {
        respawnPoint = position;

        // Heal player to full health
        currentHearts = maxHearts;
        OnHealthChanged?.Invoke(currentHearts, maxHearts);

        // Checkpoint UI mesajını göster
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("Checkpoint reached!");
        }

        // Checkpoint sesini çal
        if (oneShotAudioSource != null && checkpointSoundClip != null)
        {
            oneShotAudioSource.PlayOneShot(checkpointSoundClip);
        }
        else if (checkpointSoundClip == null)
        {
            Debug.LogWarning("Checkpoint sesi çalınamadı. Checkpoint Sound Clip atanmamış.");
        }

        if (showDebugInfo)
        {
            Debug.Log($"Respawn point set to: {position}. Health restored to {maxHearts}");
        }
    }

    public void Heal(int amount)
    {
        currentHearts = Mathf.Min(currentHearts + amount, maxHearts);
        OnHealthChanged?.Invoke(currentHearts, maxHearts);
    }

    #endregion

    #region Death

    public void Die()
    {
        if (showDebugInfo)
        {
            Debug.Log("Player died!");
        }

        // Trigger death animation
        PlayerAnimationController animController = GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            animController.TriggerDeath();
        }

        // Disable player controls
        enabled = false;

        // Stop all movement
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // Start respawn after animation
        StartCoroutine(RespawnAfterDeath());
    }

    private IEnumerator RespawnAfterDeath()
    {
        // Wait for death animation to finish (adjust time to match your animation length)
        yield return new WaitForSeconds(0.7f);

        // Reset player
        currentHearts = maxHearts;
        transform.position = respawnPoint;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;

        // Re-enable player
        PlayerAnimationController animController = GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            animController.ResetDeath();
        }

        enabled = true;

        OnHealthChanged?.Invoke(currentHearts, maxHearts);

        StartRespawnFreeze();
    }

    #endregion

    #region Item Collection

    private void CollectKey(GameObject keyObject)
    {
        hasKey = true;
        Destroy(keyObject);

        // Anahtar toplama sesini çal
        if (oneShotAudioSource != null && keyCollectSoundClip != null)
        {
            oneShotAudioSource.PlayOneShot(keyCollectSoundClip);
        }
        else
        {
            Debug.LogWarning("Anahtar sesi çalınamadı. AudioSource yok veya Key Collect Clip atanmamış.");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("You picked up a key!");
        }

        if (showDebugInfo)
        {
            Debug.Log("Player collected a key!");
        }
    }

    #endregion

    #region Ability Unlocks

    public void UnlockDash()
    {
        canDash = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Dash ability unlocked!");

        if (showDebugInfo)
            Debug.Log("Dash ability unlocked!");
    }

    public void UnlockFloat()
    {
        canFloatSkill = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Float ability unlocked!");

        if (showDebugInfo)
            Debug.Log("Float ability unlocked!");
    }

    public void UnlockWallHold()
    {
        canWallHold = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Wall Hold ability unlocked!");
        if (showDebugInfo)
        {
            Debug.Log("Wall Hold ability unlocked!");
        }
    }

    public void UnlockInvincibility()
    {
        canInvincibility = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Shield ability unlocked!");

        if (showDebugInfo)
            Debug.Log("Shield (Invincibility) ability unlocked!");
    }

    #endregion

    #region Public Methods

    public void ResetToStart()
    {
        transform.position = startPoint;
        currentHearts = maxHearts;
        ResetVelocity();
    }

    #endregion

    #region Debug

    private void DrawDebugGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.red;
        Vector3 position = Application.isPlaying ? transform.position : transform.position;
        Gizmos.DrawLine(position, position + Vector3.down * pogoCheckDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position + Vector3.left * pogoSideOffset,
                        position + Vector3.left * pogoSideOffset + Vector3.down * pogoCheckDistance * 0.9f);
        Gizmos.DrawLine(position + Vector3.right * pogoSideOffset,
                        position + Vector3.right * pogoSideOffset + Vector3.down * pogoCheckDistance * 0.9f);

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheckDistance);
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.left * wallCheckDistance);
        }
    }

    #endregion
}