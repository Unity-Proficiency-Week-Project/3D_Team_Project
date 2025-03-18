using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> availableQuests = new List<Quest>(); // 전체 퀘스트 목록
    public List<Quest> activeQuests = new List<Quest>(); // 진행 중인 퀘스트 목록

    public delegate void OnQuestListUpdatedDelegate(List<Quest> quests);
    public static event OnQuestListUpdatedDelegate OnQuestListUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        LoadQuests();
        UpdateQuestUI(); // 시작 시 UI 업데이트
    }

    private void LoadQuests()
    {
        availableQuests = new List<Quest>
        {
            CreateQuest("돌 채집", "돌 5개 수집", QuestGoalType.GatherResource, "Item_Rock", 5),
            CreateQuest("나무 수집", "나무 3개 수집", QuestGoalType.GatherResource, "Item_Wood", 3),
            CreateQuest("도끼 제작", "도끼를 제작하세요.", QuestGoalType.CraftItem, "도끼", 1),
            CreateQuest("칼 제작", "칼을 제작하세요.", QuestGoalType.CraftItem, "칼", 1),
            CreateQuest("늑대 사냥", "늑대 3마리 처치", QuestGoalType.KillSpecificEnemy, "Tiger", 3),
            CreateQuest("모든 적 처치", "적 5마리 처치", QuestGoalType.KillAnyEnemy, "", 5)
        };
    }

    private Quest CreateQuest(string name, string description, QuestGoalType type, string target, int goal)
    {
        Quest newQuest = ScriptableObject.CreateInstance<Quest>();
        newQuest.questName = name;
        newQuest.description = description;
        newQuest.goalType = type;
        newQuest.target = target;
        newQuest.goalAmount = goal;
        newQuest.curProgress = 0;
        newQuest.isAccepted = false;
        newQuest.isCompleted = false;
        return newQuest;
    }

    public void AcceptQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            quest.isAccepted = true;
            UpdateQuestUI();
        }
    }

    public void CheckQuestCompletion()
    {
        foreach (Quest quest in activeQuests)
        {
            if (quest.curProgress >= quest.goalAmount)
            {
                quest.isCompleted = true;
            }
        }
        UpdateQuestUI();
    }

    public void CompleteQuest(Quest quest)
    {
        if (activeQuests.Contains(quest) && quest.isCompleted)
        {
            activeQuests.Remove(quest);
            UpdateQuestUI();
        }
    }

    private void UpdateQuestUI()
    {
        // 모든 퀘스트(수락 가능 + 진행 중)를 UI에 표시
        List<Quest> allQuests = new List<Quest>(availableQuests);
        allQuests.AddRange(activeQuests);

        OnQuestListUpdated?.Invoke(allQuests);
    }

    public void UpdateQuestProgress(QuestGoalType type, string target, int amount = 1)
    {
        foreach (Quest quest in activeQuests)
        {
            if (quest.goalType == type && quest.target == target && !quest.isCompleted)
            {
                quest.curProgress += amount;
                if (quest.curProgress >= quest.goalAmount)
                {
                    quest.isCompleted = true;
                }
            }
        }

        UpdateQuestUI();
    }
}
