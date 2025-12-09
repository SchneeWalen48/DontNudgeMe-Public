using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum PanelType
{
    Create, Room, Customizing, Input_Nickname, Input_Title
}
public class MainUIManager : MonoBehaviour
{
    
    public static MainUIManager Instance { get; private set; }

    public StageList stageList;

    [Header("플레이어 정보 관련")]
    [SerializeField] NameTag nameTag;
    //[SerializeField] TextMeshProUGUI nicknameText; //로컬플레이어의 닉네임
    //[SerializeField] Image profileImage;
    //[SerializeField] TextMeshProUGUI levelText; //로컬플레이어의 레벨
    //[SerializeField] Slider expBar; //로컬플레이어의 경험치 (### / ###)

    [Header("오버레이")]
    public Image overlay;

    [Header("패널")]
    public RoomPanel roomPanel; //실제 방에 입장 후 보이는 패널
    public CreateRoomPanel createRoomPanel; //방 생성 패널(방의 정보 설정)
    public GameObject customizingPanel; //커스터마이징 패널(아아아주 나중에 구현할 거임)

    [Header("방 목록 영역")]
    [SerializeField] Transform roomListRect; //로비 패널(방 목록 보기용)
    [SerializeField] Button createButton; //방 만들기 버튼
    [SerializeField] RoomEntry roomEntryPrefab;

    [Header("기능 버튼")]
    [SerializeField] Button quickJoinButton; //아무 방이나 입장/생성후 입장 버튼 => 퀵스타트 버튼
    [SerializeField] Button findButton; //돋보기 버튼(방 검색인가?)
    

    [Header("슬라이딩UI 메뉴 버튼")]
    [SerializeField] Button customizingButton;
    [SerializeField] Button shopButton;
    [SerializeField] Button pickupButton;

    [Header("네임태그 기능 패널")]
    [SerializeField] InputPopup_Main inputPopup;

    [Header("플로팅 메시지 출력 전용")]
    [SerializeField] RectTransform floatingMessageRoot;
    [SerializeField] FloatingMessage floatingMessagePrefab;

    [Header("룸 툴팁 표기 전용")]
    public RoomTooltip roomTooltip;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //DatabaseReference userDataRef = FirebaseManager.Instance.DB.RootReference.Child($"UsersTest/{FirebaseManager.Instance.Auth.CurrentUser.UserId}");
        //if (!nicknameText)  print("닉네임텍스트 할당 안함");
        //else nicknameText.text = PhotonNetwork.LocalPlayer.NickName;
        //if (!levelText) print("레벨텍스트 할당 안함");
        //else levelText.text = ((int)PhotonNetwork.LocalPlayer.CustomProperties["level"]).ToString();
        //if (!expBar) print("exp텍스트 할당 안함");
        //else expBar.value = (float)PhotonNetwork.LocalPlayer.CustomProperties["exp"] / (float)PhotonNetwork.LocalPlayer.CustomProperties["nextExp"];
        if (!quickJoinButton) print("빠른시작버튼 할당 안함");
        else quickJoinButton.onClick.AddListener(QuickJoinButtonClick);
        customizingButton.onClick.AddListener(() => OpenPanel(PanelType.Customizing));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    

    void Start()
    {
        
        if (PhotonNetwork.IsConnected)
        {
            //Start에 왜 이게 있냐면
            //내 포톤네트워크의 상태가 로비에도 방에도 참가하지 않은 상태면 강제로 로비로 조인시켜주기 위해서임
            //if (!PhotonNetwork.InRoom && !PhotonNetwork.InLobby)
            //    PhotonNetwork.JoinLobby();
            //TODO: 한사이클 돌고 나오면 아마도 두 상태중 하나에 해당할 것이므로, 그 상태에 대한 처리 필요.
            //예) InRoom이라면 곧바로 Room패널을 열어줘야 할 것이고, InLobby라면 방 목록 업데이트 처리를 해주면 됨.

            // 이미 방 안에 있다면 → 바로 RoomPanel을 열어줌
            if (PhotonNetwork.InRoom)
            {
                OpenPanel(PanelType.Room);
                return;
            }

            // 방에는 없고 로비에도 없는 상태라면 → 로비로 참가
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }
    }

    public void OpenPanel(PanelType type)
    {
        CloseAllPanels();
        overlay.gameObject.SetActive(true);
        switch (type)
        {
            case PanelType.Create:
                createRoomPanel.gameObject.SetActive(true);
                break;
            case PanelType.Room:
                roomPanel.gameObject.SetActive(true);
                break;
            case PanelType.Customizing:
                customizingPanel.gameObject.SetActive(true);
                break;
            case PanelType.Input_Nickname:
                inputPopup?.Open(ChangeMode.Nickname);
                break;
            case PanelType.Input_Title:
                inputPopup?.Open(ChangeMode.Title);
                break;
        }

    }

    public void CloseAllPanels()
    {
        overlay.gameObject.SetActive(false);
        roomPanel.gameObject.SetActive(false);
        createRoomPanel.gameObject.SetActive(false);
        customizingPanel.gameObject.SetActive(false);
        inputPopup.gameObject.SetActive(false);

        //추가: 룸툴팁도 가려
        HideRoomTooltip();
    }


    ////TODO: 이제 룸리스트를 한번에 받아서 처리하지 말고 개별 룸의 속성을 따져서 처리하는 방식으로 변경해보자. 그러지 말자.
    //public void RoomEntryUpdate(RoomInfo room)
    //{
    //    GameObject match = null;
    //    foreach (Transform t in roomListRect)
    //    { 
    //        if (t.name == room.Name) match = t.gameObject; break;
    //    }
    //    if (match) //매치되는 엔트리 오브젝트가 이미 있다는 뜻
    //    {
    //        //방이 !IsVisible하거나 RemovedFromList한 경우 삭제 처리.
    //        if (!room.IsVisible || room.RemovedFromList)
    //        {
    //            Destroy(match); return;
    //        }

    //        //방의 참여인원 등등 갱신 필요하므로 해당 처리.
    //        match.GetComponent<RoomEntry>().Initialize(room);

    //    }
    //    else //if (!match)인 경우
    //    {
    //        RoomEntry entry = Instantiate(roomEntryPrefab, roomListRect);
    //        entry.gameObject.name = room.Name;
    //        entry.Initialize(room);
    //        match = entry.gameObject;
    //    }

    //    if (room == null) return;
    //    if (!room.IsVisible) return;
    //    if (room.RemovedFromList) Destroy(roomListRect.Find(room.Name).gameObject);

    //}

    //모든 룸 엔트리를 지우는 메서드를 한번 없애고 해봅니다...

    void ClearRoomEntries() { foreach (Transform c in roomListRect) Destroy(c.gameObject); }
    //1. 룸 리스트의 업데이트가 들어온 상황. MonobehaviourPunCallbacks에서 이 메서드 호출됨


    public void RoomListUpdate(List<RoomInfo> roomList)
    {




        foreach (RoomInfo info in roomList)
        {
            //TODO: 여기 처리 깔끔하게 해줄 필요가 있음...
            
            foreach (Transform t in roomListRect)
            {
                if (t.name == info.Name) //맞는 엔트리를 찾았음
                {
                    //이 순간 info가 사라져야 할 엔트리라면, 지워주기.
                    if (info.RemovedFromList || !info.IsVisible || !(info.PlayerCount < info.MaxPlayers))
                    {
                        Destroy(t.gameObject);
                        //추가: 새로고침 후에 이 엔트리가 지워질 엔트리면 룸툴팁 가려주기
                        if (roomTooltip.gameObject.activeSelf && roomTooltip.GetRoomName == info.Name)
                            HideRoomTooltip();
                        continue; 
                        //변동이 생겼을 수 있는 룸인포도 똑같이 처리해주기 위해 컨티뉴
                    }

                    //일단 맞는 룸엔트리를 찾았으면 새로고침 시켜주기.
                    var match = t.GetComponent<RoomEntry>();
                    match.GetComponent<RoomEntry>().ExternalRefresh(info);

                    //추가: 새로고침 후에 만약에 지금 마우스를 올려둔 룸이라면 툴팁도 업데이트해줘야 함.
                    //룸툴팁이 띄워져 있는데, 공교롭게도 지금 보고 있는 룸인 상황이라면?
                    if (roomTooltip.gameObject.activeSelf && roomTooltip.GetRoomName == info.Name)
                        ShowRoomTooltip(info);
                    

                }
            }
            
            
            if (info.RemovedFromList || !info.IsVisible || !(info.PlayerCount < info.MaxPlayers)) continue;
            
            
            // ????뭐지 대체 작동은 잘 되는 것같은데 내가 쓴 게 이해가 안 됨
            foreach (Transform t in roomListRect)
            {
                if (t.name == info.Name) return;
            }

            //요 아래까지 내려왔다는 것은 위에서 아무것도 안 걸린 거임. 그럼 새로 생성이 필요함.
            RoomEntry entry = Instantiate(roomEntryPrefab, roomListRect);
            entry.gameObject.name = info.Name;
            entry.ExternalRefresh(info);

            

        }
    }


    private void QuickJoinButtonClick()
    {
        quickJoinButton.interactable = false;
        RoomOptions option = new();
        option.MaxPlayers = 8;
        option.PublishUserId = true;

        

        // 랜덤 스테이지 선택
        StageInfo stage = stageList.stages[Random.Range(0, stageList.stages.Length)];

        // Custom Properties 세팅
        var customProps = new ExitGames.Client.Photon.Hashtable();
        customProps["stage"] = stage.sceneName;
        customProps["stageName"] = stage.displayName; // "바닷가" 같은 표시용 이름
        customProps["stageImage"] = stage.thumbnail.name; // Sprite 자체는 못 넣으니 리소스 이름 저장
        

        option.CustomRoomProperties = customProps;
        option.CustomRoomPropertiesForLobby = new string[] { "stage", "stageImage", "stageName" };
        // => 로비에서도 표시 가능하도록

        PhotonNetwork.JoinRandomOrCreateRoom(
            roomName: $"Room {Random.Range(0, 1000):D3} (빠른 참가)",
            roomOptions: option
            
        );
        
    }

    public void OnJoinedLobby()
    {
        createButton.interactable = true;
        quickJoinButton.interactable = true;
        
        if (!createRoomPanel.gameObject.activeSelf)
            overlay.gameObject.SetActive(false);

    }
    
    public void OnLeftRoom()
    {
        ClearRoomEntries();
    }

    //팝업 띄우는 기능 만들고, PhotonManager.OnCreateRoomFailed(...) 에서 호출하도록 만들기(방제가 중복이라거나 등등의 예외 처리용)


    public void ShowFloatingMessage(string text)
    {
        FloatingMessage message = Instantiate(floatingMessagePrefab, floatingMessageRoot);
        message.Set(text);
    }


    public void ShowRoomTooltip(RoomInfo room)
    {
        roomTooltip.gameObject.SetActive(true);
        roomTooltip.InjectRoomInfo(room);
    }

    public void MoveRoomTooltip(Vector2 mousePos)
    {
        Vector2 finalPos = new(-550, mousePos.y - 540);

        RectTransform roomTooltipRect = roomTooltip.GetComponent<RectTransform>();
        roomTooltipRect.anchoredPosition = finalPos;


    }

    public void HideRoomTooltip()
    {
        roomTooltip.gameObject.SetActive(false);
    }
}
