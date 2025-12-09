using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
public class MapChangePanel : MonoBehaviourPun
{
    public StageList stageList;
    public Transform contentArea; // 이미지 버튼들을 담을 부모
    public GameObject mapButtonPrefab; // 버튼 프리팹 (Image + Text)
    public RoomPanel roomPanel;

    private List<GameObject> spawnedButtons = new();

    void OnEnable()
    {
        ClearMapList();

        foreach (var stage in stageList.stages)
        {
            GameObject buttonObj = Instantiate(mapButtonPrefab, contentArea);
            Image img = buttonObj.GetComponentInChildren<Image>();
            TextMeshProUGUI txt = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            img.sprite = stage.thumbnail;
            txt.text = stage.displayName;

            // 클릭 이벤트 등록
            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnMapSelected(stage));

            spawnedButtons.Add(buttonObj);
        }
    }

    void OnDisable() => ClearMapList();

    void ClearMapList()
    {
        foreach (var obj in spawnedButtons)
            Destroy(obj);
        spawnedButtons.Clear();
    }

    void OnMapSelected(StageInfo stage)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("현재 방에 있지 않습니다.");
            return;
        }

        // 방 정보 갱신
        PhotonHashtable props = new();
        props["stage"] = stage.sceneName;
        props["stageImage"] = stage.thumbnail.name;
        props["stageName"] = stage.displayName;

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.Log($"[MapChangePanel] 스테이지 변경: {stage.displayName} ({stage.sceneName})");

        if (roomPanel != null)
        {
            roomPanel.SetStageImageByKey(stage.thumbnail.name);
            roomPanel.mapDisplayNameText.text = stage.displayName;
            Debug.Log("[MapChangePanel] RoomPanel 즉시 갱신 완료");
        }

        PhotonView roomPanelPV = roomPanel.GetComponent<PhotonView>();
        if (roomPanelPV != null)
        {
            roomPanelPV.RPC(nameof(RoomPanel.RPC_UpdateStageImage), RpcTarget.OthersBuffered, stage.thumbnail.name);
            roomPanelPV.RPC(nameof(RoomPanel.RPC_UpdateStageName), RpcTarget.OthersBuffered, stage.displayName);
        }

        // UI 패널 닫기
        gameObject.SetActive(false);
    }
}
