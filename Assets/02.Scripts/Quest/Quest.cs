using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Quest : MonoBehaviour
{
    public string questName;
    public string description;
    public bool isAccepted;
    public bool isCompleted;

    public QuestGoalType goalType;
    public int goalAmount;
    public int curProgress = 0;

    public string target;
    //public int reward;
}


public enum QuestGoalType
{
    KillSpecificEnemy, // 특정 적 처치
    KillAnyEnemy, // 아무 적 처치
    GatherResource, // 자원 수집
    CraftItem, // 아이템 제작
}