using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookQuestListPage : MonoBehaviour
{
    public static BookQuestListPage Instance;

    [SerializeField] private Transform container;
    [SerializeField] private GameObject questButtonPrefab;

    [Header("Colors (Ink Style)")]
    [SerializeField] private Color activeColor = new Color(0.25f, 0.22f, 0.18f);   // parchment ink
    [SerializeField] private Color completedColor = new Color(0.55f, 0.75f, 0.55f); // muted green
    [SerializeField] private Color selectedColor = new Color(0.85f, 0.65f, 0.25f);  // gold

    [Header("Icons")]
    [SerializeField] private string activeIcon = "◈";
    [SerializeField] private string completedIcon = "✓";
    [SerializeField] private string selectedIcon = "❖";

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Refresh();
    }

    public static void RefreshAll()
    {
        Instance?.Refresh();
    }

    void Refresh()
    {
        foreach (Transform c in container)
            Destroy(c.gameObject);

        if (QuestLog.Instance == null) return;

        var quests = QuestLog.Instance.GetActiveQuests();

        foreach (var quest in quests)
        {
            GameObject obj = Instantiate(questButtonPrefab, container);
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            Button button = obj.GetComponent<Button>();

            bool completed = quest.Value.Count > 0 &&
                             quest.Value[^1].Contains("COMPLETED");

            string icon = completed ? completedIcon : activeIcon;

            text.text = $"{icon} {quest.Key}";
            text.fontSize = 45;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = completed ? completedColor : activeColor;

            string questName = quest.Key;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                HighlightSelected(text, questName);

                QuestSelection.Instance?.Select(questName);
                BookQuestDetailsPage.Instance?.ShowQuest(questName);
            });
        }
    }

    void HighlightSelected(TextMeshProUGUI selected, string questName)
    {
        foreach (Transform child in container)
        {
            TextMeshProUGUI t = child.GetComponent<TextMeshProUGUI>();
            if (t == null) continue;

            string cleanName = t.text
                .Replace(selectedIcon, "")
                .Replace(activeIcon, "")
                .Replace(completedIcon, "")
                .Trim();

            var steps = QuestLog.Instance.GetQuestSteps(cleanName);
            bool completed = steps != null && steps.Count > 0 &&
                             steps[^1].Contains("COMPLETED");

            t.color = completed ? completedColor : activeColor;
            t.text = $"{(completed ? completedIcon : activeIcon)} {cleanName}";
        }

        selected.color = selectedColor;
        selected.text = $"{selectedIcon} {questName}";
    }
}
