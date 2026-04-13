using UnityEngine;

[CreateAssetMenu(fileName = "NewIntAchievement", menuName = "Achievements/Int")]
public class IntAchievementSO : AchievementSO
{
    public int currentValue;
    public int targetValue;

    public void AddProgress(int amount)
    {
        currentValue += amount;
        TryUnlock();
    }

    public override bool CheckUnlock()
    {
        return currentValue >= targetValue;
    }

    public override void ResetProgress()
    {
        base.ResetProgress();
        currentValue = 0;
    }
}
