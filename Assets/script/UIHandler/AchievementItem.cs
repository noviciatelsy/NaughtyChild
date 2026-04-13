using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public void SetData(AchievementSO achievementData)
    {
        if (icon != null)
            icon.sprite = achievementData.icon;
        if (nameText != null)
            nameText.text = achievementData.achievementName;
        if (descriptionText != null)
            descriptionText.text = achievementData.description;
    }
}
