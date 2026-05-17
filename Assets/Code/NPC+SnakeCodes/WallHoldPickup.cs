using UnityEngine;

public class WallHoldPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.UnlockWallHold(); // unlock wall hold
            Destroy(gameObject);     // remove the spider from the scene
        }
    }
}
