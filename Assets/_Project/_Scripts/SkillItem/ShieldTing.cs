using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShieldTing : SkillItemBase
{
    [SerializeField] float duration = 2f;
    [SerializeField] float bumpDuration = 0.2f;

    public override void Activate(object[] data)
    {
        //테스트
        if (ownerView != null)
        {
            transform.SetParent(ownerView.transform, false);
            transform.localPosition = Vector3.zero;
        }

        SFXEvents.Raise(SFXKey.BumpChar, transform.position, true, true);
        StartCoroutine(ShieldDuration());

        //구분구분구분
        transform.parent.GetComponent<PlayerController>().shield = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView otherView = other.GetComponent<PhotonView>();
        if (other.CompareTag("ASkillItem"))
        {
            Debug.Log("공격아이템이 들어온다");
            if (other.GetComponent<MissileBomb>() && otherView !=null && !(otherView.Owner == photonView.Owner))
            {
                otherView.RPC("OnBlockedByShield", otherView.Owner);
            }
            if (!(otherView.Owner == photonView.Owner && other.GetComponent<MissileBomb>()))
            {
                SFXEvents.Raise(SFXKey.Emote, transform.position, true, true);
                transform.DOPunchScale(Vector3.one * 0.2f, bumpDuration, vibrato: 3, elasticity: 1f);
            }

        }
    }

    private void OnTriggerStay(Collider other)
    {
        PhotonView otherView = other.GetComponent<PhotonView>();
        if (other.CompareTag("ASkillItem") && other.GetComponent<MissileBomb>() && otherView != null && !(otherView.Owner == photonView.Owner))
        {
            Debug.Log("공격아이템이 들어왔다");
            otherView.RPC("OnBlockedByShield", otherView.Owner);
            
        }
    }

    private IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(duration);

        if (photonView.IsMine)
        {
            //구분구분구분
            transform.parent.GetComponent<PlayerController>().shield = null;
            PhotonNetwork.Destroy(gameObject);
        }
    }

}
