using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRetry : BaseView
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnYes;
    [SerializeField] private Button btnNo;

    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
        btnClose.onClick.AddListener(OnClickClose);
        btnNo.onClick.AddListener(OnClickClose);
        btnYes.onClick.AddListener(OnClickYes);
    }
   
    public override void Show()
    {
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_appear_popup, 1, -1, false);
        base.Show();
    }
    public override void Hide()
    {
        base.Hide();
    }
    private void OnClickClose()
    {
        HideByScale();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }

    public void OnClickYes()
    {
        LevelManager.Instance.ReloadLevel();
        HideByScale();
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_positive);
    }
}
