using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class PlayerLockBehaviour : PlayableBehaviour
{
    public Vector3 lockRotation;

    private playermovement movement;
    private Transform playerTransform;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        movement = player.GetComponent<playermovement>();
        playerTransform = player.transform;

        if (movement != null)
            movement.SetLocked(true);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerTransform == null) return;

        playerTransform.rotation = Quaternion.Euler(lockRotation);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (movement != null)
            movement.SetLocked(false);
    }
}
