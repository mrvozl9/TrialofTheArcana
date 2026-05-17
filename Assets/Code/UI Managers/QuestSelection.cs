using UnityEngine;

public class QuestSelection : MonoBehaviour
{
    public static QuestSelection Instance;

    public string SelectedQuest { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Select(string questName)
    {
        SelectedQuest = questName;

        // Notify pages
        BookQuestDetailsPage.RefreshAll();
        BookQuestListPage.RefreshAll();
    }
}
