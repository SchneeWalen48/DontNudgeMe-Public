using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public static class SFXEvents
{
    // key(음악 뭐할지), position(2d 소리할지 3d 소리할지), pooled(오브젝트 풀링인지), allClients(Others인지 All인지)
    public static event Action<SFXKey, Vector3, bool, bool> OnPlaySFX;

    public static void Raise(SFXKey key, Vector3 pos, bool pooled = true, bool allClients = false)
    {
        OnPlaySFX?.Invoke(key, pos, pooled, allClients);
    }
}
