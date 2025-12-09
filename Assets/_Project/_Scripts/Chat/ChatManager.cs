using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    #region Define Singleton
    public static ChatManager Instance { get; private set; }

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
    #endregion


    public ChatClient chatClient;
    string currentRoom;

    public Chat lobby;
    public RoomChat room;

    public void JoinChannel(string channelName)
    {
        if (chatClient.PublicChannels.ContainsKey(channelName)) return;
        chatClient.Subscribe(new string[] { channelName });
    }
    public void LeaveChannel(string channelName)
    {
        if (!chatClient.PublicChannels.ContainsKey(channelName)) return;
        chatClient.Unsubscribe(new string[] { channelName });
    }

    public void ReconnectWithNewName()
    {
        chatClient.Disconnect();
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
                           "1.0", new AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));
    }

    public void Connect()
    {
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
                           "1.0", new AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));
    }

    void Update()
    {
        chatClient?.Service();
       
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        
    }

    public void OnChatStateChange(ChatState state)
    {
        
    }

    public void OnConnected()
    {
        if (lobby)
        {
            if (!chatClient.PublicChannels.ContainsKey(lobby.channelName))
                JoinChannel(lobby.channelName);
        }
    }

    public void OnDisconnected()
    {
       
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName == "Lobby")
        {
            for (int i = 0; i < senders.Length; i++)
            {
                if (!lobby) return;
                lobby.ShowChat(senders[i], messages[i].ToString());
                lobby.ResetIdleTime();
            }
        } else
        {
            for (int i = 0; i < senders.Length; i++)
            {
                if (!room) return;
                room.ShowChat(senders[i], messages[i].ToString());
                room.ResetIdleTime();
            }
        }

    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        
    }

    public void OnUnsubscribed(string[] channels)
    {

    }

    public void OnUserSubscribed(string channel, string user)
    {
        
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        
    }
}
