using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Audio/SFXLibrary")]
public class SFXLibrary : ScriptableObject
{
    [Serializable]
    public struct SFX
    {
        public SFXKey type;
        public AudioClip[] clips;
    }

    public SFX[] sfxs;
    private Dictionary<SFXKey, AudioClip[]> dic;

    private void OnEnable()
    {
        dic = new Dictionary<SFXKey, AudioClip[]>();
        foreach (var a in sfxs)
        {
            dic[a.type] = a.clips;
        }
    }

    public int GetRandomIndex(SFXKey key)
    {
        if (dic.TryGetValue(key, out var clips) && clips.Length > 0)
        {
            return UnityEngine.Random.Range(0, clips.Length);
        }
        return -1;
    }

    public AudioClip GetClip(SFXKey key, int index)
    {
        if (dic.TryGetValue(key, out var clips) && index >= 0 && index < clips.Length)
        {
            return clips[index];
        }

        return null;
    }
}
