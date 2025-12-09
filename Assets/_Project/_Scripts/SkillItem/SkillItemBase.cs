using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillItemBase : MonoBehaviourPun
{
    // 아이템을 사용한 플레이어
    protected PhotonView ownerView;

    // 아이템 초기화
    public virtual void Init(PhotonView ownerView) // 아이템 소유자(누가 쏜건지)
    {
        this.ownerView = ownerView;
    }

    // 발동 시 실행, data[0] int targetViewId (int값 -1 = null)
    public abstract void Activate(object[] data);

    //테스트 targetViewId를 Transform으로 변환
    protected Transform FindTargetById(int targetViewId)
    {
        if (targetViewId == -1) return null;

        PhotonView pv = PhotonView.Find(targetViewId);
        return pv != null ? pv.transform : null;
    }
}
