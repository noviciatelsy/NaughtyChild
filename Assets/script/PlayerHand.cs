using DG.Tweening;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand Instance;

    [SerializeField] private Transform handPoint;

    public Throwable currentItem;

    private void Awake()
    {
        Instance = this;
    }

    public bool HasItem => currentItem != null;

    public void PickItem(Throwable item)
    {
        currentItem = item;

        item.transform.SetParent(handPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        item.OnPicked(handPoint);
    }

    public void UseItem(GameObject target)
    {
        if (currentItem == null) return;

        currentItem.OnUse(target);
    }

    public void ThrowItem(Vector3 dir)
    {
        if (currentItem == null) return;

        currentItem.OnThrow(dir);
        currentItem = null;
    }

    public void ResetHand()
    {
        if (currentItem != null)
        {
            currentItem.transform.SetParent(null);

            // 可选：丢到地上（防止消失）
            //currentItem.transform.position = handPoint.position + Vector3.forward * 0.5f;

            currentItem.OnThrow(Vector3.zero); // 或者直接“失活逻辑”
            
        }

        currentItem = null;
    }
}