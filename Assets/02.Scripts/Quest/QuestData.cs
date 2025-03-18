using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestData : MonoBehaviour
{
    private void Start()
    {
        LoadQuests();
    }

    private void LoadQuests()
    {
        Quest[] quests = Resources.LoadAll<Quest>("Quests"); // "Quests" 폴더에서 ScriptableObject 불러오기
        QuestManager.Instance.availableQuests = new List<Quest>(quests);
    }
}
