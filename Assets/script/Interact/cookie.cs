using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cookie : Interact
{
    public override bool InteractObject(GameObject item)
    {
        base.InteractObject(item);
        GameManager.Instance.CompleteRound();
        return true;
    }

}
