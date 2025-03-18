using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("퀘스트 리스트")]
    public List<Quest> availableQuests = new List<Quest>();     //모든 퀘스트
    public List<Quest> activeQuests = new List<Quest>();        //수락 퀘스트

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AcceptQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            quest.isAccepted = true;
            activeQuests.Add(quest);
        }
    }

    public void UpdateQuestProgress(QuestGoalType goalType, string targetName, int amount)
    {
        foreach (Quest quest in activeQuests)
        {
            if (!quest.isCompleted && quest.goalType == goalType)
            {
                if ((goalType == QuestGoalType.KillSpecificEnemy || goalType == QuestGoalType.GatherResource && quest.target != targetName))
                    continue;

                quest.curProgress += amount;

                if (quest.curProgress >= quest.goalAmount)
                {
                    quest.isCompleted = true;
                    Debug.Log($"퀘스트 완료: {quest.questName}");
                    GiveReward(quest);
                }
            }
        }
    }

    private void GiveReward(Quest quest)
    {
        Debug.Log($"보상 지급");
    }

    public void ClearCompletedQuests()
    {
        activeQuests.RemoveAll(q => q.isCompleted);
    }
}
