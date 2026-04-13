using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    [TextArea(2, 5)]
    public string dialogueText;

    private bool hasPlayed;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (hasPlayed) return;
        hasPlayed = true;

        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(dialogueText))
        {
            DialogueManager.Instance.SetText(dialogueText);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        hasPlayed = false;
    }
}
