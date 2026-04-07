using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Interact
{
    protected override void OnInteracted(GameObject item)
    {
        //判断是否是 axe
        if (item != null && item.GetComponent<axe>() != null)
        {
            Debug.Log("用斧头砍箱子");

            BreakBox();
            return;
        }

        // 默认逻辑
        Debug.Log("普通交互箱子");
    }

    private void BreakBox()
    {
        Debug.Log("箱子被破坏！");
        Destroy(gameObject);
    }
}
