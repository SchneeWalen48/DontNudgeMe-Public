using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBUIManager : MonoBehaviour
{
    [Header("Login")]
    public GameObject loginPanel;
    public InputField emailInput;
    public InputField pwInput;

    public Button confirmButton;
    public Button singupButton;


    [Header("Signup")]
    public GameObject singupPanel;




    public bool isSignup = false;


    void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        singupButton.onClick.AddListener(() => isSignup = !isSignup);
        //logoutButton.onClick.AddListener(OnLogoutButtonClick);
    }
    public void OnConfirmButtonClick()
    {
        SetLoginPanelInteractable(false);
        //버튼이 눌렸으니 일단 모두 비활성화
        if (isSignup)
        {
            FirebaseManager.Instance.CreateAccount(emailInput.text, pwInput.text, OnCreateSuccess);
        }
        else
        {
            //TODO:로그인
            FirebaseManager.Instance.SignIn(emailInput.text, pwInput.text, OnLoginSuccess);
        }
    }

    public void OnLogoutButtonClick()
    {
        //TODO:로그아웃
        FirebaseManager.Instance.SignOut();
        //OnLogout();

    }

    //void OnLogout()
    //{
    //    loginPanel.SetActive(true);
    //    SetLoginPanelInteractable(true);
    //    emailInput.text = "";
    //    pwInput.text = "";

    //}

    void OnCreateSuccess(FirebaseUser user)
    {
        if (user != null)
        {
            //PopupManager.Open(PopupType.Dialogue, "알림", $"회원가입 성공", OnLogin);
            Debug.Log($"계정 생성함: {user.Email} ");
        }
        else
        {
            //PopupManager.Open(PopupType.Dialogue, "알림", $"회원가입 실패", x => SetLoginPanelInteractable(true));
            Debug.Log($"계정 생성 실패");
            SetLoginPanelInteractable(true);
        }
    }
    void OnLoginSuccess(FirebaseUser user)
    {
        if (user != null)
        {
            //PopupManager.Open(PopupType.Dialogue, "알림", $"로그인 성공", OnLogin);
        }
        else
        {
            //PopupManager.Open(PopupType.Dialogue, "알림", $"로그인 실패", x => SetLoginPanelInteractable(true));
        }
    }

    public void SetLoginPanelInteractable(bool isInteractable)
    {
        //로그인패널의 모든 컴포넌트를 활성화하거나 비활성화
        emailInput.interactable = isInteractable;
        pwInput.interactable = isInteractable;
        confirmButton.interactable = isInteractable;

    }


    void OnLogin(bool _)
    {
        loginPanel.SetActive(false);
        FirebaseUser user = FirebaseManager.Instance.Auth.CurrentUser;

        //emailText.text = user.Email;
        //nameText.text = user.DisplayName;
        //System.Uri imageURL = user.PhotoUrl;
        //Debug.Log(imageURL);
        //infoPanel.SetActive(true);
    }
}
