using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PlayBGM : 배경음 재생 메서드
/// PlayOneShotSFX : 저빈도 SFX (UI 효과음, 카운트다운, 특수 이벤트, 게임 시작/종료, 라운드 승리/탈락)
/// PlayPooledSFX : 고빈도 SFX (캐릭터 각종 효과음, 충돌 효과음, 아이템?(아이템 갯수에 따라 저빈도 고빈도 갈듯))
/// </summary>
/// 

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("AudioSource")]
    [SerializeField] AudioSource _audioSource; // 이 오브젝트에 붙어 있는 AudioSource

    [Header("AudioMixer & Parameter")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] string BGMVolumeParam = "BGMVolume";
    [SerializeField] string SFXVolumeParam = "SFXVolume";
    [SerializeField] SFXLibrary sfxLibrary;

    [Header("SFX Source")]
    [SerializeField] AudioMixerGroup sfxGroup; // SFX 그룹

    const string PREF_BGM_VOLUME = "BGMVolumeLinear"; // 0~1 저장
    const string PREF_SFX_VOLUME = "SFXVolumeLinear"; // 0~1 저장

    //private float currentBGMLinear = 0.8f;
    //private float currentSFXLinear = 0.8f;

    // SFX 오브젝트 풀링
    private int poolSize = 60;
    private Queue<AudioSource> pool = new Queue<AudioSource>();
    //

    [Header("MainScene")]
    [SerializeField] AudioClip lobbyBGM;
    [SerializeField] AudioClip roomBGM;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Debug.LogWarning("중복 감지 제거됨");
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("AudioManager 생성");

        // SFX 오브젝트 풀링
        for (int i = 0; i < poolSize; i++)
        {
            var gObj = new GameObject("PooledAudioSource_" + i);
            gObj.transform.parent = transform;
            var src = gObj.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            gObj.SetActive(false);
            pool.Enqueue(src);
        }
        //
    }

    private void Start()
    {
        // 볼륨 로드 & 적용
        float savedBGM = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 0.8f);
        float savedSFX = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.8f);
        //currentBGMLinear = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 0.8f);
        //currentSFXLinear = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.8f);
        SetBGMVolumeLinear(savedBGM);
        SetSFXVolumeLinear(savedSFX);
    }

    // BGM . 씬마다 다른 음악 재생
    public void PlayBGM(AudioClip clip, float fadeInTime = 0f, float fadeOutTime = 0f, bool loop = true)
    {
        if (clip == null) return;
        if (_audioSource.clip == clip && _audioSource.isPlaying) return;

        _audioSource.DOKill();
        // 현재 음악 페이드 아웃
        _audioSource.DOFade(0f, fadeOutTime).OnComplete(() =>
        {
            _audioSource.clip = clip;
            _audioSource.loop = loop;
            _audioSource.Play();

            // 새 음악 페이드 인
            _audioSource.DOFade(1f, fadeInTime);
        });
    }

    // SFX . UI, 시스템 효과음, 캐릭터 및 아이템 효과음
    // Enum_SFXKey.cs가 있으니 굳이 ex) PlayerController.cs, Plate.cs 등등에 오디오 파일을 안넣어도
    // 2D 기준(UI 등등) AudioManager.Instance.PlayOneShotSFX / PlayPooledSFX(SFXKey.key값);
    // 3D 기준(플레이어, 오브젝트 소리 등등) AudioManager.Instance.PlayOneShotSFX / PlayPooledSFX(SFXKey.key값, transform.positon Or 어디든 위치 값)
    // 으로 실행.

    public int GetIndex(SFXKey key)
    {
        int index = sfxLibrary.GetRandomIndex(key);
        return index;
    }

    public void PlayLobbyBGM()
    {
        if (lobbyBGM == null) return;
        PlayBGM(lobbyBGM, 0.5f, 0.5f, true);
    }

    public void PlayRoomBGM()
    {
        if (roomBGM == null) return;
        PlayBGM(roomBGM, 0.5f, 0.5f, true);
    }

    #region 원샷 SFX
    public void PlayOneShotSFX(SFXKey key, Vector3? position = null, int index = -1)
    {
        //var clip = sfxLibrary.GetRandomClip(key);
        AudioClip clip;

        if (index == -1) // 랜덤
        {
            int randomIndex = sfxLibrary.GetRandomIndex(key);
            clip = sfxLibrary.GetClip(key, randomIndex);
        }
        else // index 사용
        {
            clip = sfxLibrary.GetClip(key, index);
        }

        if (clip == null) return;

        GameObject gObj = new GameObject("SFX_" + clip.name);
        gObj.transform.parent = transform;
        var src = gObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.outputAudioMixerGroup = sfxGroup;

        if (position.HasValue)
        {
            // 3D 사운드
            gObj.transform.position = position.Value;
            src.spatialBlend = 1f; // 3D
            // 테스트
            src.rolloffMode = AudioRolloffMode.Custom;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.9f, 0.5f), new Keyframe(0.3f, 0.3f), new Keyframe(1f, 0f));
            src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
            src.minDistance = 0.3f;  // float 값 이내는 항상 최대 볼륨
            src.maxDistance = 5f; // float 값 이상은 안 들림
            //
            // 기존
            //src.rolloffMode = AudioRolloffMode.Logarithmic; // 자연스럽게 감소
            //src.minDistance = 0.3f;  // float 값 이내는 항상 최대 볼륨
            //src.maxDistance = 5f; // float 값 이상은 안 들림
            //
        }
        else
        {
            // 2D 사운드 (UI, 시스템 효과음 등)
            src.spatialBlend = 0f;
        }

        // 피치, 볼륨 랜덤(선택)
        src.pitch = Random.Range(0.95f, 1.05f);
        src.volume = Random.Range(0.9f, 1.0f);

        src.Play();
        Destroy(gObj, clip.length);
    }
    #endregion
    //
    #region 오브젝트 풀링 SFX
    public void PlayPooledSFX(SFXKey key, Vector3? position = null, int index = -1)
    {
        //var clip = sfxLibrary.GetRandomClip(key);
        AudioClip clip;

        if (index == -1) // 랜덤
        {
            int randomIndex = sfxLibrary.GetRandomIndex(key);
            clip = sfxLibrary.GetClip(key, randomIndex);
        }
        else // index 사용
        {
            clip = sfxLibrary.GetClip(key, index);
        }
        if (clip == null) return;

        var src = GetSource();
        src.clip = clip;

        if (position.HasValue)
        {
            // 3D 사운드
            src.transform.position = position.Value;
            src.spatialBlend = 1f; // 3D
            // 테스트
            src.rolloffMode = AudioRolloffMode.Custom;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.9f, 0.5f), new Keyframe(0.3f, 0.3f), new Keyframe(1f, 0f));
            src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
            src.minDistance = 0.3f;  // float 값 이내는 항상 최대 볼륨
            src.maxDistance = 5f; // float 값 이상은 안 들림
            //
            // 기존
            //src.rolloffMode = AudioRolloffMode.Logarithmic; // 자연스럽게 감소
            //src.minDistance = 0.3f;  // float 값 이내는 항상 최대 볼륨
            //src.maxDistance = 5f; // float 값 이상은 안 들림
            //
        }
        else
        {
            // 2D 사운드 (UI, 시스템 효과음 등)
            src.spatialBlend = 0f;
        }

        // 피치, 볼륨 랜덤(선택)
        src.pitch = Random.Range(0.95f, 1.05f);
        src.volume = Random.Range(0.9f, 1.0f);

        src.Play();
        StartCoroutine(ReturnAfterDelay(src, clip.length));
    }

    private AudioSource GetSource()
    {
        if (pool.Count > 0)
        {
            var src = pool.Dequeue();
            src.gameObject.SetActive(true);
            return src;
        }
        else
        {
            // 풀 크기 초과 시 새로 생성 (필요하다면 제한 가능)
            var gObj = new GameObject("ExtraAudioSource");
            gObj.transform.parent = transform;
            var src = gObj.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            return src;
        }
    }

    private void ReturnSource(AudioSource src)
    {
        src.Stop();
        src.gameObject.SetActive(false);
        pool.Enqueue(src);
    }

    private IEnumerator ReturnAfterDelay(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnSource(src);
    }
    #endregion
    //
    #region 볼륨 값 저장 및 조절
    public void SetBGMVolumeLinear(float linear)
    {
        // 0 -> -80dB(거의 무음), 그 외는 로그 변환
        //currentBGMLinear = Mathf.Clamp01(linear);
        float dB = (linear <= 0.0001f) ? -80f : Mathf.Log10(linear) * 20f;
        mixer.SetFloat(BGMVolumeParam, dB);
        PlayerPrefs.SetFloat(PREF_BGM_VOLUME, Mathf.Clamp01(linear));
        PlayerPrefs.Save();
    }

    public void SetSFXVolumeLinear(float linear)
    {
        //currentSFXLinear = Mathf.Clamp01(linear);
        float dB = (linear <= 0.0001f) ? -80f : Mathf.Log10(linear) * 20f;
        mixer.SetFloat(SFXVolumeParam, dB);
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, Mathf.Clamp01(linear));
        PlayerPrefs.Save();
    }

    public float GetBGMVolumeLinear()
    {
        return PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 0.8f);
    }
    //public float GetBGMVolumeLinear() => currentBGMLinear;

    public float GetSFXVolumeLinear()
    {
        return PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.8f);
    }
    //public float GetSFXVolumeLinear() => currentSFXLinear;
    #endregion

    private void OnDrawGizmosSelected()
    {
        var src = GetComponent<AudioSource>();
        if (src == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, src.minDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, src.maxDistance);
    }
}
