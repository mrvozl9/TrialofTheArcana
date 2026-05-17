using UnityEngine;
using UnityEngine.UI;

public class BookMinimapController : MonoBehaviour
{
    [Header("Map UI (INSIDE BOOK SPREAD)")]
    public RectTransform mapRect;        // The full 2-page map image
    public RectTransform playerIcon;     // Butterfly icon (UI Image)

    [Header("Player")]
    public Transform player;

    [Header("World Bounds (LEVEL SIZE)")]
    public Vector2 worldMin;             // Bottom-left corner of the level
    public Vector2 worldMax;             // Top-right corner of the level

    [Header("Visual Fine Tuning")]
    public Vector2 iconOffset;           // Fix parchment / art padding

    [Header("Options")]
    public bool rotateIcon = true;
    public bool flipSpriteWithDirection = true; // Flip butterfly left/right based on movement

    [Header("Butterfly Animation")]
    public Sprite[] butterflyFrames;     // Assign your butterfly sprite frames here
    public float animationSpeed = 0.1f;  // Time between frames
    private Image butterflyImage;
    private int currentFrame = 0;
    private float frameTimer = 0f;

    [Header("Glow Effect")]
    public bool enableGlow = true;
    public Image glowImage;              // Optional: Separate glow image behind butterfly
    public Color glowColor = new Color(1f, 0.8f, 0.2f, 0.5f); // Yellow-ish glow
    public float glowPulseSpeed = 2f;    // How fast the glow pulses
    public float glowMinAlpha = 0.3f;
    public float glowMaxAlpha = 0.8f;
    private float glowTimer = 0f;

    [Header("Movement Glow")]
    public bool glowWhenMoving = true;   // Glow brighter when moving
    public float movementThreshold = 0.1f; // Minimum movement to trigger glow
    private Vector2 lastPlayerPosition;
    private float lastPlayerDirection = 1f; // Track horizontal direction (-1 = left, 1 = right)

    void Start()
    {
        // Get the Image component from playerIcon
        if (playerIcon != null)
        {
            butterflyImage = playerIcon.GetComponent<Image>();
            if (butterflyImage == null)
            {
                Debug.LogWarning("⚠️ PlayerIcon doesn't have an Image component!");
            }
        }

        // Setup glow image if provided
        if (glowImage != null)
        {
            glowImage.color = glowColor;
        }

        // Initialize last position
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    void Update()
    {
        // Only update when minimap page is visible
        if (!gameObject.activeInHierarchy)
            return;

        if (!player || !mapRect || !playerIcon)
            return;

        // Update position
        UpdatePlayerIconPosition();

        // Update sprite direction based on player movement
        UpdateSpriteDirection();

        // Update butterfly animation
        UpdateButterflyAnimation();

        // Update glow effect
        UpdateGlowEffect();
    }

    private void UpdatePlayerIconPosition()
    {
        // Normalize player position (0..1)
        float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x);
        float normalizedY = Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.y);

        // Convert to UI space
        float mapWidth = mapRect.sizeDelta.x;
        float mapHeight = mapRect.sizeDelta.y;
        float mapX = (normalizedX * mapWidth) - mapWidth * 0.5f;
        float mapY = (normalizedY * mapHeight) - mapHeight * 0.5f;

        playerIcon.anchoredPosition = new Vector2(mapX, mapY) + iconOffset;

        // Update glow position to match
        if (glowImage != null)
        {
            glowImage.rectTransform.anchoredPosition = playerIcon.anchoredPosition;
        }

        if (rotateIcon)
        {
            playerIcon.localRotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.z);

            if (glowImage != null)
            {
                glowImage.rectTransform.localRotation = playerIcon.localRotation;
            }
        }
    }

    private void UpdateSpriteDirection()
    {
        if (!flipSpriteWithDirection || player == null || playerIcon == null)
            return;

        // Calculate horizontal movement
        float deltaX = player.position.x - lastPlayerPosition.x;

        // Only update direction if there's significant horizontal movement
        if (Mathf.Abs(deltaX) > 0.01f)
        {
            lastPlayerDirection = Mathf.Sign(deltaX); // -1 for left, 1 for right
        }

        // Flip the butterfly sprite
        Vector3 scale = playerIcon.localScale;
        scale.x = Mathf.Abs(scale.x) * lastPlayerDirection;
        playerIcon.localScale = scale;

        // Also flip the glow if it exists
        if (glowImage != null)
        {
            Vector3 glowScale = glowImage.rectTransform.localScale;
            glowScale.x = Mathf.Abs(glowScale.x) * lastPlayerDirection;
            glowImage.rectTransform.localScale = glowScale;
        }
    }

    private void UpdateButterflyAnimation()
    {
        if (butterflyFrames == null || butterflyFrames.Length == 0 || butterflyImage == null)
            return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= animationSpeed)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % butterflyFrames.Length;
            butterflyImage.sprite = butterflyFrames[currentFrame];
        }
    }

    private void UpdateGlowEffect()
    {
        if (!enableGlow || glowImage == null)
            return;

        // Calculate if player is moving
        bool isMoving = false;
        if (player != null)
        {
            float distance = Vector2.Distance(lastPlayerPosition, player.position);
            isMoving = distance > movementThreshold * Time.deltaTime;
            lastPlayerPosition = player.position;
        }

        // Pulse the glow
        glowTimer += Time.deltaTime * glowPulseSpeed;
        float pulseValue = Mathf.Sin(glowTimer) * 0.5f + 0.5f; // 0 to 1

        // Brighter glow when moving
        float targetAlpha = glowWhenMoving && isMoving
            ? Mathf.Lerp(glowMinAlpha, glowMaxAlpha, pulseValue) * 1.5f
            : Mathf.Lerp(glowMinAlpha, glowMaxAlpha, pulseValue);

        Color newColor = glowColor;
        newColor.a = Mathf.Clamp01(targetAlpha);
        glowImage.color = newColor;
    }

    // 🔧 Helper: Call this to set world bounds automatically from a Tilemap or Collider
    public void SetWorldBoundsFromCollider(Collider2D worldCollider)
    {
        if (worldCollider == null) return;

        Bounds bounds = worldCollider.bounds;
        worldMin = bounds.min;
        worldMax = bounds.max;

        Debug.Log($"🗺️ World bounds set: Min={worldMin}, Max={worldMax}");
    }
}