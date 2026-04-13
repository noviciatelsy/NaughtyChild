using UnityEngine;
using UnityEngine.Playables;

public class DialogueClip : PlayableAsset
{
    public DialogueBehaviour template = new DialogueBehaviour();

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<DialogueBehaviour>.Create(graph, template);
    }
}
