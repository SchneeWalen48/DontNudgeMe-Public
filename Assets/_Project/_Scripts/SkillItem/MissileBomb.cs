using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MissileBomb : SkillItemBase
{
    [Header("Flight Setting")]
    [SerializeField] float ascendHeight = 10f;
    [SerializeField] float ascendDuration = 0.5f;
    [SerializeField] float homingSpeed = 100f;

    [Header("Hit Setting")]
    [SerializeField] float knockbackForce = 20f;

    [Header("Effect")]
    [SerializeField] GameObject letsSmoke;
    [SerializeField] ParticleSystem doSmoke;
    [SerializeField] GameObject smokeZone;
    [SerializeField] GameObject confettiObj;


    private Transform target;
    private bool blockedByShield = false;

    private void OnDestroy()
    {
        var confetti = (Instantiate(confettiObj, transform.position + Vector3.up * 1f, transform.rotation)).GetComponent<ParticleSystem>();
        confetti.Play();
        Destroy(confetti.gameObject, 2.5f);
    }

    public override void Activate(object[] data)
    {
        int targetViewId = (int)data[0];
        target = FindTargetById(targetViewId);

        StartCoroutine(MissileRoutine());
    }

    private IEnumerator MissileRoutine()
    {
        SFXEvents.Raise(SFXKey.LaunchMissile, transform.position, true, true);

        // 1단계: 상승
        Vector3 start = transform.position;
        Vector3 ascendPos = start + Vector3.up * ascendHeight;

        float t = 0f;
        while (t < ascendDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, ascendPos, t / ascendDuration);
            yield return null;
        }

        doSmoke = (Instantiate(letsSmoke, smokeZone.transform.position, smokeZone.transform.rotation)).GetComponent<ParticleSystem>();
        doSmoke.transform.SetParent(this.transform);
        doSmoke.Play();

        // 2단계: 타겟 추적
        while (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * homingSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir);
            yield return null;
        }

        // 타겟이 없거나 끝까지 날아간 미사일의 마지막 폭발 처리
        if (!blockedByShield && photonView.IsMine)
        {
            int targetViewId = -1;
            if (target != null)
            {
                PhotonView targetView = target.GetComponent<PhotonView>();
                if (targetView != null) targetViewId = targetView.ViewID;
            }
            Debug.Log("문제1");
            photonView.RPC("TriggerMissile", RpcTarget.All, targetViewId);
        }

        PhotonNetwork.Destroy(gameObject);
    }

    private void DoHitEffect(PhotonView otherView)
    {
        if (blockedByShield) return; // 쉴드에 막혔으면 타격 무효

        doSmoke.Pause();
        Destroy(doSmoke.gameObject, 1f);
        SFXEvents.Raise(SFXKey.MissileBomb, transform.position, true, true);

        if (otherView != null && otherView.GetComponent<PlayerController>())
        {
            Debug.Log("문제2");
            photonView.RPC("TriggerMissile", RpcTarget.All, otherView.ViewID);
        }

        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView otherView = other.GetComponent<PhotonView>();
        if (otherView == null) return;

        if (other.CompareTag("DSkillItem"))
        {
            Debug.Log("D스킬아이템검출");
            if (otherView.Owner == photonView.Owner)
            {
                // 내 방어막 무시
                Debug.Log("내 방어막이에요");
                return;
            }

            blockedByShield = true;
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        if (otherView != null && !other.CompareTag("DSkillItem") && otherView.Owner != photonView.Owner /*|| (other.TryGetComponent<PlayerController>(out PlayerController pc) && pc.shield != null)*/)
        {
            Debug.Log("문제3");
            DoHitEffect(otherView);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PhotonView otherView = other.GetComponent<PhotonView>();
        if (other.CompareTag("DSkillItem"))
        {
            if (otherView.Owner == photonView.Owner)
            {
                // 내 방어막 무시
                Debug.Log("내 방어막이에요");
                return;
            }

            blockedByShield = true;
            Debug.Log("문제4");
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void TriggerMissile(int targetViewId)
    {
        PhotonView targetView = PhotonView.Find(targetViewId);
        if (targetView != null)
        {
            Rigidbody rb = targetView.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                Vector3 knockback = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(knockback * knockbackForce, ForceMode.VelocityChange);
            }
        }
    }

    [PunRPC]
    private void OnBlockedByShield()
    {
        blockedByShield = true;
        Debug.Log("RPC문제에요");
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}
