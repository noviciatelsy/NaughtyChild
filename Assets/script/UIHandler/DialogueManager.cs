using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float charDuration = 0.05f;

    public void SetText(string dialogue)
    {
        text.gameObject.SetActive(true);
        text.text = "";
        text.DOText(dialogue, dialogue.Length * charDuration);
    }
}
