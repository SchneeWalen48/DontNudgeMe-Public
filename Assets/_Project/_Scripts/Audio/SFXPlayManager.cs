using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SFX오디오 실행은 Static_SFXEvents.cs에서 재생.
// key(음악 뭐할지), pooled(오브젝트 풀링인지), position(2d 소리할지 3d 소리할지), allClients(RpcTarget Others인지 All인지)
public class SFXPlayManager : MonoBehaviourPun
{
    private void OnEnable()
    {
        SFXEvents.OnPlaySFX += HandlePlaySFX;
    }

    private void OnDisable()
    {
        SFXEvents.OnPlaySFX -= HandlePlaySFX;
    }

    private void HandlePlaySFX(SFXKey key, Vector3 pos, bool pooled, bool allClients)
    {
        PlaySFX(key, pooled, pos, networked: true, allClients);
    }

    public void PlaySFX(SFXKey key, bool pooled = false, Vector3? pos = null, bool networked = true, bool allClients = false)
    {
        int index = AudioManager.Instance.GetIndex(key);
        if (!allClients)
        {
            // 로컬 즉시 재생
            if (pooled) AudioManager.Instance.PlayPooledSFX(key, pos, index);
            else AudioManager.Instance.PlayOneShotSFX(key, pos, index);
        }

        if (networked)
        {
            RpcTarget target = allClients ? RpcTarget.All : RpcTarget.Others;
            photonView.RPC(nameof(RPC_PlaySFX), target, (int)key, index, pooled, pos ?? Vector3.zero, pos.HasValue);
        }
    }

    [PunRPC]
    private void RPC_PlaySFX(int keyInt, int index, bool pooled, Vector3 pos, bool hasPos)
    {
        SFXKey key = (SFXKey)keyInt;
        if (pooled) AudioManager.Instance.PlayPooledSFX(key, hasPos ? pos : (Vector3?)null, index);
        else AudioManager.Instance.PlayOneShotSFX(key, hasPos ? pos : (Vector3?)null, index);
    }
}

