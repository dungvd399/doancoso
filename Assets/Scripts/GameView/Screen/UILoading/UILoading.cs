using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : BaseView
{
    [SerializeField] private TextMeshProUGUI progressText;
    public Image progress;
    public override void Initialize(CancellationToken propagateCancellationToken)
    {
        base.Initialize(propagateCancellationToken);
    }
    public override void Show()
    {
        base.Show();
        StartProgress(3).Forget();

    }
    public override void Hide()
    {
        base.Hide();
    }
    public void RunNumber()
    {
    }
  
    public async UniTaskVoid StartProgress(float duration)
    {
        GameViewsManager.Instance.GetView<UIMainMenu>().gameObject.SetActive(true);
        GameViewsManager.Instance.GetView<UIMainMenu>().btnPlay.gameObject.SetActive(false);
        float elapsed = 0f;
        progress.fillAmount = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int percent = Mathf.RoundToInt(t * 100f);

            if (progressText != null)
            {
                progressText.text = $"{percent}%";
                progress.fillAmount = t;
            }

            await UniTask.Yield(); // Đợi frame tiếp theo
        }

        // Đảm bảo kết thúc ở 100%
        if (progressText != null)
            progressText.text = "100%";
        progress.fillAmount = 1;
        GameViewsManager.Instance.GetView<UIMainMenu>().btnPlay.gameObject.SetActive(true);
        GameViewsManager.Instance.GetView<UIMainMenu>().btnPlay.transform.localScale = Vector3.zero;
        GameViewsManager.Instance.GetView<UIMainMenu>().btnPlay.transform.DOScale(1, 1f).SetEase(Ease.OutBack);
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_btn_play_click);
        Debug.Log("xx");
    }

}
