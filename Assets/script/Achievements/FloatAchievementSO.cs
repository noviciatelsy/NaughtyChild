using UnityEngine;

[CreateAssetMenu(fileName = "NewFloatAchievement", menuName = "Achievements/Float")]
public class FloatAchievementSO : AchievementSO
{
    public float currentValue;
    public float targetValue;

    public void AddProgress(float amount)
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
        currentValue = 0f;
    }
}
