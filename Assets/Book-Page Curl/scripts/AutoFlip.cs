using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Book))]
public class AutoFlip : MonoBehaviour
{
    public float PageFlipTime = 0.6f;
    public float TimeBetweenPages = 0.4f;

    Book book;
    bool isFlipping;

    void Awake()
    {
        book = GetComponent<Book>();
    }

    public void FlipNext()
    {
        if (isFlipping) return;
        if (book.currentPage + 2 >= book.TotalPageCount) return;

        StartCoroutine(FlipRoutine(true));
    }

    public void FlipPrev()
    {
        if (isFlipping) return;
        if (book.currentPage - 2 < 0) return;

        StartCoroutine(FlipRoutine(false));
    }

    IEnumerator FlipRoutine(bool forward)
    {
        isFlipping = true;

        if (forward)
            book.TweenForward();
        else
            book.TweenBackward();

        yield return new WaitForSeconds(PageFlipTime);
        isFlipping = false;
    }
}
