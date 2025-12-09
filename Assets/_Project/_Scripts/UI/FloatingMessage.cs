using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class FloatingMessage : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CanvasGroup group;
    [SerializeField] TextMeshProUGUI messageText;

    [Range(.01f,3)]
    public float fadeInDuration;
    [Range(.01f,3)]
    public float stallDuration;
    [Range(.01f,3)]
    public float fadeOutDuration;

    void OnEnable()
    {
        group.alpha = 0;
        StartCoroutine(AppearCoroutine());
    }

    IEnumerator AppearCoroutine()
    {
        
        WaitForSeconds stall = new(stallDuration);
        //fadeIn에 걸쳐 보여주고
        while (group.alpha < 1)
        {
            
            group.alpha += Time.deltaTime * (1 / fadeInDuration);
            yield return null;
        }
        group.alpha = 1;
        //stall동안 정지 후에
        yield return stall;

        //fadeOut에 걸쳐 사라지게 함.
        while (group.alpha > 0)
        {
            group.alpha -= Time.deltaTime * (1 / fadeOutDuration);
            yield return null;
        }
        Destroy(gameObject);

    }

    public void Set(string message)
    {
        messageText.text = message;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
