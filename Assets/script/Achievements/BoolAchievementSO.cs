using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBoolAchievement", menuName = "Achievements/Bool")]
public class BoolAchievementSO : AchievementSO
{
    public List<bool> conditions = new List<bool>();

    public void SetCondition(int index, bool value)
    {
        if (index < 0 || index >= conditions.Count) return;
        conditions[index] = value;
        TryUnlock();
    }

    public override bool CheckUnlock()
    {
        foreach (var c in conditions)
            if (!c) return false;
        return conditions.Count > 0;
    }

    public override void ResetProgress()
    {
        base.ResetProgress();
        for (int i = 0; i < conditions.Count; i++)
            conditions[i] = false;
    }
}
