using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI 오디오 슬라이더
// 이 스크립트를 넣고 BGM, SFX 맞게 설정

public class AudioVolumeSlider : MonoBehaviour
{
    public enum VolumeType { BGM, SFX }

    [SerializeField] Slider slider;
    [SerializeField] VolumeType volumeType;

    private void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // 저장된 값으로 슬라이더 초기화 (씬 재진입 시에도 유지)
        if (AudioManager.Instance == null) return;

        switch (volumeType)
        {
            case VolumeType.BGM:
                slider.SetValueWithoutNotify(AudioManager.Instance.GetBGMVolumeLinear());
                break;
            case VolumeType.SFX:
                slider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolumeLinear());
                break;
        }


    }

    public void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance == null) return;

        switch (volumeType)
        {
            case VolumeType.BGM:
                AudioManager.Instance.SetBGMVolumeLinear(value);
                break;
            case VolumeType.SFX:
                AudioManager.Instance.SetSFXVolumeLinear(value);
                break;
        }

    }
}
