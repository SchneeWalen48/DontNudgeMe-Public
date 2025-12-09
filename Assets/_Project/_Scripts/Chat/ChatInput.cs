using System.Collections;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class ChatInput : MonoBehaviour
{
    [SerializeField] Chat chat;
    public TMP_InputField inputField;

    void OnEnable()
    {
        StartCoroutine(ActivateInputField());
    }

    IEnumerator ActivateInputField()
    {
        yield return null;
        inputField.ActivateInputField();
    }
    void OnDisable()
    {
        if (!inputField.text.IsNullOrEmpty())
        {
            ChatManager.Instance.chatClient.PublishMessage(chat.channelName, inputField.text);
            //UserData.Local.GainExp(inputField.text.Length * 20);
            //print($"1글자당 로컬 유저데이터 경험치 20씩 획득함ㅋ");
        }
        inputField.text = "";
    }

}
