using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCWorldPrompt : MonoBehaviour
{
    [Header("Prompt Text")]
    [SerializeField] private string promptText = "F to interact";

    [Header("Font (optional)")]
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Placement")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float worldScale = 0.01f;

    [Header("Text Look")]
    [SerializeField] private float fontSize = 55f;
    [SerializeField] private bool enableOutline = true;
    [SerializeField] private float outlineWidth = 0.3f;
    [SerializeField] private Color outlineColor = Color.black;

    [Header("Canvas Look")]
    [SerializeField] private int sortingOrder = 200;
    [SerializeField] private bool faceCamera = true;

    private Camera cam;
    private GameObject root;
    private TextMeshProUGUI tmp;
    private bool created;

    private void Awake()
    {
        cam = Camera.main;
        CreateIfNeeded();
        Hide();
    }

    private void LateUpdate()
    {
        if (!created || root == null) return;

        root.transform.position = transform.position + worldOffset;

        if (faceCamera)
        {
            if (cam == null) cam = Camera.main;
            if (cam != null)
                root.transform.forward = cam.transform.forward;
        }
    }

    public void SetText(string text)
    {
        promptText = text;
        if (tmp != null) tmp.text = promptText;
    }

    public void SetOffset(Vector3 offset)
    {
        worldOffset = offset;
    }

    public void Show()
    {
        CreateIfNeeded();
        if (root != null) root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root);
    }

    private void CreateIfNeeded()
    {
        if (created) return;
        created = true;

        // Root
        root = new GameObject($"{gameObject.name}_Prompt");
        root.transform.position = transform.position + worldOffset;
        root.transform.localScale = Vector3.one * worldScale;

        // World-space Canvas
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = sortingOrder;

        root.AddComponent<CanvasRenderer>();

        // Canvas Scaler (helps crispness)
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 25f;

        // Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(root.transform, false);

        tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = promptText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = false;

        tmp.fontSize = fontSize;

        if (customFont != null)
            tmp.font = customFont;

        if (enableOutline)
        {
            tmp.outlineWidth = outlineWidth;
            tmp.outlineColor = outlineColor;
        }
        else
        {
            tmp.outlineWidth = 0f;
        }

        // RectTransform size
        var rt = tmp.rectTransform;
        rt.sizeDelta = new Vector2(600, 200);
        rt.anchoredPosition = Vector2.zero;
    }
}

