using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Chat : MonoBehaviour
{
    public string channelName = "Lobby";

    protected bool isWriting;

    [SerializeField] protected ChatEntry chatEntryPrefab;
    [SerializeField] protected Transform chatEntryRoot;
    [SerializeField] protected ChatInput chatInputRoot;
    [SerializeField] protected CanvasGroup backgroundGroup;

    protected float idleTime = 0f;
    protected virtual IEnumerator Start()
    {
        yield return new WaitUntil(() => ChatManager.Instance.chatClient.State == Photon.Chat.ChatState.ConnectedToFrontEnd);
        ChatManager.Instance.lobby = this;

        //HACK: 1001-강욱: 챗매니저의 챗클라이언트가 구독 중인 퍼블릭채널 유효성 검증 추가
        if (!ChatManager.Instance.chatClient.PublicChannels.ContainsKey(channelName))
        ChatManager.Instance.JoinChannel(channelName);
        StartCoroutine(BackgroundFadeCoroutine());
    }


    public virtual void ShowChat(string sender, string message)
    {
        ChatEntry entry = Instantiate(chatEntryPrefab, chatEntryRoot);
        entry.SetMessage(sender, message);
    }

    protected virtual void Update()
    {
        if (!isWriting)
        idleTime += Time.deltaTime;

        AwaitEnterKey();

    }

    protected IEnumerator BackgroundFadeCoroutine()
    {
        WaitUntil beforeIdleTime = new WaitUntil(() => idleTime <= 3f);
        WaitUntil afterIdleTime = new WaitUntil(() => idleTime >= 3f);

        while (true)
        {
            yield return afterIdleTime;
            backgroundGroup.DOFade(.3f, 1f);
            yield return beforeIdleTime;
            backgroundGroup.DOComplete();
            backgroundGroup.DOFade(1f, .3f);
        }
    }

    public void ResetIdleTime()
    {
        idleTime = 0f;
    }

    private void AwaitEnterKey()
    {
        if (PhotonNetwork.InLobby && Input.GetKeyDown(KeyCode.Return))
        {
            isWriting = !isWriting;
            chatInputRoot.gameObject.SetActive(isWriting);
            idleTime = 0f;
        }
    }
}
