using Firebase;
using Firebase.Auth;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhotonTable = ExitGames.Client.Photon.Hashtable;
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    public Dictionary<string, CustomizationData> customizationDict;



    public void FetchCustomizationData()
    {
        customizationDict = new();
        foreach (var c in PhotonNetwork.CurrentRoom.Players)
        {
            if (CustomizationData.TryFromPhoton(c.Value.CustomProperties, out CustomizationData data))
            {
                if (!customizationDict.ContainsKey(c.Value.UserId))
                {
                    Debug.Log($"{c.Value.NickName}의 커스터마이징 정보 불러옴.");
                    if (data == null) { data = new(); Debug.Log($"왠지 몰라도 {c.Value.NickName}의 커스터마이징 정보가 null임"); }
                    Debug.Log(c.Value.UserId);
                    customizationDict.Add(c.Value.UserId, data);
                    Debug.Log($"{c.Value.NickName}:{c.Value.UserId}의 커스터마이징 정보 최초 저장.");
                }
                else
                {
                    customizationDict[c.Value.UserId] = data;
                }
            }
        }
    }

    public void RemoveCustomizationData(Player otherPlayer)
    {
        customizationDict.Remove(otherPlayer.UserId);
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnJoinedLobby()
    {
        //여기서 UI의 방 목록 띄워주는 메서드를 연결시켜주면 될 듯.
        //다만 현재 OnRoomListUpdate에서 처리 중이므로 여기는 그냥 냅둬도 될 듯
        base.OnJoinedLobby();
        //LSH오디오
        AudioManager.Instance.PlayLobbyBGM();
        //확인용
        Debug.Log($"OnJoinedLobby 호출 완료");
        MainUIManager.Instance?.OnJoinedLobby();
        ResetInventory.ResetPlayerInventory();
    }

    public override void OnLeftRoom()
    {
        //HACK: 1001-강욱: 아래의 OnConnectedToMaster에서, 현재 씬이 메인씬이라면 로비로 조인시키도록 변경.
        //방을 나갔기 때문에 로비로 다시 강제 복귀시키기.
        //PhotonNetwork.JoinLobby();


        Debug.Log(PhotonNetwork.NetworkClientState);
        //HACK: 강욱 - 1010: 여기서 메인UI매니저가 모든 룸 엔트리를 지우도록 하면 괜찮을 듯..?
        MainUIManager.Instance.OnLeftRoom();
        ResetInventory.ResetPlayerInventory();


        //방을 나갈 때에, 나의 레디상태를 false로 할당.
        PhotonNetwork.LocalPlayer.SetCustomProperties(new() { { "isReady", false } });
    }


    public override void OnConnectedToMaster()
    {
        print("OnConnectedToMaster() 호출.");

        //인트로씬일 때만 아래 코드 수행
        if (SceneManager.GetActiveScene().name == "MainScene") PhotonNetwork.JoinLobby();
        if (SceneManager.GetActiveScene().name != "IntroScene") return;
        
        
        
        
        
        PhotonTable data = new();
        data["level"] = UserData.Local.level;
        data["exp"] = UserData.Local.exp;
        data["nextExp"] = UserData.Local.nextExp;
        data["userTitle"] = UserData.Local.userTitle ?? string.Empty;

        //isReady를 무조건 false로 만들면서 커스텀프로퍼티 설정.
        data["isReady"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(data);


        //포톤 유저닉네임, 로컬 유저데이터 네임, DB 네임 모두 동기화됨.
        //DB에서 받아온 유저데이터 -> 로컬유저데이터화 정상.
        //포톤 로컬플레이어의 커스텀프로퍼티화 -> 정상.

        //HACK: 0928-강욱: 여기에 코드 추가함: 기존에는 포톤 커스텀프로퍼티로 유저데이터만 저장했는데,
        //이제부터는 커스터마이징 정보도 커스텀프로퍼티화해서 저장함.
        CustomizationData.LocalToPhotonCP();
        //이게 잘 된다면 커스텀프로퍼티로도 커스터마이징 정보가 넘어간 것이므로
        //메인씬 로드해도 큰 문제 안 생김. 메인 씬의 커스터마이징 정보 반영되는 부분에서 로컬의 커스텀프로퍼티 참조 필요.
        ChatManager.Instance.Connect();


        
        //TODO: 하.... 이거.... 자꾸 마스터서버에 접속할 때마다 이 씬을 다시 로드하는 것 확인함.
        //룸->로비로 돌아올 때에 채팅 사라지는 문제도 결국엔 이거때문인거 확인함.
        //방법을 좀 찾아봐야 할 듯. 단순히 현재 씬 이름 분기로 비교해서 리턴시키기엔 다른 변수가 발생함.
        SceneManager.LoadScene("MainScene");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //엔트리에 새 플레이어 보여줘야 함.
        if (MainUIManager.Instance != null && MainUIManager.Instance.roomPanel != null)
            MainUIManager.Instance.roomPanel.AddEntry(newPlayer);
        FetchCustomizationData();

        //만약에 방이 다 차게 되면 룸을 안보이게 해줘야겠지?

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonTable rcp = new();
            if (!rcp.ContainsKey("lastRefresh"))
                rcp.Add("lastRefresh", System.DateTime.UtcNow.Ticks);
            else rcp["lastRefresh"] = System.DateTime.UtcNow.Ticks;

            PhotonNetwork.CurrentRoom.SetCustomProperties(rcp);
            PhotonNetwork.CurrentRoom.IsVisible = PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers;
            MainUIManager.Instance.roomPanel.SetStartButton(IsStartable);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //나간 플레이어를 엔트리에서 빼줘야 함.
        if (MainUIManager.Instance != null && MainUIManager.Instance.roomPanel != null)
            MainUIManager.Instance.roomPanel.RemoveEntry(otherPlayer);
        RemoveCustomizationData(otherPlayer);

        if (PhotonNetwork.IsMasterClient)
            {
                PhotonTable rcp = new();
                if (!rcp.ContainsKey("lastRefresh"))
                    rcp.Add("lastRefresh", System.DateTime.UtcNow.Ticks);
                else rcp["lastRefresh"] = System.DateTime.UtcNow.Ticks;

                PhotonNetwork.CurrentRoom.SetCustomProperties(rcp);
            if (SceneManager.GetActiveScene().name == "MainScene")
                PhotonNetwork.CurrentRoom.IsVisible = PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers;
            MainUIManager.Instance.roomPanel.SetStartButton(IsStartable);
        }
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (MainUIManager.Instance != null && MainUIManager.Instance.roomPanel != null)
        MainUIManager.Instance.roomPanel.ResetEntry(newMasterClient);

        //만약 내가 마스터가 된다면 나의 준비상태는 false가 됨(의미없지만 아무튼 false가 됨)
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonTable { { "isReady", false } });
            MainUIManager.Instance.roomPanel.SetStartButton(IsStartable);
        }
    }
    public override void OnJoinedRoom()
    {
        Debug.Log($"방에 참여함. 방 제목: {PhotonNetwork.CurrentRoom.Name}");
        if (MainUIManager.Instance != null && MainUIManager.Instance.roomPanel != null)
            MainUIManager.Instance.OpenPanel(PanelType.Room);
        FetchCustomizationData();

        //방에 들어가는 순간 나의 준비상태는 false가 됨.
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonTable { { "isReady", false } });

    }

    //아몰랑 방참가 실패하면 걍 로비로 귀환
    public override void OnJoinRoomFailed(short returnCode, string message)
    {

        string errorMessage = "알 수 없는 오류로 인해 방에 참가하지 못했어요!";

        switch (returnCode)
        {
            case 32765: // GameFull
                errorMessage = "방이 가득 찼나봐요!";
                break;
            case 32764: // GameClosed
                errorMessage = "방이 닫혀버렸어요...";
                break;
            case 32758: // GameDoesNotExist
                errorMessage = "방이 없어졌나봐요!";
                break;
            default:
                break;
        }
        
        MainUIManager.Instance.ShowFloatingMessage(errorMessage);
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        MainUIManager.Instance.createRoomPanel.Exit();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        string errorMessage = "알 수 없는 오류로 인해 방을 만들지 못했어요!";

        switch (returnCode)
        {
            case 32766: // GameIdAlreadyExists
                errorMessage = "같은 이름의 방이 이미 있나 봐요!";
                break;
            case 32762: // ServerFull
                errorMessage = "서버가 꽉 차서 방을 만들지 못했어요!";
                break;
            case -2: // InvalidOperation
                errorMessage = "잘못된 요청입니다...";
                break;
            case -1: // InternalServerError
                errorMessage = "서버 내부 오류로 방을 만들지 못했어요!";
                break;
            default:
                break;
        }


        MainUIManager.Instance.ShowFloatingMessage(errorMessage);
        MainUIManager.Instance.createRoomPanel.SetAllInteractables(true);
        Debug.Log(PhotonNetwork.NetworkClientState);
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //foreach (RoomInfo roomInfo in roomList)
        //{
        //        //TODO: IsVisible한 roomInfo 각각에 대한 룸 엔트리 생성 처리?
        //        MainUIManager.Instance.RoomEntryUpdate(roomInfo);
        //}
        MainUIManager.Instance?.RoomListUpdate(roomList);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonTable changedProps)
    {
        //누군가의 커스텀프로퍼티가 업데이트되었다 -> 그가 준비/준비해제했다는 뜻
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            MainUIManager.Instance.roomPanel.ResetEntry(targetPlayer);

            if (PhotonNetwork.IsMasterClient)
                MainUIManager.Instance.roomPanel.SetStartButton(IsStartable);
        }
        

        
    }

    public override void OnRoomPropertiesUpdate(PhotonTable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }

    public bool IsStartable
    {
        get
        {
            foreach (var p in PhotonNetwork.CurrentRoom.Players)
            {
                //방장일 때는 건너뜀
                if (p.Value.IsMasterClient) continue;

                if (p.Value.CustomProperties["isReady"] is false)
                {
                    return false;
                    //누구 하나라도 준비가 안 됐을 경우 즉시 false 리턴
                }
            }
            return true;
        }
    }

}
