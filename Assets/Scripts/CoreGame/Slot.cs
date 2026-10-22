using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public bool isAvaiable = true;
    public Box currentBox = null;

    public void NudgeDown()
    {
        Transform t = transform;

        t.DOKill();
        t.DOLocalMoveY(-0.2f, 0.1f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                t.DOLocalMoveY(0f, 0.1f)
                 .SetEase(Ease.Linear);
            });
    }
}
