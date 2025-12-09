using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPung : SkillItemBase
{
    //[SerializeField] float offsetBack = 1f;
    [SerializeField] float duration = 60f;
    [SerializeField] float knockbackForce = 50f;

    //private void Start()
    //{
    //    Vector3 pos = transform.position;
    //    pos.y += 0.2f;
    //    transform.position = pos;
    //}

    public override void Activate(object[] data)
    {
        //if (ownerView != null)
        //{
        //    transform.position = ownerView.transform.position - ownerView.transform.forward * offsetBack;
        //    transform.rotation = Quaternion.identity;
        //}
        StartCoroutine(TrapDuration());
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView otherView = other.GetComponent<PhotonView>();
        if (otherView == null) return;
        if (other.CompareTag("DSkillItem"))
        {
            //if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
            //if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
            StartCoroutine(DestroyAfterDelay(0.1f));
        }

        if ((other.TryGetComponent<PlayerController>(out PlayerController pc) && pc.shield == null))
        {
            SFXEvents.Raise(SFXKey.Trampoline, transform.position, true, true);
            if (PhotonNetwork.IsMasterClient) photonView.RPC("TriggerTrap", RpcTarget.All, otherView.ViewID);
        }

        //if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
        StartCoroutine(DestroyAfterDelay(0.1f));
    }

    private IEnumerator TrapDuration()
    {
        yield return new WaitForSeconds(duration);

        if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RpcInitTrap(int ownerViewId)
    {
        ownerView = PhotonView.Find(ownerViewId);
        Activate(null);
    }

    [PunRPC]
    private void TriggerTrap(int targetViewId)
    {
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f)).normalized;

        PhotonView targetView = PhotonView.Find(targetViewId);
        if (targetView != null)
        {
            Rigidbody rb = targetView.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.AddForce(randomDir * knockbackForce, ForceMode.VelocityChange);
            }
        }
    }
}
