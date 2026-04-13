using UnityEngine;
using UnityEngine.Playables;

public class PlayerLockClip : PlayableAsset
{
    public PlayerLockBehaviour template = new PlayerLockBehaviour();

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<PlayerLockBehaviour>.Create(graph, template);
    }
}
