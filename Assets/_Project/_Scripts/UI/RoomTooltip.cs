using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomTooltip : MonoBehaviour
{
    RoomInfo roomInfo;


    [SerializeField] StageList stageList;
    public string GetRoomName => roomInfo?.Name;

    [SerializeField] TextMeshProUGUI roomLabel;
    [SerializeField] TextMeshProUGUI stageLabel;
    [SerializeField] Image stageImage;

    //[SerializeField] RectTransform playersGroup;
    //[SerializeField] PlayerEntry playerEntryPrefab;
    [Range(.2f, 1f)]
    public float refreshInterval;

    float refreshGauge;
    void OnEnable()
    {
        StartCoroutine(AppearCoroutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }
    IEnumerator AppearCoroutine()
    {
        GetComponent<CanvasGroup>().alpha = 0f;
        yield return new WaitUntil(() => roomInfo != null);
        GetComponent<CanvasGroup>().alpha = 1f;
        
    }

    void RefreshRoomInfo()
    {
        roomLabel.text = roomInfo.Name;
        if (roomInfo.CustomProperties.TryGetValue("stageImage", out object imgObj))
            SetStageImageByKey(imgObj as string);
        if (roomInfo.CustomProperties.TryGetValue("stageName", out object stageName))
            stageLabel.text = stageName as string;
    }


    //포톤네트워크에서, 로비에 있는 채로 특정 방 안의 플레이어 정보를 정확히 알려면 아예 룸의 커스텀프로퍼티로 넘기는 처리가 필요함.
    //일단은 플레이어 엔트리를 제외한 부분만 제어해보고, 추후에 필요 시 기능을 강화하려고 함.
    //void RefreshPlayerEntries()
    //{
    //    int playerCount = roomInfo.PlayerCount;
    //    Player[] players = roomInfo.
    //    if (playersGroup.childCount < roomInfo.PlayerCount)
    //    {
    //        var playerEntry = Instantiate(playerEntryPrefab, playersGroup);
    //    }
        
    //}

    public void InjectRoomInfo(RoomInfo roomInfo)
    {
        this.roomInfo = roomInfo;
        RefreshRoomInfo();
    }

    public void ClearRoomInfo()
    {
        this.roomInfo = null;
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
        stageImage.sprite = null;
    }
}
