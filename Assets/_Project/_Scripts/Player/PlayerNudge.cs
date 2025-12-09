using UnityEngine;
using Photon.Pun;

public class PlayerNudge : MonoBehaviourPunCallbacks
{
    [Header("Push")]
    public float pushForce = 8f;
    public float pushRadius = 1f;
    private float radiusOffset = 0.5f;
    public LayerMask othersLayer;

    [Range(0, 90)]
    public float pushAngle = 50f;
    [Tooltip("밀쳐진 플레이어 입력 차단 되는 시간")]
    public float inputBlockDuration = 0.3f;

    private Rigidbody rb;
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (PhotonNetwork.IsConnectedAndReady)
                photonView.RPC("PushAndAnimate", RpcTarget.All);
            else
                PushAndAnimate();
        }
    }

    [PunRPC]
    void PushAndAnimate()
    {
        anim.SetTrigger("Push");

        int mask = (othersLayer.value == 0) ? ~0 : othersLayer;
        var center = new Vector3(transform.position.x, transform.position.y + radiusOffset, transform.position.z);
        var colls = Physics.OverlapSphere(center, pushRadius, mask, QueryTriggerInteraction.Ignore);
        float pushAngleCos = Mathf.Cos(pushAngle * Mathf.Deg2Rad);

        foreach (Collider col in colls)
        {
            Rigidbody othersRb = col.attachedRigidbody;
            if (!othersRb || othersRb == rb) continue;

            Vector3 pushDir = (col.transform.position - transform.position).normalized;

            if (Vector3.Dot(transform.forward, pushDir) <= pushAngleCos) continue;

            var otherPv = col.GetComponentInParent<PhotonView>();
            if (otherPv && otherPv != photonView)
                otherPv.RPC(nameof(ApplyNudgeForce), otherPv.Owner, pushDir * pushForce, (int)ForceMode.Impulse);
            //LSH오디오
            if (!photonView.IsMine) return;
            SFXEvents.Raise(SFXKey.Bump, transform.position, true, false);
            //
        }
    }

    [PunRPC]
    void ApplyNudgeForce(Vector3 force, int mode)
    {
        GetComponent<Rigidbody>().AddForce(force, (ForceMode)mode);

        if (photonView.IsMine)
        {
            PlayerController pController = GetComponent<PlayerController>();
            if (pController != null)
                pController.BlockInput(inputBlockDuration);
        }

        //LSH오디오
        if (!photonView.IsMine) return;
        SFXEvents.Raise(SFXKey.BumpChar, transform.position, true, true);
        //
    }

    // Nudge Radius
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + radiusOffset, transform.position.z), pushRadius);
    }
}