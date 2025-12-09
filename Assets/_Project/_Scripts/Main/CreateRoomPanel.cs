using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : MonoBehaviour
{
    public GameObject overlay;

    [Header("Inputs")]
    public TMP_InputField roomNameInput;
    public TMP_Dropdown maxPlayersDropdown;

    [Header("Stage Selection")]
    public Button stageButton;
    public Image stageImage;
    public Sprite defaultStageSprite;
    private Outline stageOutline; 

    [Header("Buttons")]
    public Button createButton;
    public Button exitButton;

    [Header("MapSelect")]
    public StageList stageList;
    public TMP_Dropdown mapSelecDropdown;

    private int selectedIndex = 0;
    private string selectedStageName = "기본 스테이지";

    private void OnEnable()
    {
        if (overlay != null) overlay.SetActive(true);

        if (stageButton != null)
        {
            stageOutline = stageButton.GetComponent<Outline>();
            if (stageOutline != null)
                stageOutline.enabled = false; 

            //stageButton.onClick.AddListener(SelectStage);
        }

        if (createButton != null)
            createButton.onClick.AddListener(CreateRoom);

        if (exitButton != null)
            exitButton.onClick.AddListener(Exit);

        if (roomNameInput != null) roomNameInput.text = $"{PhotonNetwork.LocalPlayer.NickName}님의 방";
        if (maxPlayersDropdown != null) maxPlayersDropdown.value = 0;

        if (stageImage != null && defaultStageSprite != null)
            stageImage.sprite = defaultStageSprite;

        if (mapSelecDropdown != null)
        {
            mapSelecDropdown.onValueChanged.AddListener(SelectStage);
        }
        BuildDropdown();
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (overlay != null) overlay.SetActive(true);
        gameObject.SetActive(true);
    }
    private void BuildDropdown()
    {
        mapSelecDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var s in stageList.stages)
            options.Add(s.displayName); // 플레이어에게 보일 이름만
        mapSelecDropdown.AddOptions(options);
        
        mapSelecDropdown.onValueChanged.RemoveAllListeners();
        mapSelecDropdown.onValueChanged.AddListener(OnMapChanged);

        OnMapChanged(0);
    }
    private void OnMapChanged(int index)
    {
        selectedIndex = index;

        // 썸네일 적용
        if (stageImage != null)
            stageImage.sprite = stageList.stages[index].thumbnail;
    }
    private void SelectStage(int index)
    {
        //TODO: 스테이지 선택시키는 방법
        //스테이지 정보 스크립터블 오브젝트가 필요함.

        //(이건 후순위)(enum)StageType stageType(예: 스테이지 진행 로직이 다른 경우) (이건 후순위)
        //string stageName(예: DelightfulShore), string stageDisplayName(예: 신나는 바닷가) Sprite stageImage(예: 스크린샷) 정도 필요하고
        //스테이지 선택할 때에는 디스플레이네임이랑 스크린샷을 보여줌. 카트라이더 맵고르는 것 생각하면 됨.


        //선택할 때마다 이 방의 커스텀프로퍼티 스트링으로 넘김.
        //예: ExitGames.Client.Photon.Hashtable rCP = new();
        //rCP.Add("stage", "DelightfulShore");
        //PhotonNetwork.CurrentRoom.SetCustomProperties(rCP);


        //실제 게임 시작할 때 로드하는 씬을 방의 커스텀프로퍼티를 가져와서 실행함.
        //예: PhotonNetwork.CurrentRoom.CustomProperties["stage"] == "DelightfulShore"인 상태일 것이므로

        //게임 시작할 때에는 PhotonNetwork.LoadLevel((string)PhotonNetwork.CurrentRoom.CustomProperties["stage"]); 해주면 설정된 스테이지 씬 로드.
        //그럼 모두가 같은 스테이지 씬으로 넘어가게 될 것임.

        selectedStageName = mapSelecDropdown.options[index].text;
        Debug.Log($"[CreateRoomPanel] 스테이지 선택: {selectedStageName}");

        //isStageSelected = !isStageSelected; 

        //if (isStageSelected)
        //{
        //    selectedStageName = "기본 스테이지";
        //    Debug.Log($"스테이지 선택기능 구현 필요함: {selectedStageName}");

        //    if (stageOutline != null)
        //    {
        //        stageOutline.enabled = true;
        //        stageOutline.effectColor = new Color(1f, 0.84f, 0f, 1f); 
        //        stageOutline.effectDistance = new Vector2(5, 5);
        //    }
        //}
        //else
        //{
        //    Debug.Log("스테이지 선택 취소됨");
        //    selectedStageName = "";
        //    if (stageOutline != null)
        //        stageOutline.enabled = false;
        //}
    }

    public void SetAllInteractables(bool isInteractable)
    {
        roomNameInput.interactable = isInteractable;
        maxPlayersDropdown.interactable = isInteractable;
        stageButton.interactable = isInteractable;
        createButton.interactable = isInteractable;
        exitButton.interactable = isInteractable;
        mapSelecDropdown.interactable = isInteractable;
        
    }

    private void CreateRoom()
    {
        //HACK: 1002-강욱: 로비에 있지 않다면 리턴시켜야 함.
        if (!PhotonNetwork.InLobby) return;


        //일단 모든 상호작용 가능 컴포넌트들 상호작용 불가 처리
        SetAllInteractables(false);

        string roomName = roomNameInput.text;
        int maxPlayers = int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

        Debug.Log($"방 생성 요청: {roomName}, 인원: {maxPlayers}, 스테이지: {selectedStageName}");
        RoomOptions ro = new();
        ro.MaxPlayers = maxPlayers;
        ro.PublishUserId = true;

        // 드롭다운에서 현재 선택된 씬 이름
        StageInfo stage = stageList.stages[selectedIndex];
        string sceneName = stage.sceneName;

        var customProps = new Hashtable();
        customProps["stage"] = sceneName;
        customProps["stageImage"] = stage.thumbnail.name; // Sprite 자체는 못 넣으니
        customProps["stageName"] = stage.displayName;
        ro.CustomRoomProperties = customProps;
        ro.CustomRoomPropertiesForLobby = new string[] { "stage", "stageImage", "stageName" }; // 로비에서 보려면 추가
        
        //방 생성 요청하는 코드
        if (!PhotonNetwork.CreateRoom(roomName, ro)) //방 생성 요청을 보내는데, 보낼수 없는 상태라면 UI 요소 상호작용 가능 처리
        {
            SetAllInteractables(true);
        }
    }

    public void Exit()
    {
        //상호작용 가능 컴포넌트 인터랙터블 처리 후에 닫기.
        SetAllInteractables(true);

        CloseOverlay();
        gameObject.SetActive(false);
    }
    private void CloseOverlay()
    {
        if (overlay != null) overlay.SetActive(false);
    }
}