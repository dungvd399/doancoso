using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Car : MonoBehaviour, IPoolable
{
    public GameColor color;
    public BoxType carType;
    public GameObject[] models;
    public Sprite star;
    public Sprite noStar;
    public SpriteRenderer[] slotRender;
    public Slot[] slots;
    public bool canUse = false;
    public int starId;
    public ParticleSystem vfxSmoke;
    public bool isMoving = false;
    public Transform wheel1;
    public Transform wheel2;
    Animator anim;

    public void OnSpawn()
    {
        foreach (var model in models)
        {
            model.gameObject.SetActive(false);
        }
        canUse = false;
    }

    public void OnDespawn()
    {
        foreach (var item in slots)
        {
            if (item.currentBox != null) PoolManager.Instance.Despawn("Box", item.currentBox.gameObject);
            item.currentBox = null;
            item.isAvaiable = true;
        }
    }

    public void Setup(GameColor _color)
    {
        color = _color;
        models[(int)color].SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            if (carType.HasFlag(BoxType.Star) && i == starId)
                slotRender[i].GetComponent<SpriteRenderer>().sprite = star;
            else slotRender[i].GetComponent<SpriteRenderer>().sprite = noStar;
        }
        wheel1 = models[(int)color].transform.GetChild(1).GetChild(1);
        wheel2 = models[(int)color].transform.GetChild(1).GetChild(2);

        anim = models[(int)color].transform.GetChild(0).GetComponent<Animator>();
    }

    public void NudgeDown()
    {
        Transform t = models[(int)color].transform.GetChild(1).GetChild(0);

        t.DOKill();
        t.DOLocalMoveY(-0.1f, 0.05f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                t.DOLocalMoveY(0f, 0.05f)
                 .SetEase(Ease.Linear);
            });
    }

    public bool IsHasSlot(bool isStar)
    {
        bool check = false;
        if(!carType.HasFlag(BoxType.Star))
        for (int i = 0; i < 3; i++)
        {
            if (slots[i].isAvaiable)
            {
                check = true;
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (slots[i].isAvaiable)
                {
                    if((i == starId && isStar) || (i!=starId && !isStar))
                    check = true;
                }
            }
        }
        return check;
    }

    public bool IsHasStar()
    {
        if (carType.HasFlag(BoxType.Star) && slots[starId].isAvaiable)
        {
            return true;
        }
        else return false;
    }

    public Slot GetSlotStar()
    {
        return slots[starId];
    }

    public Slot GetFreeSlot()
    {
        foreach (Slot item in slots)
        {
            if (carType.HasFlag(BoxType.Star))
            {
                if (item.isAvaiable && item != slots[starId]) return item;
            }
            else
            {
                if (item.isAvaiable) return item;
            }
        }
        return null;
    }

    public bool CheckFull() //check car full
    {
        bool check = true;
        foreach (Slot item in slots)
        {
            if (item.isAvaiable || item.currentBox.foodCount > 0) check = false;
        }
        int count = 0;
        for (int i = 0;i<3;i++)
        {
            if (slots[i].currentBox != null && slots[i].currentBox.canCheck) count += 1;
        }

        return (check && count >= 3);
    }

    public void MoveOut()
    {
        isMoving = true;
        //vfxSmoke.Play();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_complete);
        SetRun(true);
        transform.DOMove(LevelManager.Instance.nextCarPos.position, 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            isMoving = false;
            PoolManager.Instance.Despawn("Car", gameObject);
        });
        wheel1.transform.DORotate(new Vector3(0, 0, -360 * 5), 1);//banh xe quay khi di chuyen
        wheel2.transform.DORotate(new Vector3(0, 0, -360 * 5), 1);
    }

    public void MoveToMain()// xe chay ra giua
    {
        isMoving = true;
        //vfxSmoke.Play();
        SetRun(true); // anim run
        transform.DOMove(LevelManager.Instance.mainCarPos.position, 1).SetEase(Ease.Linear).OnComplete(() => //di chuyen, oncomplete la check di chuyen xonh
        {
            SetRun(false); //anim idle
            isMoving = false;
            canUse = true;
            AutoMovePlaceHolderBox();
        });
        wheel1.transform.DORotate(new Vector3(0, 0, -360 * 5), 1); // anim banh xe
        wheel2.transform.DORotate(new Vector3(0, 0, -360 * 5), 1); // anim banh xe
    }

    void SetRun(bool isRun) // chay anim
    {
        if (isRun)
            anim.CrossFade("Run", 0.15f);
        else
            anim.CrossFade("Idle", 0.15f);
    }

    public void AutoMovePlaceHolderBox()
    {
        int count = 0;
        for (int i = 0; i < GameController.Instance.slots.Length; i++)
        {
            if (GameController.Instance.slots[i].currentBox != null && GameController.Instance.slots[i].currentBox.color == color && count < 3 
                && ((IsHasStar() && GameController.Instance.slots[i].currentBox.boxType.HasFlag(BoxType.Star)) 
                || (IsHasSlot(false) && !GameController.Instance.slots[i].currentBox.boxType.HasFlag(BoxType.Star))))
            {
                AudioManager.Instance.PlaySfx(AudioSfxID.sfx_vut2);
                HapticController.Instance.PlayHaptic();
                Box box = GameController.Instance.slots[i].currentBox;
                GameController.Instance.slots[i].currentBox = null;
                GameController.Instance.slots[i].isAvaiable = true;
                box.MoveToSlot(box.boxType.HasFlag(BoxType.Star)?GetSlotStar(): GetFreeSlot(), false, true);
                count += 1;
            }
        }
    }
}
