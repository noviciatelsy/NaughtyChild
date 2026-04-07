using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand Instance;

    [SerializeField] private Transform handPoint;

    private Throwable currentItem;

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

    public void UseItem(Interact target)
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
}