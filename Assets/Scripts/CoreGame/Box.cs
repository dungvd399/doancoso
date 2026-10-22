using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Box : MonoBehaviour, IPoolable
{
    public GameColor color;
    public BoxType boxType;
    public Transform transformNormal;
    public Transform transformLarge;
    public GameObject[] models;
    public GameObject[] modelsLarge;
    GameObject[] modelsRef;
    GameObject secretModelRef;
    public GameObject secretModel;
    public GameObject secretModelLarge;
    public bool canPlay = false;
    public int foodCount = 10;
    public TMP_Text foodTxt;
    public int columnId = 0;
    public int rowId = 0;
    public int connectId;
    public LineRenderer line;
    public Box connectBox;
    public Transform lid1;
    public Transform lid2;
    public Transform boxRender;
    public Material normalMat;
    public Material starMat;
    public Material normalMatLarge;
    public Material starMatLarge;
    public List<Food> listFood = new();
    public bool canCheck = false;
    public void OnSpawn()
    {
        SetCannotPlay();
        foreach (var model in models)
        {
            model.gameObject.SetActive(false);
        }
        foreach (var model in modelsLarge)
        {
            model.gameObject.SetActive(false);
        }
        secretModel.gameObject.SetActive(false);
        secretModelLarge.gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;
        foodTxt.color = new Color(1, 1, 1, 0.08f);
        isReveal = false;
        transformNormal.rotation = Quaternion.identity;
        transformLarge.rotation = Quaternion.identity;
        canCheck = false;
        transformNormal.GetComponent<QuickOutline>().enabled = true;
        transformLarge.GetComponent<QuickOutline>().enabled = true;
        connectId = 0;
        connectBox = null;
        line.gameObject.SetActive(false);
    }

    public void OnDespawn()
    {
        foreach (var item in listFood)
        {
            PoolManager.Instance.Despawn("Food", item.gameObject);
        }
        listFood.Clear();
    }

    public void InvalidShake(Transform target)
    {
        if (isMoving) return;
        target.DOKill();
        Debug.Log("Shake");
        target.localRotation = Quaternion.identity;
        Quaternion origin = target.localRotation;
        target.DOShakeRotation(
            duration: 0.3f,
            strength: new Vector3(15, 0, 0), // shake on Z (2D). Change axis if 3D
            vibrato: 15,
            randomness: 0,
            fadeOut: true
        ).OnComplete(() =>
        {
            target.localRotation = Quaternion.identity; // snap back exactly
        });
    }

    public void Setup(GameColor _color)
    {
        color = _color;
        if (!boxType.HasFlag(BoxType.Large))
        {
            modelsRef = models;
            secretModelRef = secretModel;
        }
        else
        {
            modelsRef = modelsLarge;
            secretModelRef = secretModelLarge;
        }
        boxRender = modelsRef[(int)color].transform.GetChild(0);
        lid1 = modelsRef[(int)color].transform.GetChild(1);
        lid2 = modelsRef[(int)color].transform.GetChild(2);
        lid1.transform.localRotation = Quaternion.Euler(0, 0, boxType.HasFlag(BoxType.Large) ? 290 : 110);
        lid2.transform.localRotation = Quaternion.Euler(0, 0, boxType.HasFlag(BoxType.Large) ? 70 : 250);

        if (boxType.HasFlag(BoxType.Star))
        {
            boxRender.GetComponent<Renderer>().material = boxType.HasFlag(BoxType.Large) ? starMatLarge : starMat;
            lid1.GetComponent<Renderer>().material = boxType.HasFlag(BoxType.Large) ? starMatLarge : starMat;
            lid2.GetComponent<Renderer>().material = boxType.HasFlag(BoxType.Large) ? starMatLarge : starMat;
        }
        else
        {
            boxRender.GetComponent<Renderer>().material = boxType.HasFlag(BoxType.Large) ? normalMatLarge : normalMat;
            lid1.GetComponent<Renderer>().material = boxType.HasFlag(BoxType.Large) ? normalMatLarge : normalMat;
            lid2.GetComponent<Renderer>().material = boxType.HasFlag(BoxType.Large) ? normalMatLarge : normalMat;
        }

        if (!boxType.HasFlag(BoxType.Large))
        {
            transform.localScale = new Vector3(1, 1, 1);
            foodCount = 10;
        }
        else
        {
            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            foodCount = 20;
        }
        if (!boxType.HasFlag(BoxType.Secret))
        {
            modelsRef[(int)color].SetActive(true);
            secretModelRef.SetActive(false);
            foodTxt.gameObject.SetActive(true);
            UpdateFoodCountText();
        }
        else
        {
            foreach (var item in modelsRef)
            {
                item.SetActive(false);
            }
            secretModelRef.SetActive(true);
            foodTxt.gameObject.SetActive(true);
            UpdateFoodCountText();
        }
        //starObj.SetActive(boxType.HasFlag(BoxType.Star));
        if (boxType.HasFlag(BoxType.Connect))
        {
            foreach (var item in LevelManager.Instance.ListAllBox)
            {
                if (item.boxType.HasFlag(BoxType.Connect) && item != this && item.connectId == connectId)
                {
                    connectBox = item;
                    item.connectBox = this;
                }
            }
            line.gameObject.SetActive(true);
            line.positionCount = 2;
            line.useWorldSpace = true;
        }
        else
        {
            line.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (boxType.HasFlag(BoxType.Connect) && connectBox != null)
        {
            line.SetPosition(0, transform.position + new Vector3(0, 0.05f, 0));
            line.SetPosition(1, connectBox.transform.position + new Vector3(0, 0.05f, 0));
        }
    }

    public void DisableLine()
    {
        line.gameObject.SetActive(false);
        connectBox.line.gameObject.SetActive(false);
    }
    public void SetCanPlay()
    {
        canPlay = true;
        if (boxType.HasFlag(BoxType.Secret))
        {
            RevealSecret();
        }
        OutlineController.Instance.AddOutline(transformNormal.gameObject);
        OutlineController.Instance.AddOutline(transformLarge.gameObject);
        foodTxt.color = new Color(1, 1, 1, 1);
        AnimReadyUse();
        if (lid1 != null) lid1.GetComponent<Renderer>().material.SetFloat("_Brightness", 1.1f);
        if (lid2 != null) lid2.GetComponent<Renderer>().material.SetFloat("_Brightness", 1.1f);
        if (boxRender != null) boxRender.GetComponent<Renderer>().material.SetFloat("_Brightness", 1.1f);
    }

    public void SetCanPlay2()
    {
        //canPlay = true;
        if (boxType.HasFlag(BoxType.Secret))
        {
            RevealSecret();
        }
        OutlineController.Instance.AddOutline(transformNormal.gameObject);
        OutlineController.Instance.AddOutline(transformLarge.gameObject);
        foodTxt.color = new Color(1, 1, 1, 1);
        AnimReadyUse();
        if (lid1 != null) lid1.GetComponent<Renderer>().material.SetFloat("_Brightness", 1.1f);
        if (lid2 != null) lid2.GetComponent<Renderer>().material.SetFloat("_Brightness", 1.1f);
        if (boxRender != null) boxRender.GetComponent<Renderer>().material.SetFloat("_Brightness", 1.1f);
    }

    public void SetCannotPlay()
    {
        canPlay = false;
        OutlineController.Instance.AddOutline2(transformNormal.gameObject, boxType.HasFlag(BoxType.Secret) ? Color.black : LevelManager.Instance.colorRef[(int)color], 14);
        OutlineController.Instance.AddOutline2(transformLarge.gameObject, boxType.HasFlag(BoxType.Secret) ? Color.black : LevelManager.Instance.colorRef[(int)color], 14);
        RemoveAnimUse();
        if (lid1 != null) lid1.GetComponent<Renderer>().material.SetFloat("_Brightness", 0.5f);
        if (lid2 != null) lid2.GetComponent<Renderer>().material.SetFloat("_Brightness", 0.5f);
        if (boxRender != null) boxRender.GetComponent<Renderer>().material.SetFloat("_Brightness", 0.5f);
    }

    public void SetCannotPlay2()
    {
        canPlay = false;
        //transformNormal.GetComponent<QuickOutline>().enabled = false;
        //transformLarge.GetComponent<QuickOutline>().enabled = false;
        RemoveAnimUse();
    }

    Tween move;
    Tween move2;
    Tween move3;
    Tween move4;
    public void MoveToSlot(Slot slot, bool isMoveToPlaceHolder = false, bool isPlaceHolderMoveToCar = false)
    {
        if (move != null)
        {
            move.Kill();
            move2.Kill();
            move3.Kill();
            move4.Kill();
            GameController.Instance.listBoxCheckAction.Remove(gameObject);
            GameController.Instance.listBoxMoving.Remove(this);
        }
        GameController.Instance.listBoxCheckAction.Add(gameObject);
        GameController.Instance.listBoxMoving.Add(this);
        SetCannotPlay2();
        if(!isMoveToPlaceHolder)
        {
            transformNormal.GetComponent<QuickOutline>().enabled = false;
            transformLarge.GetComponent<QuickOutline>().enabled = false;
        }
        else
        {
            OutlineController.Instance.AddOutline2(transformNormal.gameObject, boxType.HasFlag(BoxType.Secret) ? Color.black : LevelManager.Instance.colorRef[(int)color], 4);
            OutlineController.Instance.AddOutline2(transformLarge.gameObject, boxType.HasFlag(BoxType.Secret) ? Color.black : LevelManager.Instance.colorRef[(int)color], 4);
        }
            slot.isAvaiable = false;
        slot.currentBox = this;
        transform.SetParent(slot.transform);
        //transform.DOScale(new Vector3(1.1f, 0.65f, 1.1f), 0.5f).SetEase(Ease.Linear);

        if(!isMoveToPlaceHolder)
        {
            foodTxt.transform.SetParent(transform);
            if(boxType.HasFlag(BoxType.Large))
            {
                transformLarge.DOLocalRotate(new Vector3(0, -90, 0), 0.5f).SetEase(Ease.Linear);
            }
            else transformNormal.DOLocalRotate(new Vector3(0, -90, 0), 0.5f).SetEase(Ease.Linear);
        }
        transform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.Linear);

        move = transform.DOLocalJump((isMoveToPlaceHolder ? new Vector3(0, 0.3f, 0) : new Vector3(0, 0.1f, 0)) - new Vector3(0, 0.15f, 0), 4, 1, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            GameController.Instance.listBoxMoving.Remove(this);
            move4 = transform.DOScale(new Vector3(1.1f, 0.65f, 1.1f), 0.1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                move2 = transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f).SetEase(Ease.Linear);
            });

            move3 = transform.DOLocalMoveY(transform.localPosition.y + 0.2f, 0.15f).SetEase(Ease.Linear);
            if (foodCount > 0)
            {
                GameController.Instance.listBoxCheck.Add(this);
                GameController.Instance.Check(this);
            }
            else
            {
                GameController.Instance.listBoxCheckAction.Remove(gameObject);
                if(GameController.Instance.listFoodMoving.Count <= 0)
                GameController.Instance.CheckCompleteCar();
            }
            //move = null;

            if(isMoveToPlaceHolder)
            {
                if (GameController.Instance.NumSLotAvaialbe() == 1)
                {
                    FlashRed(GameController.Instance.GetFreeSlot().GetComponent<Renderer>());
                }
                slot.NudgeDown();
            }
            else
            {
                slot.transform.parent.GetComponent<Car>().NudgeDown();
            }
        });
        if (!isPlaceHolderMoveToCar)
            LevelManager.Instance.ActionBoxColumn(columnId);
    }   

    public void FlashRed(Renderer rd)
    {
        Material mat = rd.material;
        Color redTransparent = new Color(1f, 0.6f, 0.6f, 0.2f);
        Color origin = mat.color;

        mat.DOKill();
        mat.color = Color.white;
        mat.DOColor(redTransparent, 0.25f).SetEase(Ease.Linear)
           .SetLoops(6, LoopType.Yoyo)
           .OnComplete(() =>
           {
               mat.color = Color.white;
           });
    }
    bool isMoving;
    public void MoveUp()
    {
        if (LevelManager.Instance.boxColumnData[columnId].Peek() == this)
        {
            isMoving = true;
            transform.DOLocalMoveZ(0, 0.5f).OnComplete(() =>
            {
                isMoving = false;
                canPlay = true;
            });
        }
        else
        {
            isMoving = true;
            transform.DOLocalMoveZ(-1 - LevelManager.Instance.boxDistance.y * (-LevelManager.Instance.boxColumnData[columnId].Peek().rowId + rowId), 0.5f).OnComplete(() =>
            {
                isMoving = false;
            });
        }
    }

    public void LidAnim()
    {
        canCheck = true;
        lid1.DOLocalRotate(boxType.HasFlag(BoxType.Large) ? new Vector3(0, 0, 180 + 360) : new Vector3(0, 0, 360), 0.35f, RotateMode.FastBeyond360);//
        lid2.DOLocalRotate(boxType.HasFlag(BoxType.Large) ? new Vector3(0, 0, -180) : Vector3.zero, 0.35f, RotateMode.FastBeyond360);// dong vao
    }

    public bool IsFull()
    {
        return foodCount == 0;
    }

    public void UpdateFoodCountText(int value = -1)
    {
        if(boxType.HasFlag(BoxType.Secret) && !isReveal)
        {
            foodTxt.text = "?";
        }
        else if(value!=-1)
        {
            if (value > 0)
            {
                foodTxt.text = value.ToString();
            }
            else
            {
                foodTxt.gameObject.SetActive(false);
            }
        }
        else if (foodCount > 0)
        {
            foodTxt.text = foodCount.ToString();
        }
        else
        {
            foodTxt.gameObject.SetActive(false);
        }
    }
    bool isReveal = false;
    public void RevealSecret()
    {
        isReveal = true;
        modelsRef[(int)color].SetActive(true);
        secretModelRef.SetActive(false);
        foodTxt.gameObject.SetActive(true);
        UpdateFoodCountText();
    }

    Tween scale1;
    Tween scale2;
    Tween scale3;
    Tween scale4;

    private void AnimReadyUse()
    {
        float time = Random.Range(0.9f, 1.1f);
        var c0 = transformNormal;
        var c1 = transformLarge;

        scale1 = c0.DOScaleY(1.05f, time).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        scale2 = c0.DOLocalMoveY(0.1f, time).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        scale3 = c1.DOScaleY(1.05f, time).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        scale4 = c1.DOLocalMoveY(0.1f, time).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        if(boxType.HasFlag(BoxType.Large))
        {
            foodTxt.transform.SetParent(c1);
        }
        else
        {
            foodTxt.transform.SetParent(c0);
        }
    }

    private void RemoveAnimUse()
    {
        if (scale1 != null) scale1.Kill();
        if (scale2 != null) scale2.Kill();
        if (scale3 != null) scale3.Kill();
        if (scale4 != null) scale4.Kill();

        var c0 = transformNormal;
        var c1 = transformLarge;

        c0.localScale = Vector3.one;
        c1.localScale = Vector3.one;
        c0.localPosition = Vector3.zero;
        c1.localPosition = Vector3.zero;
    }

    public void NudgeDown()
    {
        Transform t = boxType.HasFlag(BoxType.Large)? transformLarge:transformNormal;

        t.DOKill();
        t.DOLocalMoveY(-0.1f, 0.01f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                t.DOLocalMoveY(0f, 0.01f)
                 .SetEase(Ease.Linear);
            });
    }
}
