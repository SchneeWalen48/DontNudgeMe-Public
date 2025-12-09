using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 화면전환
// FadePlay는 페이드인아웃 효과
// RadialPlay는 시계방향으로 검은화면 사라지는 효과
// 둘중 원하는거 쓰시거나 원하는 전환 있으면 추가가능

public class ScreenFader : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] Image radialImg;

    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float holdDuration = 1f;

    private void Awake()
    {
            radialImg.fillAmount = 1f;
            img.gameObject.SetActive(false);
            radialImg.gameObject.SetActive(false);
    }

    private void Start()
    {
        //FadePlay(fadeInDuration, fadeOutDuration, holdDuration);
        RadialPlay(1f);
    }

    // 나중에 public으로 바꾸는게 좋을 듯
    private void FadePlay(float fadeInD, float fadeOutD, float holdD) 
    {
        if (img == null) return;
        img.gameObject.SetActive(true);
        img.DOKill();

        // 페이드인-딜레이-페이드아웃
        Sequence seq = DOTween.Sequence();
        seq.Append(img.DOFade(1, fadeInD).SetEase(Ease.Linear));
        seq.AppendInterval(holdD);
        seq.Append(img.DOFade(0, fadeOutD).SetEase(Ease.InQuad));
        seq.Play();
    }

    private void RadialPlay(float radialD)
    {
        if (radialImg == null) return;
        radialImg.gameObject.SetActive(true);

        radialImg.DOFillAmount(0f, radialD).SetEase(Ease.InOutCubic);
    }

}
