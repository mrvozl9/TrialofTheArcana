using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    public enum MoveDirection { Horizontal, Vertical }

    [Header("Movement Settings")]
    public MoveDirection moveDirection = MoveDirection.Horizontal;
    public float moveDistance = 3f;
    public float speed = 2f;
    public float pauseTime = 0.5f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private float pauseTimer = 0f;

    void Start()
    {
        startPos = transform.position;
        Vector3 offset = (moveDirection == MoveDirection.Horizontal ? Vector3.right : Vector3.up) * moveDistance;
        targetPos = startPos + offset;
    }

    void Update()
    {
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        Vector3 destination = movingToTarget ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.02f)
        {
            movingToTarget = !movingToTarget;
            pauseTimer = pauseTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            StartCoroutine(SetParentNextFrame(collision.transform, true));
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            StartCoroutine(SetParentNextFrame(collision.transform, false));
    }

    private IEnumerator SetParentNextFrame(Transform player, bool makeChild)
    {
        yield return null;

        if (player != null)
        {
            if (makeChild)
                player.SetParent(transform);
            else if (player.parent == transform)
                player.SetParent(null);
        }
    }
}
