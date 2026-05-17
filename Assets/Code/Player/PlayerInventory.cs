using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public enum ElementType { Cup, Staff, Sword, Medal }
    public ElementType type;

    [Header("Audio Settings")]
    public AudioClip cupClip;
    public AudioClip staffClip;
    public AudioClip swordClip;
    public AudioClip medalClip;
    [Range(0f, 1f)]
    public float collectionVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        switch (type)
        {
            case ElementType.Cup:
                MagicianNPC.hasCup = true;
                if (cupClip != null)
                    AudioSource.PlayClipAtPoint(cupClip, transform.position, collectionVolume);
                break;

            case ElementType.Staff:
                MagicianNPC.hasStaff = true;
                if (staffClip != null)
                    AudioSource.PlayClipAtPoint(staffClip, transform.position, collectionVolume);
                break;

            case ElementType.Sword:
                MagicianNPC.hasSword = true;
                if (swordClip != null)
                    AudioSource.PlayClipAtPoint(swordClip, transform.position, collectionVolume);
                break;

            case ElementType.Medal:
                MagicianNPC.hasMedal = true;
                if (medalClip != null)
                    AudioSource.PlayClipAtPoint(medalClip, transform.position, collectionVolume);
                break;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Collected: " + type);

        Destroy(gameObject);
    }
}
