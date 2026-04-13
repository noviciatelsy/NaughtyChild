using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float charDuration = 0.05f;

    private void Awake()
    {
        Instance = this;
    }

    public void SetText(string dialogue)
    {
        text.gameObject.SetActive(true);
        text.text = "";
        text.DOText(dialogue, dialogue.Length * charDuration);
    }
}
