using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : Interact
{
    private bool isOpening = false;
    private int curRound = 0;
   
    public override void InteractObject(GameObject item)
    {
        base.InteractObject(item);
        Openthedoor();
    }

    private void Start()
    {
        curRound = GameManager.Instance.CurrentRound;

        GameManager.Instance.OnRoundStarted += OnRoundChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStarted -= OnRoundChanged;
    }

    private void OnRoundChanged(int newRound)
    {
        if (newRound > curRound)
        {
            curRound = newRound;
            Doorreset();
        }
    }


    public void Openthedoor()
    {
        if (!isOpening)
        {
            StartCoroutine(OpenDoor());
        }
    }

    public void Doorreset()
    {
        Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
        transform.rotation = targetRot;
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        float duration = 0.5f;
        float time = 0f;

        // 初始状态
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // 目标状态
        //Vector3 targetPos = startPos + new Vector3(-1f, 0f, 1f);
        Vector3 targetPos = startPos;
        Quaternion targetRot = Quaternion.Euler(0f, -90f, 0f);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 插值（平滑）
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        // 最终强制对齐（防止误差）
        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}
