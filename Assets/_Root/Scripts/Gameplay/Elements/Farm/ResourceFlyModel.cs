using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pancake;
using UnityEngine;

public class ResourceFlyModel : GameComponent
{
    private Vector3 defaultPos;
    private float randomDistance;
    private float randomJumpForce;
    private int randomNumJump;

    private void Awake()
    {
        defaultPos = transform.localPosition;
    }

    public void DoBouncing(Action completeAction)
    {
        var randomSign = UnityEngine.Random.Range(0, 2) * 2 - 1;
        randomDistance = UnityEngine.Random.Range(1, 4) * 0.25f * randomSign;
        randomJumpForce = UnityEngine.Random.Range(2, 4) * 0.4f;
        randomNumJump = UnityEngine.Random.Range(1, 3);
        var randomPos = new Vector3(defaultPos.x - randomDistance, 0.0f, defaultPos.z + randomDistance);
        transform.DOLocalJump(randomPos, randomJumpForce, randomNumJump, duration: randomNumJump / 2.0f ).SetEase(Ease.Linear)
            .OnComplete(() => completeAction?.Invoke());
    }
}