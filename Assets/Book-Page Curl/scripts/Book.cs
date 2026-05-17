using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public enum FlipMode
{
    RightToLeft,
    LeftToRight
}

public class Book : MonoBehaviour
{
    [Header("References")]
    public Canvas canvas;
    public RectTransform BookPanel;

    [Header("Pages")]
    public Sprite background;
    public Sprite[] bookPages;

    [Header("UI Images")]
    public Image LeftNext;
    public Image RightNext;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pageFlipSound;

    [Header("Settings")]
    public bool interactable = true;

    [Header("Events")]
    public UnityEvent OnFlip;

    // currentPage ALWAYS represents the RIGHT page index
    public int currentPage = 0;

    Camera uiCamera;

    // ----------------------------------------------------
    // ✅ COMPATIBILITY (FIXES YOUR ERRORS)
    // ----------------------------------------------------

    public int TotalPageCount
    {
        get { return bookPages != null ? bookPages.Length : 0; }
    }

    public void TweenForward()
    {
        if (!interactable) return;
        if (currentPage < TotalPageCount - 1)
            StartCoroutine(TurnForward());
    }

    public void TweenBackward()
    {
        if (!interactable) return;
        if (currentPage > 0)
            StartCoroutine(TurnBackward());
    }

    // ----------------------------------------------------

    void Start()
    {
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        ApplyPageState();
    }

    void Update()
    {
        if (!interactable)
            return;

        if (Input.GetMouseButtonDown(0))
            HandleClick(Input.mousePosition);
    }

    // ----------------------------------------------------
    // CLICK LOGIC
    // ----------------------------------------------------

    void HandleClick(Vector2 screenPos)
    {
        if (!BookPanel)
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(BookPanel, screenPos, uiCamera))
            return;

        // BACK COVER: click anywhere → go back
        if (currentPage == TotalPageCount - 1)
        {
            TweenBackward();
            return;
        }

        // FRONT COVER: click anywhere → go forward
        if (currentPage == 0)
        {
            TweenForward();
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            BookPanel,
            screenPos,
            uiCamera,
            out Vector2 localPoint
        );

        // NORMAL SPREAD
        if (localPoint.x < 0)
            TweenBackward();
        else
            TweenForward();
    }


    // ----------------------------------------------------
    // PAGE TURN LOGIC
    // ----------------------------------------------------

    IEnumerator TurnForward()
    {
        yield return new WaitForSeconds(0.12f);

        // Going to back cover → single page
        if (currentPage >= TotalPageCount - 2)
            currentPage = TotalPageCount - 1;
        else
            currentPage += 2;

        ApplyPageState();
    }

    IEnumerator TurnBackward()
    {
        yield return new WaitForSeconds(0.12f);

        if (currentPage <= 1)
            currentPage = 0;
        else
            currentPage -= 2;

        ApplyPageState();
    }

    // ----------------------------------------------------
    // APPLY PAGE STATE (CORE FIX)
    // ----------------------------------------------------

    void ApplyPageState()
    {
        bool isFrontCover = currentPage == 0;
        bool isBackCover = currentPage == TotalPageCount - 1;

        // RIGHT PAGE (always exists)
        RightNext.sprite = bookPages[currentPage];
        RightNext.gameObject.SetActive(true);

        // LEFT PAGE
        if (isFrontCover || isBackCover)
        {
            // Covers = single page
            LeftNext.gameObject.SetActive(false);
        }
        else
        {
            LeftNext.gameObject.SetActive(true);
            LeftNext.sprite = bookPages[currentPage - 1];
        }

        if (audioSource && pageFlipSound)
            audioSource.PlayOneShot(pageFlipSound);

        OnFlip?.Invoke();
    }
}
