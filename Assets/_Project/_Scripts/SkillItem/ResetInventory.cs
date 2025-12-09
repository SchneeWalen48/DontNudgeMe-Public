using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResetInventory
{
    public static void ResetPlayerInventory()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["items"] = new string[0];
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
