using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 바닥 종류가 있을 시
// 레거시파일

[CreateAssetMenu(menuName = "Scriptable Object/Audio/SFX Footstep")]
public class SFXFootstep : ScriptableObject
{
    [Serializable]
    public struct Footsteps
    {
        public SurfaceType type;
        public AudioClip[] clips;
    }

    public Footsteps[] footsteps;
    private Dictionary<SurfaceType, AudioClip[]> dic;

    private void OnEnable()
    {
        dic = new Dictionary<SurfaceType, AudioClip[]>();
        foreach (var a in footsteps)
        {
            dic[a.type] = a.clips;
        }
    }

    public AudioClip GetRandomClip(SurfaceType type)
    {


        return null;
    }
}
