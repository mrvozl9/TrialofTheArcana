using UnityEngine;

/// <summary>
/// Butterfly behavior:
/// - Follows player
/// - Orbits while idle
/// - Lands slowly, one by one
/// - Perches on 5 fixed slots (head + shoulders)
/// - Boosts player soul light when perched
/// </summary>
public class ButterflyFollower : MonoBehaviour
{
    // ================= STATIC PERCH CONTROL =================

    // Only ONE butterfly can land at a time
    private static bool landingInProgress = false;

    // 5 total slots:
    // 0 = Head
    // 1–2 = Left shoulder
    // 3–4 = Right shoulder
    private static bool[] perchSlotsTaken = new bool[5];

    private static readonly Vector3[] perchSlots =
    {
        new Vector3(0f, 1.6f, 0f),        // Head
        new Vector3(-0.35f, 1.3f, 0f),    // Left shoulder (inner)
        new Vector3(-0.55f, 1.25f, 0f),   // Left shoulder (outer)
        new Vector3(0.35f, 1.3f, 0f),     // Right shoulder (inner)
        new Vector3(0.55f, 1.25f, 0f)     // Right shoulder (outer)
    };

    // ================= REFERENCES =================

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    private PlayerController playerController;
    private PlayerSoulLight soulLight;
    private SpriteRenderer spriteRenderer;

    // ================= MOVEMENT =================

    [Header("Movement")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float catchUpSpeed = 5f;
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField] private float idleOrbitRadius = 1.5f;
    [SerializeField] private float idleHeightOffset = 1f;
    [SerializeField] private Vector2 followOffset = new Vector2(-1f, 0.5f);
    [SerializeField] private float randomness = 0.3f;

    // ================= PERCH =================

    [Header("Perching")]
    [SerializeField] private float idleTimeBeforeLanding = 3f;
    [SerializeField] private float landingApproachSpeed = 2f;
    [SerializeField] private float perchBobAmount = 0.05f;
    [SerializeField] private float perchBobSpeed = 2f;

    // ================= INTERNAL =================

    private Vector3 targetPosition;
    private Vector2 randomOffset;
    private float randomTimer;
    private float orbitAngle;
    private float idleTimer;

    private bool playerIsIdle;
    private bool isLanding;
    private bool isPerched;
    private int perchSlotIndex = -1;

    // ================= UNITY =================

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
            soulLight = playerObj.GetComponentInChildren<PlayerSoulLight>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        orbitAngle = Random.Range(0f, 360f);
        UpdateRandomOffset();

        // 🪶 Size-based movement polish
        float scale = transform.localScale.x;
        followSpeed *= Mathf.Lerp(1.3f, 0.8f, scale);
        catchUpSpeed *= Mathf.Lerp(1.3f, 0.8f, scale);
        orbitSpeed *= Mathf.Lerp(1.4f, 0.9f, scale);
    }

    private void Update()
    {
        if (player == null) return;

        CheckPlayerState();

        if (playerIsIdle)
        {
            idleTimer += Time.deltaTime;

            if (!isPerched && !isLanding && idleTimer >= idleTimeBeforeLanding)
                TryBeginLanding();
        }
        else
        {
            ResetPerchState();
        }

        if (isLanding)
            LandingMotion();
        else if (isPerched)
            PerchMotion();
        else
        {
            CalculateOrbitTarget();
            Move(followSpeed);
        }

        UpdateRandomOffset();
        FlipSprite();
    }

    // ================= PLAYER STATE =================

    private void CheckPlayerState()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        playerIsIdle =
            Mathf.Abs(moveInput) < 0.01f &&
            (playerController == null || playerController.IsGrounded);
    }

    // ================= LANDING =================

    private void TryBeginLanding()
    {
        if (landingInProgress) return;

        for (int i = 0; i < perchSlotsTaken.Length; i++)
        {
            if (!perchSlotsTaken[i])
            {
                perchSlotsTaken[i] = true;
                perchSlotIndex = i;

                isLanding = true;
                landingInProgress = true;

                if (animator != null)
                    animator.speed = 0.6f;

                return;
            }
        }
    }

    private void LandingMotion()
    {
        Vector3 perchTarget = player.position + perchSlots[perchSlotIndex];

        transform.position = Vector3.Lerp(
            transform.position,
            perchTarget,
            landingApproachSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, perchTarget) < 0.05f)
        {
            isLanding = false;
            isPerched = true;
            landingInProgress = false;

            if (animator != null)
                animator.speed = 0.3f;

            // 🌟 Notify soul light
            if (soulLight != null)
                soulLight.OnButterflyPerched();
        }
    }

    // ================= PERCHED =================

    private void PerchMotion()
    {
        Vector3 basePos = player.position + perchSlots[perchSlotIndex];
        float bob = Mathf.Sin(Time.time * perchBobSpeed) * perchBobAmount;

        transform.position = Vector3.Lerp(
            transform.position,
            basePos + Vector3.up * bob,
            4f * Time.deltaTime
        );
    }

    // ================= RESET =================

    private void ResetPerchState()
    {
        idleTimer = 0f;

        if (isPerched || isLanding)
        {
            if (perchSlotIndex >= 0)
                perchSlotsTaken[perchSlotIndex] = false;

            perchSlotIndex = -1;
            isLanding = false;
            isPerched = false;
            landingInProgress = false;

            if (animator != null)
                animator.speed = 1f;

            // 🌑 Notify soul light
            if (soulLight != null)
                soulLight.OnButterflyLeft();
        }
    }

    // ================= ORBIT =================

    private void CalculateOrbitTarget()
    {
        orbitAngle += orbitSpeed * Time.deltaTime * 60f;

        float x = Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * idleOrbitRadius;
        float y = Mathf.Sin(orbitAngle * Mathf.Deg2Rad) + idleHeightOffset;

        targetPosition =
            player.position +
            new Vector3(x, y, 0) +
            (Vector3)randomOffset;
    }

    private void Move(float speed)
    {
        transform.position =
            Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }

    // ================= UTILS =================

    private void UpdateRandomOffset()
    {
        randomTimer += Time.deltaTime;
        if (randomTimer >= 1f)
        {
            randomTimer = 0f;
            randomOffset = Random.insideUnitCircle * randomness;
        }
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        Vector3 velocity = targetPosition - transform.position;
        if (Mathf.Abs(velocity.x) > 0.01f)
            spriteRenderer.flipX = velocity.x < 0;
    }
}
