using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AbilityNPC : MonoBehaviour
{
    public enum AbilityType { Dash, Float, WallHold }

    [Header("NPC Settings")]
    [SerializeField] private AbilityType abilityToGrant;
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string message = "You unlocked a new ability!";

    [Header("Dialogue Lines (keep your existing text)")]
    [SerializeField] private List<string> dialogueLines = new List<string>();

    [Header("Interact")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private float interactDistance = 2f;

    [Header("Prompt")]
    [SerializeField] private NPCWorldPrompt worldPrompt;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private string promptText = "F to interact";

    [Header("Sound Effects")]
    [SerializeField] private AudioClip grantAbilitySound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    private AudioSource audioSource;
    private Transform player;

    private bool inRange;
    private bool waitingDialogueEnd;
    private bool abilityGranted;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = soundVolume;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (worldPrompt == null) worldPrompt = GetComponent<NPCWorldPrompt>();
        if (worldPrompt == null) worldPrompt = gameObject.AddComponent<NPCWorldPrompt>();
        worldPrompt.SetOffset(promptOffset);
        worldPrompt.SetText(promptText);
        worldPrompt.Hide();

        // Daha önce alınmışsa gizle
        if (NPCFlags.IsNPCUsed(abilityToGrant))
        {
            abilityGranted = true;
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (abilityGranted || player == null) return;

        // Dialogue bittiyse ability ver
        if (waitingDialogueEnd)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
            {
                waitingDialogueEnd = false;
                GrantAbility();
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        bool nowInRange = dist <= interactDistance;

        if (nowInRange && !inRange)
        {
            inRange = true;
            worldPrompt.Show();
        }
        else if (!nowInRange && inRange)
        {
            inRange = false;
            worldPrompt.Hide();
        }

        if (inRange && Input.GetKeyDown(interactKey))
        {
            worldPrompt.Hide();

            if (DialogueManager.Instance != null && dialogueLines != null && dialogueLines.Count > 0)
            {
                DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
                waitingDialogueEnd = true;
            }
            else
            {
                GrantAbility();
            }
        }
    }

    private void GrantAbility()
    {
        if (abilityGranted) return;

        var controller = player.GetComponent<PlayerController>();
        if (controller == null) return;

        switch (abilityToGrant)
        {
            case AbilityType.Dash: controller.UnlockDash(); break;
            case AbilityType.Float: controller.UnlockFloat(); break;
            case AbilityType.WallHold: controller.UnlockWallHold(); break;
        }

        NPCFlags.SetNPCUsed(abilityToGrant);

        if (grantAbilitySound != null && audioSource != null)
            audioSource.PlayOneShot(grantAbilitySound, soundVolume);

        UIManager.Instance?.ShowMessage(message);

        // Quest ilerletme (kedi/crow)
        if (QuestSequenceManager.Instance != null)
        {
            if (abilityToGrant == AbilityType.Dash)
                QuestSequenceManager.Instance.CompleteStep(QuestStepId.FindCat);

            if (abilityToGrant == AbilityType.Float)
                QuestSequenceManager.Instance.CompleteStep(QuestStepId.FindCrow);
        }

        abilityGranted = true;
        worldPrompt.Hide();

        SaveSystem.Instance?.SaveGame(player.gameObject);

        // NPC’yi kapat
        Invoke(nameof(DisableNPC), grantAbilitySound != null ? grantAbilitySound.length : 0.1f);
    }

    private void DisableNPC()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (worldPrompt != null) worldPrompt.Hide();
    }
}
 




