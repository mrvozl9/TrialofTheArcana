using UnityEngine;

[ExecuteAlways]
public class InfiniteParallax : MonoBehaviour
{
    [Header("Parallax Speed (lower = slower)")]
    [SerializeField, Range(0f, 2f)] float parallaxMultiplierX = 0.5f;
    [SerializeField, Range(0f, 2f)] float parallaxMultiplierY = 1.0f;

    [Header("Looping Options")]
    [SerializeField] bool infiniteHorizontal = true;
    [SerializeField] bool infiniteVertical = true;

    private Transform cam;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : FindObjectOfType<Camera>().transform;

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            textureUnitSizeX = sr.bounds.size.x;
            textureUnitSizeY = sr.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Direct camera-relative positioning (no stored start positions)
        Vector3 parallaxPosition = cam.position;
        parallaxPosition.x *= parallaxMultiplierX;
        parallaxPosition.y *= parallaxMultiplierY;
        parallaxPosition.z = transform.position.z; // Keep original Z

        transform.position = parallaxPosition;

        // Loop horizontally
        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            float camDistX = cam.position.x - transform.position.x;
            if (Mathf.Abs(camDistX) >= textureUnitSizeX)
            {
                float offsetX = Mathf.Floor(camDistX / textureUnitSizeX) * textureUnitSizeX;
                transform.position += new Vector3(offsetX, 0, 0);
            }
        }

        // Loop vertically
        if (infiniteVertical && textureUnitSizeY > 0)
        {
            float camDistY = cam.position.y - transform.position.y;
            if (Mathf.Abs(camDistY) >= textureUnitSizeY)
            {
                float offsetY = Mathf.Floor(camDistY / textureUnitSizeY) * textureUnitSizeY;
                transform.position += new Vector3(0, offsetY, 0);
            }
        }
    }
}