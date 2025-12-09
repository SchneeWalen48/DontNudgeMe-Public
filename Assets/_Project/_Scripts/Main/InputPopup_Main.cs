using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ChangeMode
{
    Nickname, Title
}
public class InputPopup_Main : MonoBehaviour
{
    private ChangeMode mode;

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] Button closeButton;

    void Start()
    {
        cancelButton.onClick.AddListener(() => MainUIManager.Instance.CloseAllPanels());
        closeButton.onClick.AddListener(() => MainUIManager.Instance.CloseAllPanels());
        confirmButton.onClick.AddListener(() => ConfirmButtonClick());
    }

    public void Open(ChangeMode mode)
    {
        gameObject.SetActive(true);
        Clear();
        this.mode = mode;

        switch (this.mode)
        {
            case ChangeMode.Nickname:
                //여기는 닉네임체인지 모드인 경우
                titleText.text = "닉네임 변경";
                messageText.text = "새로운 이름을 알려주세요!";
                inputField.transform.Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>().text = "새로운 이름!";
                //TODO: 컨펌버튼 리스너에다가 닉네임체인지 관련 메서드 달기.
                break;
            case ChangeMode.Title:
                //여기는 칭호체인지 모드인 경우
                titleText.text = "칭호 변경";
                messageText.text = "새로운 칭호를 알려주세요!";
                inputField.transform.Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>().text = "새로운 칭호!";
                //TODO: 컨펌버튼 리스너에다가 타이틀체인지 관련 메서드 달기.
                break;
        }
    }

    /// <summary>
    /// 이 UI 그룹의 초기상태 세팅용임.
    /// </summary>
    void Clear()
    {
        titleText.text = "제목";
        messageText.text = "메시지";
        if (!inputField.gameObject.activeSelf) inputField.gameObject.SetActive(true);
        inputField.text = "";
        inputField.transform.Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>().text = "";
        confirmButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(false);
    }


    void ChangeMessageText(string str)
    {
        messageText.text = str;
    }

    private async void ConfirmButtonClick()
    {

        if (mode == ChangeMode.Nickname)
        {
            confirmButton.interactable = false;
            if (!inputField.gameObject.activeSelf)
            {
                inputField.gameObject.SetActive(true);
            }
            cancelButton.interactable = false;
            if (await FirebaseManager.Instance.CheckIfNameReservedAndReset(inputField.text, ChangeMessageText))
            {
                ChangeMessageText($"좋아요! {UserData.Local.userName} 님!");
                confirmButton.interactable = true;
                cancelButton.interactable = true;
                confirmButton.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(false);
                closeButton.gameObject.SetActive(true);
                inputField.text = "";
                inputField.gameObject.SetActive(false);

                //자 이제 여기에다가 채팅 재접속 처리도 해야 함..
                //아니면 단순히 채팅클라이언트의 닉네임 변경 처리만?
                ChatManager.Instance.ReconnectWithNewName();
            }
            else
            {
                confirmButton.interactable = true;
                cancelButton.interactable = true;
                return;
            }
        }
        else // 그럼 체인지모드가 칭호변경이겠지?
        {
            //TODO: 파이어베이스매니저에 칭호 변경을 위한 메서드 넣기.
            //아마 이건 좀 간단할 거임(중복체크 없음.)
            confirmButton.interactable = false;
            if (!inputField.gameObject.activeSelf)
            {
                inputField.gameObject.SetActive(true);
            }
            cancelButton.interactable = false;
            if (await FirebaseManager.Instance.ChangeUserTitle(inputField.text, ChangeMessageText))
            {
                ChangeMessageText($"좋아요! 이제부터 {UserData.Local.userName} 님은 {UserData.Local.userTitle}입니다!");
                confirmButton.interactable = true;
                cancelButton.interactable = true;
                confirmButton.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(false);
                closeButton.gameObject.SetActive(true);
                inputField.text = "";
                inputField.gameObject.SetActive(false);
            }
            else
            {
                confirmButton.interactable = true;
                cancelButton.interactable = true;
                return;
            }
        }
    }
}
