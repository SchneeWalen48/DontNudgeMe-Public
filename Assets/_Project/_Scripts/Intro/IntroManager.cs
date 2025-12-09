using Firebase.Auth;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WebSocketSharp;
using PhotonTable = ExitGames.Client.Photon.Hashtable;

public class IntroManager : MonoBehaviour
{
    #region 싱글톤 정의 부분
    public static IntroManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion
    [Header("Background")]

    [Header("Buttons")]
    public Button loginButton;
    public Button signupButton;
    public Button guestButton;
    public Button startButton;
    public Button logoutButton;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;

    //HACK: 강욱 - 0923: 팝업 열고닫기 기능을 위해 추가합니다.
    [Header("Popups")]
    public GameObject popupGroup;
    public Popup messagePopup;
    public NameInputPopup nameInputPopup;

    void Start()
    {
        // 시작 시 패널 닫기
        if (loginPanel != null) loginPanel.SetActive(false);
        if (signupPanel != null) signupPanel.SetActive(false);

        if (startButton != null) startButton.gameObject.SetActive(false);
        if (logoutButton != null) logoutButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CirculateInputFields();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmInputFields();
        }
    }
    private void ConfirmInputFields()
    {
        if (!(loginPanel.activeSelf || signupPanel.activeSelf)) return;
        if (loginPanel.activeSelf)
        {
            if (loginPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().text.IsNullOrEmpty()) return;
            if (loginPanel.transform.Find("PWInput").GetComponent<TMP_InputField>().text.IsNullOrEmpty()) return;

            OnLoginConfirm();
        }
        if (signupPanel.activeSelf)
        {
            if (signupPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().text.IsNullOrEmpty()) return;
            if (signupPanel.transform.Find("PWInput").GetComponent<TMP_InputField>().text.IsNullOrEmpty()) return;

            OnSignupConfirm();
        }
    }
    private void CirculateInputFields()
    {
        if (!(loginPanel.activeSelf || signupPanel.activeSelf)) return;
        if (loginPanel.activeSelf)
        {
            if (loginPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().isFocused)
            {
                loginPanel.transform.Find("PWInput").GetComponent<TMP_InputField>().ActivateInputField();
                return;
            } else
            {
                loginPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().ActivateInputField();
                return;
            }
        }
        if (signupPanel.activeSelf)
        {
            if (signupPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().isFocused)
            {
                signupPanel.transform.Find("PWInput").GetComponent<TMP_InputField>().ActivateInputField();
                return;
            } else
            {
                signupPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().ActivateInputField();
                return;
            }
        }
    }

  
    public void OpenLoginPanel() => TogglePanel(loginPanel, true);
    public void CloseLoginPanel() => TogglePanel(loginPanel, false);
    public void OpenSignupPanel() => TogglePanel(signupPanel, true);
    public void CloseSignupPanel() => TogglePanel(signupPanel, false);

    private void TogglePanel(GameObject panel, bool open)
    {
        if (panel != null) panel.SetActive(open);
    }

    private void OpenPopup(PopupType popup, UnityAction callback, params string[] strings)
    {
        if (!popupGroup.activeSelf) popupGroup.gameObject.SetActive(true);
        switch (popup)
        {
            case PopupType.Message:
                if (!messagePopup.gameObject.activeSelf) messagePopup.gameObject.SetActive(true);
                messagePopup.SetPopup(strings[0], strings[1], strings[2]);
                messagePopup.ResetCloseButtonCallback(callback);
                break;
            case PopupType.NameInput:
                if (!nameInputPopup.gameObject.activeSelf) nameInputPopup.gameObject.SetActive(true);
                nameInputPopup.SetPopup(strings[0], strings[1], strings[2]);
                nameInputPopup.ResetCloseButtonCallback(callback);
                break;
            default:
                break;
        }
    }

    public void ClosePopup(PopupType popup)
    {
        switch (popup)
        {
            case PopupType.Message:
                messagePopup.gameObject.SetActive(false);
                break;
            case PopupType.NameInput:
                nameInputPopup.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }



    public void OnLoginConfirm()
    {
        //HACK 강욱 - 0923: Firebase에 입력한 정보로 로그인 시도. 만약에 실패하면 콜백에서 에러 날 것임
        string id = loginPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().text;
        string pw = loginPanel.transform.Find("PWInput").GetComponent<TMP_InputField>().text;
        FirebaseManager.Instance.SignIn(id, pw, WelcomeLogin);
        //CloseLoginPanel();


        void WelcomeLogin(FirebaseUser user)
        {
            if (user != null)
            {
                //로그인 성공이라는 뜻
                if (user.DisplayName == "DEFAULT" || user.DisplayName.IsNullOrEmpty())
                { OpenPopup(PopupType.NameInput, async () => { await nameInputPopup.OnNameChangeButtonClick(); CloseLoginPanel(); ShowStartAndLogout(); }, "닉네임 변경", $"환영해요!\n당신의 이름을 알려주세요!", "변경!"); }
                else
                {
                    OpenPopup(PopupType.Message, () =>
                {
                    ClosePopup(PopupType.Message); CloseLoginPanel(); ShowStartAndLogout();
                }, "로그인 성공", $"환영해요!\n{user.DisplayName} 님!", "좋아요!");
                }
            }
            else
            {
                OpenPopup(PopupType.Message, () => { ClosePopup(PopupType.Message); }, "로그인 실패", $"이메일 혹은 비밀번호가 틀렸거나,\n등록되지 않은 회원 정보입니다.", "네...");
            }
        }
        //ShowStartAndLogout();
    }

    public void OnSignupConfirm()
    {
        //HACK 강욱 - 0923: Firebase에 입력한 정보로 회원가입 시도. 만약에 실패하면 콜백에서 에러 날 것임
        //Firebase에 회원가입하면서 동시에 DB에도 UID로 정보를 남김.
        string id = signupPanel.transform.Find("IDInput").GetComponent<TMP_InputField>().text;
        string pw = signupPanel.transform.Find("PWInput").GetComponent<TMP_InputField>().text;
        FirebaseManager.Instance.CreateAccount(id, pw, WelcomeSignup);
        void WelcomeSignup(FirebaseUser user)
        {
            if (user != null)
            {
                //로그인 성공이라는 뜻
                OpenPopup(PopupType.Message, () => { ClosePopup(PopupType.Message); CloseSignupPanel(); }, "회원가입 성공", $"환영해요!\n로그인 후에 닉네임을 설정해 주세요.\n", "좋아요!");
            }
            else
            {
                OpenPopup(PopupType.Message, () => { ClosePopup(PopupType.Message); }, "회원가입 실패", $"이미 등록된 회원이거나, 양식에 맞지 않는 정보입니다", "네...");
            }
        }

        //CloseSignupPanel();
        //ShowStartAndLogout();
    }

    public void OnGuestConfirm()
    {
        //HACK 강욱 - 0923: Firebase에 익명 로그인 시도. 만약에 실패하면 콜백에서 에러 날 것임
        FirebaseManager.Instance.GuestSignIn((x) =>
        {
            if (x != null)
            {
                OpenPopup(PopupType.NameInput, async () => { await nameInputPopup.OnNameChangeButtonClick(); ShowStartAndLogout(); }, "게스트 로그인 성공", $"환영해요!\n닉네임을 설정해 주세요.\n", "변경!");
            }
            else
            {
                OpenPopup(PopupType.Message, () => { ClosePopup(PopupType.Message); }, "게스트 로그인 실패", $"오류가 발생했습니다. 다시 시도해 줄래요?", "네...");
            }
        });

        


    }

   

    public void ShowStartAndLogout()
    {
        if(loginPanel != null) loginButton.gameObject.SetActive(false);
        if(signupButton != null) signupButton.gameObject.SetActive(false);
        if(guestButton != null) guestButton.gameObject.SetActive(false);

        if(startButton != null) startButton.gameObject.SetActive(true);
        if(logoutButton != null) logoutButton.gameObject.SetActive(true);
    }
    public void OnLogout()
    {
        FirebaseManager.Instance.SignOut();
        print("로그아웃");

        if (startButton) startButton.gameObject.SetActive(false);
        if (logoutButton) logoutButton.gameObject.SetActive(false);

        if (loginButton) loginButton.gameObject.SetActive(true);
        if (signupButton) signupButton.gameObject.SetActive(true);
        if (guestButton) guestButton.gameObject.SetActive(true);
    }

    public void OnStartGame()
    {
        print("게임 시작");
        PhotonNetwork.AuthValues = new(FirebaseManager.Instance.Auth.CurrentUser.UserId);
        
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.LocalPlayer.NickName = UserData.Local.userName;
        //SceneManager.LoadScene("MainScene");
    }
}
