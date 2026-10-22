using DG.Tweening;
using Ifreet.Core.Runtime.Audio.Generated;
using Ifreet.Core.Runtime.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour, IPoolable
{
    public GameColor color;
    public GameObject[] models;
    public int ColumnId;
    public int RowId;

    public void OnSpawn()
    {
        foreach (var model in models)
        {
            model.gameObject.SetActive(false);
        }
        transform.localScale = Vector3.one;
    }

    public void OnDespawn()
    {
        DOTween.Kill(transform);
        StopAllCoroutines();
    }

    public void Setup(GameColor _color)
    {
        color = _color;
        models[(int)color].SetActive(true);
    }

    public void ActionMove(Box box, bool isComplete, int foodCount)
    {
        transform.SetParent(box.transform);
        if(isActiveAndEnabled)
        StartCoroutine(PlaySound());
        transform.DOLocalJump(new Vector3(Random.Range(-0.4f, 0.4f),box.boxType.HasFlag(BoxType.Large)? Random.Range(0.8f, 1.1f): Random.Range(0.3f, 0.75f), Random.Range(-0.4f, 0.4f)), 4, 1, 0.55f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.localScale = Vector3.one * 0.6f;
            GameController.Instance.listFoodMoving.Remove(this);
            if (isComplete) box.LidAnim();
            box.UpdateFoodCountText(foodCount);
            box.listFood.Add(this);
            //box.NudgeDown();
            //PoolManager.Instance.Despawn("Food", gameObject);
        });
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(0.25f);
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_jump_box);
        HapticController.Instance.PlayHaptic();
    }
}
