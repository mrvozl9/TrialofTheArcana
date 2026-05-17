using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemyattacking : MonoBehaviour
{
    public float lifeTime = 2f;
    public UnityEvent onPlayerHit;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            onPlayerHit.Invoke();
            Destroy(gameObject);
        }
    }
}