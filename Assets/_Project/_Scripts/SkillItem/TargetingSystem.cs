using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [SerializeField] LayerMask candidateMask;
    [SerializeField] GameObject lockOnMarkerPrefab;
    [SerializeField] Transform camTransform;
    [SerializeField] Transform myChar;
    [SerializeField] float radiusSize = 10f;
    [SerializeField] float maxDistance = 10f;
    //[SerializeField] float checkTimer = 0.2f;

    private GameObject activeMarker;
    private Transform currentTarget;
    //private float resetCheckTimer;

    // 타겟팅이 필요한 아이템이 이 메서드로 currentTarget을 가져와서 타겟팅 표시를 할 수 있게 
    public Transform CurrentTarget => currentTarget;

    private void Awake()
    {
        myChar = transform;
        camTransform = transform.Find("CamPivot/Main Camera").GetComponent<Camera>().transform;
        //resetCheckTimer = checkTimer;
    }

    #region 구버전 타겟팅 테스트

    // 타겟팅이 필요한 아이템이 이 메서드로 currentTarget을 가져와서 타겟팅 표시를 할 수 있게 
    //public Transform GetCurrentTarget()
    //{
    //    //if (!isActive) return null;
    //    return currentTarget;
    //    //return FindTarget();
    //}

    //private void Update()
    //{
    //    if (Input.GetMouseButton(1))
    //    {
    //        checkTimer -= Time.deltaTime;

    //        if (checkTimer <= 0f || (currentTarget == null && !IsTargetValid(currentTarget)))
    //        {
    //            currentTarget = FindTarget();
    //            checkTimer = resetCheckTimer;
    //        }

    //        if (currentTarget != null)
    //        {
    //            ShowTargetMarker(currentTarget);
    //            Debug.DrawLine(camTransform.position, currentTarget.position, Color.red);
    //        }
    //        else HideTargetMarker();
    //    }

    //    if (Input.GetMouseButtonUp(1))
    //    {
    //        currentTarget = null;
    //        HideTargetMarker();
    //    }

    //}

    //private void ShowTargetMarker(Transform target)
    //{
    //    //if (activeMarker != null) Destroy(activeMarker);
    //    //if (target != null)
    //    //{
    //    //    activeMarker = Instantiate(lockOnMarkerPrefab, target);
    //    //    activeMarker.transform.localPosition = new Vector3(0, 1, -0.5f);
    //    //    Debug.Log($"타겟 : {target.name}");
    //    //}
    //    // 매번 Instantiate를 하면 GC에서 화낼 수 있어서 바꿈
    //    if (activeMarker == null)
    //    {
    //        activeMarker = Instantiate(lockOnMarkerPrefab);
    //    }

    //    if (target != null)
    //    {
    //        activeMarker.transform.SetParent(target, false);
    //        activeMarker.transform.localPosition = new Vector3(0, 1, -0.5f);
    //        activeMarker.SetActive(true);
    //        Debug.Log($"타겟 : {target.name}");
    //    }
    //}

    //private void HideTargetMarker()
    //{
    //    if (activeMarker != null) activeMarker.SetActive(false);
    //}

    #endregion


    public void RefreshTarget()
    {
        currentTarget = FindTarget();
        UpdateMarker(currentTarget);
    }

    public void ClearTarget()
    {
        currentTarget = null;
        UpdateMarker(null);
    }

    private void UpdateMarker(Transform target)
    {
        if (target != null)
        {
            if (activeMarker == null) activeMarker = Instantiate(lockOnMarkerPrefab);

            activeMarker.transform.SetParent(target.transform, false);
            activeMarker.transform.localPosition = new Vector3(0, 1, 0);
            activeMarker.SetActive(true);
            Debug.Log($"타겟 : {target.name}");
            
        }
        else
        {
            if (activeMarker != null) activeMarker.SetActive(false);
        }
    }

    private Transform FindTarget()
    {
        float camHeightOffset = camTransform.position.y - myChar.position.y;
        Vector3 center = myChar.position + myChar.forward * maxDistance;
        center.y += camHeightOffset;
        Collider[] candidates = Physics.OverlapSphere(center, radiusSize, candidateMask);

        if (candidates.Length == 0)
            return null;

        Transform closeTarget = null;
        // 지금까지 발견한 후보 중 가장 카메라 정면에 가까운 값
        float bestAngle = Mathf.Infinity;

        foreach (var c in candidates)
        {
            // 태그 근데 LayerMask를 쓰는데 필요할까?
            //if (!c.CompareTag("Player")) continue;
            PhotonView view = c.GetComponent<PhotonView>();
            Debug.Log($"[Targeting2] Candidate {c.name}, has PV={view != null}, isMine={(view ? view.IsMine : false)}");
            if (c.transform.root == myChar) continue;
            if (view != null && view.IsMine) continue;
            // 후보 타겟 방향 벡터 (카메라 기준)
            Vector3 dirToTarget = (c.transform.position - camTransform.position).normalized;
            // 카메라 정면과 후보 타겟 방향 벡터 사이 각도
            float angle = Vector3.Angle(camTransform.forward, dirToTarget);
            Debug.Log($"[Targeting3] Candidate {c.name}, angle={angle}");

            if (angle < bestAngle)
            {
                bestAngle = angle;
                closeTarget = view.transform;
            }
        }
        return closeTarget;
        //return null;
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;
        if (target.root == myChar) return false;
        float dist = Vector3.Distance(myChar.position, target.position);
        if (dist > maxDistance) return false;

        return true;
    }

    private void OnDrawGizmos()
    {
        float camHeightOffset = camTransform.position.y - myChar.position.y;
        Vector3 center = myChar.position + myChar.forward * maxDistance;
        center.y += camHeightOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, radiusSize);
    }
}
