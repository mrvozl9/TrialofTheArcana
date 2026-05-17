using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;

public class BookQuestDetailsPage : MonoBehaviour
{
    public static BookQuestDetailsPage Instance;

    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private string defaultMessage = "Select a quest from the left page";

    [Header("Ink Colors")]
    [SerializeField] private Color titleColor = new Color(0.16f, 0.14f, 0.12f);
    [SerializeField] private Color bodyColor = new Color(0.25f, 0.22f, 0.18f);
    [SerializeField] private Color goldColor = new Color(0.80f, 0.65f, 0.25f);
    [SerializeField] private Color completedColor = new Color(0.55f, 0.75f, 0.55f);

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(QuestSelection.Instance?.SelectedQuest))
            ShowQuest(QuestSelection.Instance.SelectedQuest);
        else
            ShowDefault();
    }

    public void ShowQuest(string questName)
    {
        var steps = QuestLog.Instance.GetQuestSteps(questName);
        if (steps == null)
        {
            ShowDefault();
            return;
        }

        bool completed = steps.Last().Contains("COMPLETED");
        StringBuilder sb = new();

        // ─── TITLE ─────────────────────────
        sb.AppendLine($"<color={Hex(titleColor)}><size=40><b>{questName}</b></size></color>");

        // ─── STATUS ────────────────────────
        sb.AppendLine(
            completed
                ? $"<color={Hex(completedColor)}><size=30><b>✓ COMPLETED</b></size></color>"
                : $"<color={Hex(goldColor)}><size=30><b>IN PROGRESS</b></size></color>"
        );

        // ─── OBJECTIVES HEADER ──────────────
        sb.AppendLine($"<color={Hex(titleColor)}><size=32><b>Objectives</b></size></color>");

        // ─── OBJECTIVES LIST ────────────────
        foreach (var step in steps)
        {
            if (step.Contains("COMPLETED"))
                sb.AppendLine($"<color={Hex(completedColor)}><size=28><b>✓ {step}</b></size></color>");
            else
                sb.AppendLine($"<color={Hex(bodyColor)}><size=28>• {step}</size></color>");
        }

        detailsText.text = sb.ToString();
        detailsText.enableWordWrapping = true;
        detailsText.alignment = TextAlignmentOptions.TopLeft;
    }

    void ShowDefault()
    {
        detailsText.text =
            $"<color={Hex(titleColor)}><size=44><b>Quest Log</b></size></color>\n" +
            $"<color={Hex(bodyColor)}><size=30><i>{defaultMessage}</i></size></color>\n" +
            $"<color={Hex(bodyColor)}><size=26>Click a quest on the left page to read its story and objectives.</size></color>";

        detailsText.alignment = TextAlignmentOptions.Center;
    }

    static string Hex(Color c)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(c)}";
    }

    public static void RefreshAll()
    {
        if (!string.IsNullOrEmpty(QuestSelection.Instance?.SelectedQuest))
            Instance?.ShowQuest(QuestSelection.Instance.SelectedQuest);
    }
}
