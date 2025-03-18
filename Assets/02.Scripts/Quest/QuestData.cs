using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestData : MonoBehaviour
{
    public QuestManager questManager;

    private void Start()
    {
        questManager.availableQuests = new List<Quest>
        {
            new Quest { questName = "돌 채집", description = "돌 5개 수집", goalType = QuestGoalType.GatherResource, target= "돌", goalAmount = 5 },
            new Quest { questName = "나무 수집", description = "나무 3개 수집", goalType = QuestGoalType.GatherResource, target = "나무", goalAmount = 3 },
            new Quest { questName = "도끼 제작", description = "도끼를 제작하세요.", goalType = QuestGoalType.CraftItem, target = "도끼", goalAmount = 1 },
            new Quest { questName = "칼 제작", description = "칼을 제작하세요.", goalType = QuestGoalType.CraftItem, target = "칼", goalAmount = 1 },
            new Quest { questName = "늑대 사냥", description = "늑대 3마리 처치", goalType = QuestGoalType.KillSpecificEnemy, target= "늑대", goalAmount = 3 },
            new Quest { questName = "모든 적 처치", description = "적 5마리 처치", goalType = QuestGoalType.KillAnyEnemy }
        };
    }
}
