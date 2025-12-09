using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum PopupType
{
    Message, NameInput
}

public class Popup : MonoBehaviour
{
    protected TextMeshProUGUI titleText;
    protected TextMeshProUGUI messageText;
    protected Button closeButton;


    protected virtual void Awake()
    {
        titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        messageText = transform.Find("Message").GetComponent<TextMeshProUGUI>();
        closeButton = transform.Find("Close").GetComponent<Button>();
    }

    /// <summary>
    /// 팝업의 종류와 무관하게 매개변수로 여러 스트링을 받으며 팝업을 설정합니다.<br></br> 단, 팝업 종류에 따라 받을 수 있는 매개변수 갯수 이상의 매개변수는 무시됩니다.<br></br> 매개변수 순서가 중요하니까 매개변수 설명을 참조하세요.
    /// </summary>
    /// <param name="strings">0번: 제목, 1번: 안내문구, 2번: 버튼 문구(긍정), 3번: 버튼 문구(부정) 4번: 몰라</param>
    public virtual void SetPopup(params string[] strings)
    {
        titleText.text = strings[0];
        messageText.text = strings[1];
        closeButton.GetComponentInChildren<TextMeshProUGUI>().text = strings[2];
    }

    /// <summary>
    /// 매개변수로 스트링 하나만 전달해서 메시지텍스트만 바꾸는 메서드
    /// </summary>
    /// <param name="str">메시지텍스트가 보여줄 스트링</param>
    public virtual void SetPopup(string str)
    {
        messageText.text = str;
    }

    public virtual void ResetCloseButtonCallback(UnityAction callback)
    {
        if (callback == null) { return; }
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(callback);
    }
}
