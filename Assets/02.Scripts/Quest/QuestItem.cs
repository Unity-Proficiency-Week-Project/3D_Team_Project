using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItem : MonoBehaviour
{
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questProgress;
    public Button actionButton;
    private Quest currentQuest;

    public void SetQuest(Quest quest)
    {
        currentQuest = quest;
        questTitle.text = quest.questName;
        UpdateUI();

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => CompleteQuest());
    }

    public void ToggleQuestState()
    {
        if (QuestManager.Instance != null && currentQuest != null)
        {
            if (currentQuest.isAccepted)
            {
                if (currentQuest.isCompleted)
                {
                    QuestManager.Instance.CompleteQuest(currentQuest);
                }
            }
            else
            {
                QuestManager.Instance.AcceptQuest(currentQuest);
            }
        }
        UpdateUI(); // UI 갱신
    }

    private void UpdateUI()
    {
        // 진행 상황 업데이트
        questProgress.text = $"{currentQuest.curProgress} / {currentQuest.goalAmount}";

        // 퀘스트 상태에 따른 버튼 변경
        if (QuestManager.Instance.activeQuests.Contains(currentQuest))
        {
            if (currentQuest.isCompleted)
            {
                actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "완료";
                actionButton.interactable = true;
            }
            else
            {
                actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "진행 중";
                actionButton.interactable = false;
            }
        }
        else
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "수락";
            actionButton.interactable = true;
        }
    }

    private void CompleteQuest()
    {
        if (currentQuest.isCompleted)
        {
            QuestManager.Instance.CompleteQuest(currentQuest);
            Destroy(gameObject);
        }
    }
}
