using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LSH_CharMove : MonoBehaviour
{

    //[SerializeField] SFXPlayManager sfxPlayManager;
    [SerializeField] float rotationSpeed = 10f;
    Rigidbody rb;
    Animator anim;

    public float moveSpeed;

    public event Action OnJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

    }

    private void Update()
    {
        Move();

        if (Input.GetButtonDown("Jump"))
        {
            // SFXPlayManager버전
            OnJump?.Invoke();
            // SFXPlayManager2 버전
            //sfxPlayManager.PlaySFX(SFXKey.Jump, true, transform.position, false); // network False
        }

        if (Input.GetMouseButtonDown(1))
        {
            AudioManager.Instance.PlayPooledSFX(SFXKey.BumpChar, transform.position);
        }

    }

    private void Move()
    {
        //float x = Input.GetAxis("Horizontal");
        //float z = Input.GetAxis("Vertical");

        //Vector3 moveVec = Vector3.ClampMagnitude(new Vector3(x, 0, z), 1);
        //float speedValue = moveVec.magnitude;

        //rb.velocity = moveVec * moveSpeed;

        ////anim?.SetBool("isWalk", moveVec != Vector3.zero);

        //anim?.SetFloat("XDir", x);
        //anim?.SetFloat("YDir", z);
        //anim?.SetFloat("Speed", speedValue);

        // 앞으로 가는 애니메이션 하나로 360도 퉁칠 수 있다캄
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveVec = Vector3.ClampMagnitude(new Vector3(x, 0, z), 1);
        float speedValue = moveVec.magnitude;

        // 1. 이동
        rb.velocity = moveVec * moveSpeed;

        // 2. 캐릭터 회전
        if (moveVec.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveVec, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // 3. 애니메이터 (앞 걷기 애니메이션 하나만 쓸 거면 Speed만 필요)
        anim?.SetFloat("Speed", speedValue);
    }

    private void PlayStepSFX()
    {
        AudioManager.Instance.PlayPooledSFX(SFXKey.Footstep, transform.position);
    }

}
