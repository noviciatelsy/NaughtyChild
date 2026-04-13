using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cookie : Interact
{
    public override bool InteractObject(GameObject item)
    {
        base.InteractObject(item);

        var flyEffect = GetComponent<CookieFlyEffect>();
        if (flyEffect != null)
        {
            flyEffect.Fly(() =>
            {
                GameManager.Instance.CompleteRound();
            });
        }
        else
        {
            GameManager.Instance.CompleteRound();
        }

        return true;
    }

    public override void Reset()
    {
        var flyEffect = GetComponent<CookieFlyEffect>();
        if (flyEffect != null)
            flyEffect.ResetFlyState();

        base.Reset();
    }
}
