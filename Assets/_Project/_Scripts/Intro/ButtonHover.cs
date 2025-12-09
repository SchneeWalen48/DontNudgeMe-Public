using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Vector3 targetScale;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float smoothSpeed = 10f;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void OnDisable()
    {
        targetScale = originalScale;
        transform.localScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * smoothSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
