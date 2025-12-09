using Photon.Pun;
using UnityEngine;

public class PlayerSliding : MonoBehaviourPunCallbacks
{
    // Import the components that need to be referenced in PlayerController.
    private PhotonView pv;
    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;

    [Header("Sliding Settings")]
    public float slideSpeed = 5f;
    public float rotSpeedSliding = 5f;

    // Sliding status and path information
    private bool isSliding = false;
    private SliderPath currSlider; // NOTE : SliderPath 클래스가 외부에서 정의되어 있어야 함
    private int currWaypointIdx;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        if (!pv.IsMine) return;

        HandleSlidingMovement();
    }

    public void HandleSlidingMovement()
    {
        if (!isSliding) return;

        // Sliding Logic
        if (currSlider != null && currWaypointIdx < currSlider.pathWaypoints.Length)
        {
            Transform targetPoint = currSlider.pathWaypoints[currWaypointIdx];
            Vector3 dir = (targetPoint.position - transform.position).normalized;

            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, slideSpeed * Time.fixedDeltaTime);

            Quaternion targetRot = Quaternion.LookRotation(dir);
            Quaternion lyingRot = Quaternion.Euler(-70f, 0, 0); // Setting the lying angle

            Quaternion finalRot = targetRot * lyingRot;

            transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, rotSpeedSliding * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)
            {
                currWaypointIdx++;
            }
        }
        else
        {
            // End Sliding
            isSliding = false;
            rb.isKinematic = false;
            col.enabled = true; // Recover Collider

            anim.SetBool("IsSliding", false);

            // Recover Rotation -> World y Axis
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!pv.IsMine) return;

        if (other.CompareTag("Slider"))
        {
            isSliding = true;
            currSlider = other.GetComponentInParent<SliderPath>();
            anim.SetBool("IsSliding", true);
            currWaypointIdx = 0;
            rb.isKinematic = true; // Stop Physics Engine
            col.enabled = false; // Disable colliders to prevent overlap
        }
    }

    // 외부에서 슬라이딩 상태를 확인할 수 있도록
    public bool IsSliding()
    {
        return isSliding;
    }
}