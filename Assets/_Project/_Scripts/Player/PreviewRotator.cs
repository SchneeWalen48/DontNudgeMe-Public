using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewRotator : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Transform target;

    public CharacterRotate autoRotator;

    [Header("회전 속도")]
    [Tooltip("화면 전체 너비 스와이프 시 캐릭터 총 몇 도 회전할지")]
    public float degreesPerFullSwipe = 360f;
    [Tooltip("회전 민감도")]
    public float sensitivity = 1f;

    private bool isDragging = false;
    private Vector2 lastPointerPosition;

    private Quaternion initialRotation;

    // 한 픽셀당 몇 도 돌릴지(런타임 계산)
    float DegreesPerPixel => (degreesPerFullSwipe / Screen.width) * sensitivity;

    void Start()
    {
        if (target != null)
            initialRotation = target.rotation;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastPointerPosition = eventData.position;

        if(autoRotator != null)
        {
            autoRotator.enabled = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || target == null) return;

        Vector2 delta = eventData.position - lastPointerPosition;

        // X 방향 드래그 → Y축 회전
        float rotateY = -delta.x * DegreesPerPixel;

        target.Rotate(Vector3.up, rotateY, Space.World);

        lastPointerPosition = eventData.position;
    }

    // 마우스/터치 뗐을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if(autoRotator != null)
        {
            autoRotator.enabled = true;
        }
    }

    public void ResetRotation()
    {
        if (target != null)
            target.rotation = initialRotation;
    }
}