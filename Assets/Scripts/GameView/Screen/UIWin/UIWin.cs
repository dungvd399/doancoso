using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class UIWin : BaseView
{
    [SerializeField] private GameObject popupWin;
    [SerializeField] private Sprite[] spNumbers;
    [SerializeField] private Image[] imgNumbers;
    [SerializeField] private Image[] imgNumbers2;
    [SerializeField] private Button btnNextLevel;
    [SerializeField] private Button btnNextLevel2;
    [SerializeField] private Button btnNextLevel3;
    public GameObject[] unlockObj;
    public GameObject[] unlockObj2;
    public GameObject panelUnlock;
    public GameObject panelWin;
    public GameObject panelWin2;
    public TMP_Text percent;
    public int[] levelUnlock;
    private bool btnClicked;
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnNextLevel.onClick.AddListener(OnClickNextLevel);
        btnNextLevel2.onClick.AddListener(OnClickNextLevel);
        btnNextLevel3.onClick.AddListener(OnClickNextLevel);
        popupWin.SetActive(false);
        panelWin.gameObject.SetActive(false);
        panelWin2.gameObject.SetActive(false);
        panelUnlock.gameObject.SetActive(false);
    }

    private void OnClickNextLevel()
    {
        if (btnClicked) return;
        btnClicked = true;
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
        GameViewsManager.Instance.GetView<UIMainGameplay>().ChangeLevelText();
        Hide();
        //GameStateManager.Instance.GetCurrentStateClass<InGameState>().IsEndLevel = true;
        LevelManager.Instance.ReloadLevel();
    }

    public override void Show()
    {
        base.Show();
        int level = PlayerprefSave.Level;
        int number = level / 10;
        int number2 = level % 10;
        imgNumbers[0].sprite = spNumbers[number];
        imgNumbers[1].sprite = spNumbers[number2];
        
        //imgNumbers[0].SetNativeSize();
        //imgNumbers[1].SetNativeSize();

        imgNumbers[0].gameObject.SetActive(number > 0);

        imgNumbers2[0].sprite = spNumbers[number];
        imgNumbers2[1].sprite = spNumbers[number2];

        //imgNumbers2[0].SetNativeSize();
        //imgNumbers2[1].SetNativeSize();

        imgNumbers2[0].gameObject.SetActive(number > 0);

        DOVirtual.DelayedCall(1.5f, () =>
        {
            popupWin.SetActive(true);

            int id = 0;
            for (int i = 0; i < levelUnlock.Length; i++)
            {
                if(PlayerprefSave.Level <= levelUnlock[i])
                {
                    id = i;
                    break;
                }
            }
            if(PlayerprefSave.Level > levelUnlock[levelUnlock.Length - 1])
            {
                panelWin2.gameObject.SetActive(true);
                panelUnlock.gameObject.SetActive(false);
                panelWin.gameObject.SetActive(false);
            }
            else
            {
                panelWin.gameObject.SetActive(true);
                panelWin2.gameObject.SetActive(false);
                panelUnlock.gameObject.SetActive(false);
                btnNextLevel.gameObject.SetActive(false);

                int prev = 0;
                if (id > 0) prev = levelUnlock[id - 1];
                float _percent = ((float)level - (float)prev) / ((float)levelUnlock[id] - (float)prev);
                float prevPercent = _percent - 1f / ((float)levelUnlock[id] - (float)prev);

                foreach (var item in unlockObj2)
                {
                    item.gameObject.SetActive(false);
                }
                unlockObj2[id].gameObject.SetActive(true);
                var iconImage = unlockObj2[id].transform.GetChild(0).GetComponent<Image>();
                iconImage.fillAmount = prevPercent;
                percent.text = Mathf.RoundToInt(prevPercent) + "%";

                iconImage.DOFillAmount(_percent, 1f).SetEase(Ease.Linear).OnUpdate(() =>
                {
                    float tmp = iconImage.fillAmount * 100f;
                    percent.text = Mathf.RoundToInt(tmp) + "%";
                }).OnComplete(() =>
                {
                    if(_percent == 1)
                    {
                        panelWin.gameObject.SetActive(false);
                        panelUnlock.gameObject.SetActive(true);
                        btnNextLevel2.gameObject.SetActive(false);
                        StartCoroutine(ActiveBtnNext2());
                        foreach (var item in unlockObj)
                        {
                            item.gameObject.SetActive(false);
                        }
                        unlockObj[id].gameObject.SetActive(true);
                    }
                    else btnNextLevel.gameObject.SetActive(true);
                });
            }

            AudioManager.Instance.PlaySfx(AudioSfxID.sfx_level_up, 1, -1, false);
            PlayerprefSave.Level += 1;
        });
    }

    IEnumerator ActiveBtnNext2()
    {
        yield return new WaitForSeconds(2f);
        btnNextLevel2.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        popupWin.SetActive(false);
        base.Hide();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        btnClicked = false;
    }
}
