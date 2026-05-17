using UnityEngine;

public enum QuestStepId
{
    FindCat,
    FindCrow,
    FindMagician,
    BringElements,
    Done
}

public class QuestSequenceManager : MonoBehaviour
{
    public static QuestSequenceManager Instance { get; private set; }

    [SerializeField] private QuestTrackerUI ui;

    [Header("Quest Texts")]
    [SerializeField] private string findCatText = "Find the Cat";
    [SerializeField] private string findCrowText = "Find the Crow";
    [SerializeField] private string findMagicianText = "Find the Magician";
    [SerializeField] private string bringElementsText = "Bring me 4 sacred elements";

    [Header("Done Behavior")]
    [SerializeField] private bool hideTrackerWhenDone = true;
    [SerializeField] private string doneText = ""; // gizlemiyorsan göstermek istersen buraya yaz

    public QuestStepId CurrentStep { get; private set; } = QuestStepId.FindCat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (ui == null) return;

        // Done ise paneli gizle
        if (CurrentStep == QuestStepId.Done)
        {
            if (hideTrackerWhenDone)
            {
                ui.SetVisible(false);
                return;
            }

            // gizleme kapalýysa text gösterebilir
            ui.SetVisible(true);
            ui.SetQuestText(doneText);
            return;
        }

        ui.SetVisible(true);

        switch (CurrentStep)
        {
            case QuestStepId.FindCat:
                ui.SetQuestText(findCatText, true);
                break;

            case QuestStepId.FindCrow:
                ui.SetQuestText(findCrowText, true);

                break;

            case QuestStepId.FindMagician:
                ui.SetQuestText(findMagicianText, true);
                break;

            case QuestStepId.BringElements:
                ui.SetQuestText(bringElementsText, true);
                break;
        }
    }

    public void CompleteStep(QuestStepId step)
    {
        if (step != CurrentStep) return;

        switch (CurrentStep)
        {
            case QuestStepId.FindCat:
                CurrentStep = QuestStepId.FindCrow;
                break;

            case QuestStepId.FindCrow:
                CurrentStep = QuestStepId.FindMagician;
                break;

            case QuestStepId.FindMagician:
                CurrentStep = QuestStepId.BringElements;
                break;

            case QuestStepId.BringElements:
                CurrentStep = QuestStepId.Done;
                break;
        }

        RefreshUI();
    }
}

