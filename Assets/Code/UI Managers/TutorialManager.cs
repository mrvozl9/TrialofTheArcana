using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private GameObject tutorialPanel;

    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Tutorial Settings")]
    [SerializeField] private bool skipCompletedSteps = true;

    public enum TutorialStep
    {
        Movement,
        Jump,
        Attack,
        AttackDirectional,
        DashAcquire,
        DashUse,
        FloatAcquire,
        FloatUse,
        Completed
    }

    private TutorialStep currentStep = TutorialStep.Movement;
    private bool isTransitioning = false;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        PlayerData data = SaveSystem.Instance.LoadGame();
        if (data != null && data.tutorialCompleted)
        {
            currentStep = TutorialStep.Completed;
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
            return;
        }

        DetermineStartingStep();
        ShowTutorialPanel();
        UpdateTutorialText();

        // 📜 Start the Tutorial quest
        if (QuestLog.Instance != null)
        {
            QuestLog.Instance.StartQuest("Tutorial");
            QuestLog.Instance.Add("Tutorial", "Begin your journey");
        }
        else
        {
            Debug.LogWarning("⚠️ QuestLog.Instance is null! Make sure QuestLog GameObject exists in scene.");
        }
    }

    private void Update()
    {
        if (player == null || currentStep == TutorialStep.Completed || isTransitioning)
            return;

        HandleTutorialProgress();
    }

    private void DetermineStartingStep()
    {
        if (!skipCompletedSteps || player == null) return;

        if (player.CanDash && player.CanFloatSkill)
        {
            currentStep = TutorialStep.Completed;
            return;
        }

        if (player.CanFloatSkill && !player.CanDash)
        {
            currentStep = TutorialStep.DashAcquire;
            return;
        }

        if (player.CanDash && !player.CanFloatSkill)
        {
            currentStep = TutorialStep.FloatAcquire;
            return;
        }

        currentStep = TutorialStep.Movement;
    }

    private void ShowTutorialPanel()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void UpdateTutorialText()
    {
        if (tutorialText == null) return;

        switch (currentStep)
        {
            case TutorialStep.Movement:
                tutorialText.text = "Use <color=red>Left / Right Arrow</color> to move!";
                break;
            case TutorialStep.Jump:
                tutorialText.text = "Press <color=red>Z</color> to Jump!";
                break;
            case TutorialStep.Attack:
                tutorialText.text = "Press <color=red>X</color> to Attack!";
                break;
            case TutorialStep.AttackDirectional:
                tutorialText.text = "Hold <color=red>Up / Down</color> + <color=red>X</color>!";
                break;
            case TutorialStep.DashAcquire:
                tutorialText.text = "Find the <color=red>cat</color> to learn Dash!";
                break;
            case TutorialStep.DashUse:
                tutorialText.text = "Press <color=red>C</color> to Dash!";
                break;
            case TutorialStep.FloatAcquire:
                tutorialText.text = "Find the <color=red>crow</color> to learn Float!";
                break;
            case TutorialStep.FloatUse:
                tutorialText.text = "Hold <color=red>Z</color> while falling!";
                break;
        }

        tutorialText.alpha = 1f;
    }

    private void HandleTutorialProgress()
    {
        switch (currentStep)
        {
            case TutorialStep.Movement:
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                {
                    QuestLog.Instance?.Add("Tutorial", "Learned movement");
                    StartCoroutine(FadeAndNextStep(TutorialStep.Jump));
                }
                break;

            case TutorialStep.Jump:
                if (player.IsGrounded && Input.GetKeyDown(KeyCode.Z))
                {
                    QuestLog.Instance?.Add("Tutorial", "Learned jumping");
                    StartCoroutine(FadeAndNextStep(TutorialStep.Attack));
                }
                break;

            case TutorialStep.Attack:
                if (Input.GetKeyDown(KeyCode.X))
                {
                    QuestLog.Instance?.Add("Tutorial", "Learned attacking");
                    StartCoroutine(FadeAndNextStep(TutorialStep.AttackDirectional));
                }
                break;

            case TutorialStep.AttackDirectional:
                // Check for directional attack (Up + X or Down + X)
                if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) && Input.GetKeyDown(KeyCode.X))
                {
                    QuestLog.Instance?.Add("Tutorial", "Learned directional attacks");
                    StartCoroutine(FadeAndNextStep(TutorialStep.DashAcquire));
                }
                break;

            case TutorialStep.DashAcquire:
                if (player.CanDash)
                {
                    QuestLog.Instance?.Add("Tutorial", "Learned dash");
                    StartCoroutine(FadeAndNextStep(TutorialStep.DashUse));
                }
                break;

            case TutorialStep.DashUse:
                if (Input.GetKeyDown(KeyCode.C))
                {
                    QuestLog.Instance?.Add("Tutorial", "Used dash ability");
                    StartCoroutine(FadeAndNextStep(TutorialStep.FloatAcquire));
                }
                break;

            case TutorialStep.FloatAcquire:
                if (player.CanFloatSkill)
                {
                    QuestLog.Instance?.Add("Tutorial", "Learned float");
                    StartCoroutine(FadeAndNextStep(TutorialStep.FloatUse));
                }
                break;

            case TutorialStep.FloatUse:
                if (Input.GetKey(KeyCode.Z) && !player.IsGrounded)
                    CompleteTutorial();
                break;
        }
    }

    private IEnumerator FadeAndNextStep(TutorialStep nextStep)
    {
        isTransitioning = true;

        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            tutorialText.alpha = Mathf.Lerp(1f, 0f, t / 0.25f);
            yield return null;
        }

        tutorialText.alpha = 0f;
        yield return new WaitForSeconds(0.4f);

        currentStep = nextStep;
        UpdateTutorialText();
        isTransitioning = false;
    }

    public void CompleteTutorial()
    {
        currentStep = TutorialStep.Completed;

        // 📜 Complete the Tutorial quest
        QuestLog.Instance?.CompleteQuest("Tutorial");

        CardFlags.hasFool = true;
        BookCardPage.RefreshAll();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("You completed the tutorial!");

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        SaveSystem.Instance.SaveGame(player.gameObject);
    }
}

