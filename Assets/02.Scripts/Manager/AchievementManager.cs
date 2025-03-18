using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("Achievement List")]
    public List<Achievement> achievements = new List<Achievement>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateAchievements(string achievements, int amount)
    {


    }

}
