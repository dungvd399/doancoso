using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Camera camera;
    public GameColor mainColor;
    public LayerMask boxLayer;
    public Slot[] slots;
    public List<Box> listBoxCheck = new List<Box>();
    public List<GameObject> listBoxCheckAction = new List<GameObject>();
    public List<GameObject> listBoxCheckAction2 = new List<GameObject>();
    public List<Food> listFoodMoving = new List<Food>();
    public List<Box> listBoxMoving = new List<Box>();
    public bool isChecking = false;
    public Button btnReplay;

    public void Reset()
    {
        LevelManager.Instance.transform.DOKill();
        LevelManager.Instance.StopAllCoroutines();
        StopAllCoroutines();
        foreach (var slot in slots)
        {
            slot.isAvaiable = true;
            if (slot.currentBox != null)
            {
                PoolManager.Instance.Despawn("Box", slot.currentBox.gameObject);
                slot.currentBox = null;
            }
            slot.GetComponent<Renderer>().material.color = Color.white;
        }
        listBoxCheckAction.Clear();
        listBoxCheckAction2.Clear();

        foreach (var item in listFoodMoving)
        {
            DOTween.Kill(item.transform);
            item.StopAllCoroutines();
            PoolManager.Instance.Despawn("Food", item.gameObject);
        }
        listFoodMoving.Clear();
        foreach (var item in listBoxMoving)
        {
            DOTween.Kill(item.transform);
            item.StopAllCoroutines();
            PoolManager.Instance.Despawn("Box", item.gameObject);
        }
        listBoxMoving.Clear();
        listBoxCheck.Clear();
        isChecking = false;
        btnReplay.interactable = true;
    }
    public GameObject uiSetting;

    private void Update()
    {
        if (uiSetting.activeSelf) return;
        if (LevelManager.Instance.isWin || LevelManager.Instance.isLose) return;
        if (GameStateManager.Instance.CurrentState != EState.InGame) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100, boxLayer))
            {
                Box box = hit.collider.GetComponent<Box>();
                if (!box.canPlay)
                {
                    if (!box.transform.parent.GetComponent<Slot>())
                    {
                        box.InvalidShake(box.transform);
                        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_move_fail);
                    }
                    return;
                }
                if (LevelManager.Instance.mainCar.color == box.color && LevelManager.Instance.mainCar.canUse && (LevelManager.Instance.mainCar.IsHasSlot(box.boxType.HasFlag(BoxType.Star)))
                    && ((!LevelManager.Instance.mainCar.IsHasStar() && !box.boxType.HasFlag(BoxType.Star)) || LevelManager.Instance.mainCar.IsHasStar())
                    && (!box.boxType.HasFlag(BoxType.Connect) || (box.boxType.HasFlag(BoxType.Connect) && (box.connectBox.canPlay || (box.connectBox.columnId == box.columnId && box.connectBox.rowId == box.rowId + 1)))))
                {
                    LevelManager.Instance.uiTut.gameObject.SetActive(false);
                    AudioManager.Instance.PlaySfx(AudioSfxID.sfx_pick);
                    HapticController.Instance.PlayHaptic();
                    box.MoveToSlot(box.boxType.HasFlag(BoxType.Star) ? LevelManager.Instance.mainCar.GetSlotStar() : LevelManager.Instance.mainCar.GetFreeSlot());
                    if (box.boxType.HasFlag(BoxType.Connect)) box.DisableLine();
                    if(box.boxType.HasFlag(BoxType.Connect) && (box.connectBox.canPlay || (box.connectBox.columnId == box.columnId && box.connectBox.rowId == box.rowId + 1)))
                    {
                        if (LevelManager.Instance.mainCar.GetFreeSlot() != null && LevelManager.Instance.mainCar.color == box.connectBox.color
                            && ((LevelManager.Instance.mainCar.IsHasStar() && box.connectBox.boxType.HasFlag(BoxType.Star)) || (LevelManager.Instance.mainCar.IsHasSlot(false) && !box.connectBox.boxType.HasFlag(BoxType.Star))))
                            box.connectBox.MoveToSlot(box.connectBox.boxType.HasFlag(BoxType.Star) ? LevelManager.Instance.mainCar.GetSlotStar() : LevelManager.Instance.mainCar.GetFreeSlot());
                        else if (GetFreeSlot() != null)
                            box.connectBox.MoveToSlot(GetFreeSlot(), true);
                        else {
                            box.connectBox.boxType |= BoxType.Normal;
                            box.connectBox.boxType &= ~BoxType.Connect;
                        }
                    }
                }
                else if (IsHasSlot() && (!box.boxType.HasFlag(BoxType.Connect) || (box.boxType.HasFlag(BoxType.Connect) && (box.connectBox.canPlay || (box.connectBox.columnId == box.columnId && box.connectBox.rowId == box.rowId + 1)))))
                {
                    LevelManager.Instance.uiTut.gameObject.SetActive(false);
                    AudioManager.Instance.PlaySfx(AudioSfxID.sfx_pick);
                    HapticController.Instance.PlayHaptic();
                    box.MoveToSlot(GetFreeSlot(), true);
                    if (box.boxType.HasFlag(BoxType.Connect)) box.DisableLine();
                    if (box.boxType.HasFlag(BoxType.Connect) && (box.connectBox.canPlay || (box.connectBox.columnId == box.columnId && box.connectBox.rowId == box.rowId + 1)))
                    {
                        if(LevelManager.Instance.mainCar.GetFreeSlot() != null && LevelManager.Instance.mainCar.color == box.connectBox.color
                            && ((LevelManager.Instance.mainCar.IsHasStar() && box.connectBox.boxType.HasFlag(BoxType.Star)) || (LevelManager.Instance.mainCar.IsHasSlot(false) && !box.connectBox.boxType.HasFlag(BoxType.Star))))
                        {
                            box.connectBox.MoveToSlot(box.connectBox.boxType.HasFlag(BoxType.Star) ? LevelManager.Instance.mainCar.GetSlotStar() : LevelManager.Instance.mainCar.GetFreeSlot());
                        }
                        else if (GetFreeSlot() != null)
                            box.connectBox.MoveToSlot(GetFreeSlot(), true);
                        else
                        {
                            box.connectBox.boxType |= BoxType.Normal;
                            box.connectBox.boxType &= ~BoxType.Connect;
                        }
                    }
                }
                else
                {
                    box.InvalidShake(box.transform);
                    AudioManager.Instance.PlaySfx(AudioSfxID.sfx_move_fail);
                }
            }
        }
    }

    public int NumSLotAvaialbe()
    {
        int count = 0;
        foreach (var item in slots)
        {
            if (item.isAvaiable)
            {
                count += 1;
            }
        }
        return count;
    }

    public bool IsHasSlot()
    {
        bool check = false;
        foreach (var item in slots)
        {
            if (item.isAvaiable)
            {
                check = true;
            }
        }
        return check;
    }

    public Slot GetFreeSlot()
    {
        foreach (Slot item in slots)
        {
            if (item.isAvaiable) return item;
        }
        return null;
    }

    public void Check(Box box)
    {
        if (isChecking)
        {
            listBoxCheckAction.Remove(box.gameObject);
            return;
        }
        else
        {
            StartCoroutine(IE_Check(box));
        }
    }
    bool check = false;
    Tween Move1;
    Tween Move2;
    IEnumerator IE_Check(Box box)
    {
        isChecking = true;
        check = true;
        while (check)
        {
            check = false;
            for (int i = 0; i < LevelManager.Instance.foodColumnData.Count; i++) // duyet vet can xem co thuc an nao trung mau voi box can check hay khong
            {
                List<Box> listTmp = listBoxCheck;
                for (int j = 0; j < listTmp.Count; j++)
                {
                    if (LevelManager.Instance.foodColumnData[i].Count > 0)
                    {
                        if (LevelManager.Instance.foodColumnData[i].Peek().color == listTmp[j].color && listTmp[j].foodCount > 0) // check box va food cung mau
                        {
                            Food food = LevelManager.Instance.foodColumnData[i].Pop();
                            listFoodMoving.Add(food);
                            listTmp[j].foodCount -= 1;
                            food.ActionMove(listTmp[j], listTmp[j].foodCount == 0, listTmp[j].foodCount); // di chuyen food
                            Transform tmp = LevelManager.Instance.foodColumn[i];
                            tmp.transform.DOKill();
                            if (LevelManager.Instance.foodColumnData[i].Count > 0) // di chuyen cot cua food vua action
                            {
                                float des = -LevelManager.Instance.foodDistance * LevelManager.Instance.foodColumnData[i].Peek().RowId;

                                tmp.DOLocalMoveY(des, 5f).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(() =>
                                {
                                    tmp.DOLocalJump(tmp.localPosition, 0.17f, 1, 0.2f).SetEase(Ease.Linear);
                                });
                            }
                            if (listTmp[j].foodCount == 0)
                            {
                                listBoxCheck.Remove(listTmp[j]);
                                StartCoroutine(IE_CheckCompleteCar());
                            }
                            check = true;
                            yield return new WaitForSeconds(0.035f);
                            break;
                        }
                    }
                }
            }
        }

        isChecking = false;
        listBoxCheckAction.Remove(box.gameObject);
        yield return new WaitForSeconds(0.6f);
        if(listBoxCheckAction.Count == 0)
        CheckLose();
    }

    public void CheckCompleteCar()
    {
        //if (listFoodMoving.Count > 0) return;
        if (LevelManager.Instance.mainCar != null && LevelManager.Instance.mainCar.CheckFull())
        {
            LevelManager.Instance.mainCar.MoveOut(); // xe o giua
            LevelManager.Instance.mainCar = LevelManager.Instance.backCar; //xe phia sau
            if (LevelManager.Instance.mainCar != null)
            {
                LevelManager.Instance.mainCar.MoveToMain();
            }
            if(!LevelManager.Instance.isRandom)
            LevelManager.Instance.SpawnNextCar();//tao xe moi o dang sau
            else LevelManager.Instance.SpawnNextCarRandom();
        }
        else
        {
            //CheckLose();
        }
    }

    IEnumerator IE_CheckCompleteCar()
    {
        yield return new WaitForSeconds(1f);
        CheckCompleteCar();
    }

    public void CheckLose()
    {
        if (LevelManager.Instance.isWin || LevelManager.Instance.isLose) return;
        if (listBoxCheckAction.Count > 0) return;
        if (listFoodMoving.Count > 0) return;
        if (listBoxMoving.Count > 0) return;
        if (LevelManager.Instance.mainCar.CheckFull()) return;
        bool check = false;
        for (int i = 0; i < LevelManager.Instance.boxColumnData.Count; i++)
        {
            if (LevelManager.Instance.boxColumnData[i].Count > 0)
            {
                Box tmp = LevelManager.Instance.boxColumnData[i].Peek();
                if (LevelManager.Instance.mainCar.IsHasSlot(tmp.boxType.HasFlag(BoxType.Star)) && LevelManager.Instance.mainCar.color == tmp.color)
                {
                    check = true;
                }
                else if (IsHasSlot())
                {
                    check = true;
                }
            }
        }
        bool check2 = false;
        for (int i = 0; i < LevelManager.Instance.boxColumnData.Count; i++)
        {
            if (LevelManager.Instance.boxColumnData[i].Count > 0)
            {
                check2 = true;
            }
        }
        if (!check && check2 && (LevelManager.Instance.mainCar == null || !LevelManager.Instance.mainCar.isMoving) && (LevelManager.Instance.backCar == null || !LevelManager.Instance.backCar.isMoving))
        {
            btnReplay.interactable = false;
            UIMainGameplay.Instance.OnShowLose();
        }
    }

    public bool IsAllBoxUse()
    {
        bool check = true;
        foreach (var item in LevelManager.Instance.boxColumnData)
        {
            if (item.Count > 0) check = false;
        }
        return check;
    }
}
