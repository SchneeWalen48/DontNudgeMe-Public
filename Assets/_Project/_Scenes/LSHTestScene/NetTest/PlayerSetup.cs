using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    [Header("Player Components")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    private void Awake()
    {
        playerCamera = transform.Find("CamPivot/Main Camera").GetComponent<Camera>();
        //테스트
        audioListener = transform.GetComponent<AudioListener>();
        //기존
        //audioListener = transform.Find("CamPivot/Main Camera").GetComponent<AudioListener>();

        // 내 캐릭터가 아니면 카메라/리스너 끄기
        if (!photonView.IsMine)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }
    }
}
