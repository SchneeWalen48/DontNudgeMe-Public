using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomChat : Chat
{
    protected override IEnumerator Start()
    {
        yield return new WaitUntil(() => ChatManager.Instance.chatClient.State == Photon.Chat.ChatState.ConnectedToFrontEnd);
        ChatManager.Instance.room = this;
        
    }
    void OnEnable()
    {
        idleTime = 0f;
        if (this.channelName != PhotonNetwork.CurrentRoom.Name)
        {
            this.channelName = PhotonNetwork.CurrentRoom.Name;
            foreach (Transform t in chatEntryRoot)
            {
                Destroy(t.gameObject);
            }
        }
        ChatManager.Instance.JoinChannel(channelName);
        StartCoroutine(BackgroundFadeCoroutine());
    }

    void OnDisable()
    {
        ChatManager.Instance.LeaveChannel(channelName);
    }

    protected override void Update()
    {
        base.Update();
        AwaitEnterKey();
    }

    private void AwaitEnterKey()
    {
        if (PhotonNetwork.InRoom && Input.GetKeyDown(KeyCode.Return))
        {
            isWriting = !isWriting;
            chatInputRoot.gameObject.SetActive(isWriting);
            idleTime = 0f;
        }
    }
}
