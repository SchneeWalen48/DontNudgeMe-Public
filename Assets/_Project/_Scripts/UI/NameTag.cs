using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public class NameTag : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    UserData data;
    [SerializeField] TextMeshProUGUI userNameTMP;
    [SerializeField] TextMeshProUGUI userTitleTMP;
    [SerializeField] TextMeshProUGUI currentLevelTMP;
    [SerializeField] TextMeshProUGUI nextLevelTMP;
    [SerializeField] Image expFillImage;

    [SerializeField] RectTransform nametagMenuGroup;
    [SerializeField] Button nicknameChangePopupButton;
    [SerializeField] Button titleChangePopupButton;

    void OnEnable()
    {
        data = UserData.Local;
        data.onDataChanged += OnDataChanged;
        ApplyData(data);

        if (nametagMenuGroup)
        nametagMenuGroup.anchoredPosition = Vector3.zero;
    }

    void OnDisable()
    {
        data.onDataChanged -= OnDataChanged;
    }
    void ApplyData(UserData data)
    {
        userNameTMP.text = UserData.Local.userName;
        userTitleTMP.text= UserData.Local.userTitle;
        currentLevelTMP.text = UserData.Local.level.ToString();
        nextLevelTMP.text = (UserData.Local.level + 1).ToString();
        expFillImage.fillAmount = (UserData.Local.exp / UserData.Local.nextExp);
    }

    void Awake()
    {
        nicknameChangePopupButton.onClick.AddListener(() => MainUIManager.Instance.OpenPanel(PanelType.Input_Nickname));
        titleChangePopupButton.onClick.AddListener(() => MainUIManager.Instance.OpenPanel(PanelType.Input_Title));
        //if (PhotonNetwork.InLobby && FirebaseManager.Instance.Auth.CurrentUser.DisplayName != UserData.Local.userName)
        //{
        //    UserData.RefetchUserData();
        //}
    }

    void OnDataChanged(UserData data)
    {
        ApplyData(data);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        print($"로컬 유저데이터 네임: {UserData.Local.userName}");
        print($"현재 인증정보 디스플레이 네임: {FirebaseManager.Instance.Auth.CurrentUser.DisplayName}");
        print($"원격 유저데이터 네임: {FirebaseManager.Instance.DB.RootReference.Child("Users").Child(FirebaseManager.Instance.Auth.CurrentUser.UserId).Child("Data").Child("userName").GetValueAsync()}");
        if (PhotonNetwork.InLobby && FirebaseManager.Instance.Auth.CurrentUser.DisplayName == UserData.Local.userName)
        {
            //TODO: 네임태그 메뉴 그룹 띄워주기.
            if (nametagMenuGroup)
            nametagMenuGroup.DOAnchorPos(new(0,-80), .5f).SetEase(Ease.InOutFlash);
            //이 부분은 DOKill이나 DOComplete가 필요 없음: 고정 위치로 이동하기 때문에
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //TODO: 네임태그 메뉴 그룹 가려주기
        if (nametagMenuGroup)
        nametagMenuGroup.DOAnchorPos(Vector2.zero, .5f).SetEase(Ease.InOutFlash);
        //이 부분은 DOKill이나 DOComplete가 필요 없음: 고정 위치로 이동하기 때문에
    }




}
    
