using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillItemBox : MonoBehaviourPun
{
    [SerializeField] SkillItemPool siPool;
    [SerializeField] Vector3 rotationAngle = new Vector3(0, 45, 0);
    [SerializeField] float respawnTime = 5f;

    private Collider col;
    private MeshRenderer mRend;

    private void Awake()
    {
        col = GetComponent<Collider>();
        mRend = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        RotateObject();
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView otherPv = other.GetComponent<PhotonView>();
        if (!otherPv.IsMine) return;

        var inven = other.GetComponent<PlayerSkillItemInventory>();
        if (inven != null)
        {
            SkillItemData si = siPool.GetRandomSkillItem();
            
            if (si != null)
            {
                inven.AddSkillItem(si);
            }

            photonView.RPC("HideBox", RpcTarget.All);
            Invoke(nameof(RespawnBox), respawnTime);
        }
    }

    [PunRPC]
    private void HideBox()
    {
        col.enabled = false;
        mRend.enabled = false;
    }

    [PunRPC]
    private void ShowBox()
    {
        col.enabled = true;
        mRend.enabled = true;
    }

    private void RespawnBox()
    {
        photonView.RPC("ShowBox", RpcTarget.All);
    }

    private void RotateObject()
    {
        transform.Rotate(rotationAngle * Time.deltaTime, Space.Self);
    }
}
