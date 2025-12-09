//using Photon.Pun;
//using Photon.Realtime;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using PhotonTable = ExitGames.Client.Photon.Hashtable;


//public class RoomPanel : MonoBehaviour
//{
//    [SerializeField] TextMeshProUGUI roomNameText;
//    [SerializeField] RectTransform playerList;
//    [SerializeField] PlayerEntry playerEntryPrefab;
//    [SerializeField] Button startButton;
//    [SerializeField] Button leaveButton;

//    void Awake()
//    {
        



//        startButton.onClick.AddListener(() => { PhotonNetwork.LoadLevel("StageOneScene"); });
//        //leaveButton.onClick.AddListener(() => { PhotonNetwork.LeaveRoom(); PhotonNetwork.JoinLobby(); });
//    }




//    void OnEnable()
//    {
//        if (!PhotonNetwork.InRoom) return; //방에 들어온 상태가 맞는지 체크해줌.

//        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
//        //foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
//        //{
//        //    //플레이어 엔트리 생성
//        //    JoinPlayer(player);
//        //}
//        //SortPlayer();

//        bool isMaster = PhotonNetwork.IsMasterClient;

//        startButton.gameObject.SetActive(isMaster);
//        PhotonNetwork.AutomaticallySyncScene = true;

//        //UpdatePlayerList();
//    }

//    void OnDisable()
//    {
//        foreach (Transform child in playerList)
//        {
//            Destroy(child.gameObject);
//        }
//        PhotonNetwork.AutomaticallySyncScene = false;
//    }

//    void UpdatePlayerList()
//    {
//        Transform[] pList = playerList.GetComponentsInChildren<Transform>();
//        foreach (Transform a in pList)
//        {
//            Destroy(a.gameObject);
//        }
//        var players = PhotonNetwork.CurrentRoom.Players.Values;
//        foreach (var p in players)
//        {
//            JoinPlayer(p);
//        }
//    }

//    public void JoinPlayer(Player newPlayer)
//    {
//        //새 플레이어 엔트리 생성
//        PlayerEntry playerEntry = Instantiate(playerEntryPrefab, playerList, false);
//        playerEntry.name = newPlayer.NickName;
//        playerEntry.playerNameText.text = newPlayer.NickName;
//        playerEntry.player = newPlayer;

//        if (newPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
//        //내 플레이어가 아닐 경우
//        {
//            playerEntry.readyToggle.gameObject.SetActive(false);
//        }
//        SortPlayer();
//    }

//    private void SortPlayer()
//    {
//        foreach (Transform child in playerList)
//        {
//            Player player = child.GetComponent<PlayerEntry>().player;
//            child.SetSiblingIndex(player.ActorNumber);
//        }
//    }

//    public void LeavePlayer(Player leaving)
//    //기존 플레이어 엔트리 삭제
//    {
//        foreach (Transform child in playerList)
//        {
//            Player player = child.GetComponent<PlayerEntry>().player;
//            if (player.ActorNumber == leaving.ActorNumber)
//            {
//                Destroy(child.gameObject);
//            }

//        }
//        SortPlayer();
//    }







//    //TODO: 플레이어들의 Ready상태 동기화 처리
//    //public void OnPlayerReady(Player player, bool isReady)
//    //{

//    //}

 


//}
