using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
public class RoomEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    public RoomInfo roomInfo;
    TextMeshProUGUI roomNameLabel;
    TextMeshProUGUI currentPlayers;
    TextMeshProUGUI maxPlayers;
    
    

    void Awake()
    {
        roomNameLabel = transform.Find("Room Name Label").GetComponent<TextMeshProUGUI>();
        currentPlayers = transform.Find("Current Players").GetComponent<TextMeshProUGUI>();
        maxPlayers = transform.Find("Max Players").GetComponent<TextMeshProUGUI>();
    }

    public void ExternalRefresh(RoomInfo roomInfo)
    {
        this.roomInfo = roomInfo;
        Refresh();
        if (MainUIManager.Instance.roomTooltip.gameObject.activeSelf && MainUIManager.Instance.roomTooltip.GetRoomName == roomInfo.Name)
        {
            MainUIManager.Instance.ShowRoomTooltip(roomInfo);
        }
    }

    void Refresh()
    {
        if (roomInfo == null) return;
        roomNameLabel.text = roomInfo.Name;
        currentPlayers.text = roomInfo.PlayerCount.ToString();
        maxPlayers.text = roomInfo.MaxPlayers.ToString();
    }

    //클릭하면 룸에 조인, 근데 실패하는 상황은 어떻게 처리하지?
    //클릭 후 룸에 조인 처리가 끝나기 전에 다른 동작을 하게 되면 어떡하지?
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!PhotonNetwork.JoinRoom(this.roomInfo.Name))
        {
            //JoinRoom 요청을 보낼 수 없는 상황을 뜻함.
            Debug.LogWarning("방 참가 요청을 보낼 수 없음. 이번 요청은 무시됩니다.");
        } else
        {
            Debug.Log($"{this.roomInfo.Name} 방에 대한 참가요청 정상 송신됨.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MainUIManager.Instance.ShowRoomTooltip(roomInfo);
        MainUIManager.Instance.MoveRoomTooltip(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        
        MainUIManager.Instance.MoveRoomTooltip(eventData.position);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        MainUIManager.Instance.HideRoomTooltip();
    }

}
