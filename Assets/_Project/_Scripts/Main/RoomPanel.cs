using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviourPunCallbacks
{
    public StageList stageList;

    [Header("Room Info UI")]
    public TextMeshProUGUI roomInfoText;
    public TextMeshProUGUI playerCountText;

    [Header("Stage Info UI")]
    public Image stageImage; 
    public Sprite defaultStageSprite;
    public TextMeshProUGUI mapDisplayNameText;

    [Header("Panels")]
    public GameObject mapChangePanel;

    [Header("Buttons")]
    public Button readyButton;
    public Button exitButton;
    public Button startButton;

    [Header("Player List")]
    public Transform playerEntryArea;
    public PlayerEntry playerEntryPrefab;
    public List<PlayerEntry> playerEntries = new();


    private bool isReady = false;
    private Button stageImageButton;

    private void Start()
    {
       
        readyButton.onClick.AddListener(ToggleReady);
        exitButton.onClick.AddListener(OnExit);
        startButton.onClick.AddListener(OnStart);

        stageImageButton = stageImage.GetComponent<Button>();
        if (stageImageButton == null)
            stageImageButton = stageImage.gameObject.AddComponent<Button>();

        stageImageButton.onClick.AddListener(OnClickStageImage);

        //LSH오디오

        //
        //SetStageImage(null); 
    }

    public override void OnEnable()
    {
        isReady = false;
        SetReadyButton(isReady);

        if (!PhotonNetwork.InRoom) return;
        AudioManager.Instance.PlayRoomBGM();
        InitializePanel();
        PhotonNetwork.AutomaticallySyncScene = true;

        //HACK: 강욱 - 1009: 이 패널이 열렸다는 것은 게임이 한바퀴 돌고 메인씬으로 복귀했다는 것임. 따라서 룸을 다시 Visible하게.
        //근데이제 마스터클라이언트 한명만 수행, 그리고 인원이 꽉찼으면 열지 마
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers;
            SetStartButton(PhotonManager.Instance.IsStartable); //맘에 안들긴 하는데.. 이게 적절한 처리긴 함
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("stageImage", out object imgObj))
            SetStageImageByKey(imgObj as string);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("stageName", out object stageName))
            mapDisplayNameText.text = stageName as string;
    }

    public override void OnDisable()
    {
        ClearPlayerList();
        PhotonNetwork.AutomaticallySyncScene = false;
    }
    void ClearPlayerList()
    {
        foreach (var p in playerEntries)
        {
            Destroy(p.gameObject);
        }
        playerEntries.Clear();
    }

    //패널 OnEnable에서 자동 호출: (룸에 입장) -> 패널 오픈 -> 초기화
    private void InitializePanel()
    {
        //TODO: 전부 포톤네트워크에서 정보 받아오는 것으로 바꿔야 함. 지금은 테스트용으로 currentPlayers는 주작 가능

        //방제
        roomInfoText.text = $"{PhotonNetwork.CurrentRoom.Name}";
        //방 인원 초기화
        playerCountText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        // 이름 출력
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("stageName", out object nameObj))
            mapDisplayNameText.text = nameObj as string;

        // 썸네일 출력
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("stageImage", out object imageObj))
        {
            string imageKey = imageObj as string;
            SetStageImageByKey(imageKey);
        }
        else
        {
            stageImage.sprite = defaultStageSprite;
        }
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            AddEntry(p.Value);
        }

    }
    public void AddEntry(Player newPlayer)
    {
        PlayerEntry entry = Instantiate(playerEntryPrefab, playerEntryArea);
        entry.Set(newPlayer);
        playerEntries.Add(entry);
        playerCountText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
        SetHost(PhotonNetwork.LocalPlayer.IsMasterClient);
    }

    public void RemoveEntry(Player otherPlayer)
    {
        PlayerEntry leaver = playerEntries.Find((x)=>x.player==otherPlayer);
        playerEntries.Remove(leaver);
        Destroy(leaver.gameObject);
        playerCountText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    public void ResetEntry(Player player)
    {
        playerEntries.Find(x => x.player == player).Set(player);
        SetHost(PhotonNetwork.LocalPlayer.IsMasterClient);
    }
    public void SetStageImageByKey(string imageKey)
    {
        foreach (var s in stageList.stages)
        {
            if (s.thumbnail != null && s.thumbnail.name == imageKey)
            {
                stageImage.sprite = s.thumbnail;
                return;
            }
        }
        stageImage.sprite = defaultStageSprite;
    }


    public void SetHost(bool isHost)
    {
        readyButton.gameObject.SetActive(!isHost); 
        startButton.gameObject.SetActive(isHost); 
        exitButton.gameObject.SetActive(true);

        if (stageImageButton != null)
        {
            stageImageButton.interactable = isHost;
        }
    }

    public void SetStartButton(bool isStartable)
    {
        bool isChanging = startButton.interactable != isStartable;
        if (isChanging)
        {
            //여기가 의미하는 것: 스타트버튼의 interactable 상태가 전환되는 타이밍일 때
            startButton.transform.Find("fill").GetComponent<Image>().DOColor(isStartable ? new Color32(0xF4,0x25,0xAC,255) : new Color(.6f, .6f, .6f), .2f);
        }
        startButton.interactable = isStartable;
    }

    private void OnClickStageImage()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (mapChangePanel != null)
        {
            mapChangePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MapChangePanel이 연결되지 않았습니다!");
        }
    }
    private void ToggleReady()
    {
        isReady = !isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new() { { "isReady", isReady } });
        SetReadyButton(isReady);
    }

    private void SetReadyButton(bool isReady)
    {
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "준비해제" : "준비";
    }

    private void OnExit()
    {
        PhotonNetwork.LeaveRoom(); //포톤네트워크 방 나가기 요청 보낸 후
        this.gameObject.SetActive(false); //패널 닫기
    }

    private void OnStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("stage", out object stageObj))
        {
            string sceneName = stageObj as string;
            Debug.Log($"선택된 스테이지로 이동: {sceneName}");
            
            //HACK: 강욱 - 1009: 게임 시작되면 방이 안 보이도록 처리.
            foreach (var p in PhotonNetwork.CurrentRoom.Players)
            {
                p.Value.SetCustomProperties(new() { { "isReady", false } });
            }
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(sceneName);
        }
        else
        {
            Debug.LogWarning("스테이지가 선택되지 않았습니다.");
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("stageImage"))
        {
            string imageKey = propertiesThatChanged["stageImage"] as string;
            SetStageImageByKey(imageKey);
        }

        if (propertiesThatChanged.ContainsKey("stageName"))
        {
            string name = propertiesThatChanged["stageName"] as string;
            mapDisplayNameText.text = name;
            // 맵 이름을 표시하는 Text가 있다면 업데이트
            // e.g. stageNameText.text = name;
        }
    }

    [PunRPC]
    public void RPC_UpdateStageImage(string imageKey)
    {
        SetStageImageByKey(imageKey);
        Debug.Log($"[RoomPanel] RPC 수신: 맵 이미지 {imageKey} 로 변경");
    }
    [PunRPC]
    public void RPC_UpdateStageName(string stageName)
    {
        mapDisplayNameText.text = stageName;
        Debug.Log($"[RoomPanel] RPC 수신: 맵 이름 {stageName} 로 변경");
    }
}