using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public GameObject questListWindow;
    public Transform questListContent;
    public GameObject questItemPrefab;

    private void OnEnable()
    {
        QuestManager.OnQuestListUpdated += UpdateQuestList;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestListUpdated -= UpdateQuestList;
    }

    public void OffWindow()
    {
        questListWindow.SetActive(false);
    }

    private void UpdateQuestList(List<Quest> quests)
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Quest quest in quests)
        {
            CreateQuestItem(quest);
        }
    }

    private void CreateQuestItem(Quest quest)
    {
        GameObject questItem = Instantiate(questItemPrefab, questListContent);
        QuestItem questItemComponent = questItem.GetComponent<QuestItem>();

        if (questItemComponent != null)
        {
            questItemComponent.SetQuest(quest);
        }
    }
}
