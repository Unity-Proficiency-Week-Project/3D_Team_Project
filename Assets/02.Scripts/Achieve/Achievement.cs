using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Achievement
{
    public string name;
    public string description;
    public bool isAccepted;
    public bool isUnlocked;
    public int goal;
    public int curProgress;
}
