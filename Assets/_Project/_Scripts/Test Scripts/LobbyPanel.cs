using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPanel : MonoBehaviour
{
    RectTransform roomListingRoot;
    public Room currentRoom;

    void Awake()
    {
        roomListingRoot = transform.Find("Room Listing").GetChild(0).GetComponent<RectTransform>();
    }

    public void ShowRoomInfo(Room room)
    {
        // ...
    }
    

}
