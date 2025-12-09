using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMoveScript : MonoBehaviour
{
    public float moveSpeed;
    public float lifeCount = 5;
    Rigidbody rigid;
    PhotonView photonView;
    private bool inputLocked = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
    }
    void Start()
    {
        //StartCoroutine(SetReady());

        StageManager.Instance.StartGameTimer();
        //SpectatorManager.Instance.AddTarget(transform);
        //SpectatorManager.Instance.SetTarget(transform);

        if (photonView.IsMine)    // 로컬 플레이어만 카메라 타겟 등록
        {
            SpectatorManager.Instance.AddTarget(transform);
            SpectatorManager.Instance.SetTarget(transform);
        }
        else
        {
            SpectatorManager.Instance.AddTarget(transform); // 관전용 타겟 등록
        }
    }
    void Update()
    {
        if (inputLocked) return;

        if(photonView.IsMine)
        {
            Move();

            if (Input.GetButtonDown("Jump"))
            {
                rigid.AddForce(Vector3.up * moveSpeed, ForceMode.Impulse);
            }
        }
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        transform.Translate(x * moveSpeed * Time.deltaTime, 0, z * moveSpeed * Time.deltaTime);
    }
    public void DisableInput() => inputLocked = true;
    public void EnableInput() => inputLocked = false;
    IEnumerator SetReady()
    {
        //플레이어 스폰 이후 3초 대기
        DisableInput();
        yield return new WaitForSeconds(3f);
        EnableInput();
        // 3초가 지난 시점부터 제한시간 카운트 시작
        StageManager.Instance.StartGameTimer();
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Water"))
        {
            SpectatorManager.Instance.RemoveTarget(transform);

            // 플레이어 생성만 호출
            StageManager.Instance.SpawnPlayer();

            // 기존 오브젝트 파괴는 마지막에
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
