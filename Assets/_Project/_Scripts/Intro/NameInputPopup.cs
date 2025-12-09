using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class NameInputPopup : Popup
{
    protected TMP_InputField inputField;

    protected override void Awake()
    {
        base.Awake();
        inputField = transform.Find("NewName").GetComponent<TMP_InputField>();
    }

    public override void SetPopup(params string[] strings)
    {
        base.SetPopup(strings);
    }

    void OnEnable()
    {
        if (!inputField.gameObject.activeSelf)
        {
            inputField.gameObject.SetActive(true);
        }
    }

    public async Task OnNameChangeButtonClick()
    {
        if (!inputField.gameObject.activeSelf)
        {
            inputField.gameObject.SetActive(true);
        }
        closeButton.interactable = false;
        if (await FirebaseManager.Instance.CheckIfNameReservedAndReset(inputField.text, SetPopup))
        {
            SetPopup($"좋아요! {UserData.Local.userName} 님!");
            closeButton.interactable = true;
            closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "좋아요!";
            inputField.text = "";
            inputField.gameObject.SetActive(false);
            ResetCloseButtonCallback(() => { IntroManager.Instance.ClosePopup(PopupType.NameInput); IntroManager.Instance.ShowStartAndLogout(); });
        }
        else
        {
            closeButton.interactable = true;
            return;
        }
    }
}
