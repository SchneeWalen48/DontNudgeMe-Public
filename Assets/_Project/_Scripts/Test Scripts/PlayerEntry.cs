using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameLabel;
    [SerializeField] private Image background;

    public Player player;
    private bool isMaster = false;
    private bool isLocal = false;
    private bool IsReady => (bool)player.CustomProperties["isReady"];
    public void Set(Player player)
    {
        this.player = player;
        Refresh();
    }

    private void Refresh()
    {
        isMaster = player.IsMasterClient;
        isLocal = player.IsLocal;


        playerNameLabel.text = isMaster ? $"{player.NickName} <color=yellow>(방장!)</color>" : player.NickName;
        playerNameLabel.color = isLocal ? Color.green : Color.white;

        background.color = IsReady ? Color.cyan : Color.gray;
    }
}
