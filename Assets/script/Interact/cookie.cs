using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cookie : Interact
{
    public override void InteractObject()
    {
        base.InteractObject();
        GameManager.Instance.CompleteRound();
    }

}
