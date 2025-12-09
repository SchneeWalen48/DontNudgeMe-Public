using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI senderTMP;
    [SerializeField] TextMeshProUGUI messageTMP;

    
    public float highlightTime = 2f;
    public Color finalColor = Color.white * .8f;
    public void SetMessage(string sender,  string message)
    {
        senderTMP.text = sender;
        messageTMP.text = message;
        StartCoroutine(ColorChangeCoroutine());
    }
    
    IEnumerator ColorChangeCoroutine()
    {
        senderTMP.color = Color.white * .1f;
        messageTMP.color = Color.white * .1f;
        while (senderTMP.color.r <= Color.white.r)
        {
            senderTMP.color += Color.white * Time.deltaTime * 5;
            messageTMP.color += Color.white * Time.deltaTime * 5;
            yield return null;
        }
        yield return new WaitForSeconds(highlightTime);
        
        
        while (senderTMP.color.r >= finalColor.r)
        {
            senderTMP.color -= Color.white * Time.deltaTime * .5f;
            messageTMP.color -= Color.white * Time.deltaTime * .5f;
            yield return null;
        }
    }

    
}
