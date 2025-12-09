using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerQuickEmoji : MonoBehaviourPunCallbacks
{
    [Header("Quick Emoji")]
    public Sprite[] emojis;
    public Image emojiImage;
    public float emojiLifeTime = 2f;
    private float emojiTimer = 0f;

    void Awake()
    {
        // World Canvas의 이모지 이미지 비활성화
        if (emojiImage)
        {
            emojiImage.enabled = false;
            emojiImage.sprite = null;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleEmojiInput();
        }

        // emoji timer (로컬/원격 플레이어 모두에게 적용)
        if (emojiImage && emojiImage.enabled)
        {
            emojiTimer -= Time.deltaTime;
            if (emojiTimer <= 0f)
            {
                emojiImage.enabled = false;
                emojiImage.sprite = null;
            }
        }
    }

    void HandleEmojiInput()
    {
        int idx = -1;
        // LSH오디오 0929 추후 감정표현에 따라 소리도 나눌 생각임다
        if (Input.GetKeyDown(KeyCode.Alpha1))
        { idx = 0; SFXEvents.Raise(SFXKey.Emote, transform.position, false, false); }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        { idx = 1; SFXEvents.Raise(SFXKey.Emote, transform.position, false, false); }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        { idx = 2; SFXEvents.Raise(SFXKey.Emote, transform.position, false, false); }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        { idx = 3; SFXEvents.Raise(SFXKey.Emote, transform.position, false, false); }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        { idx = 4; SFXEvents.Raise(SFXKey.Emote, transform.position, false, false); }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        { idx = 5; SFXEvents.Raise(SFXKey.Emote, transform.position, false, false); }

        // Change World Canvas's Image
        if (idx >= 0 && idx < emojis.Length)
        {
            // RPC를 호출하여 모든 클라이언트에게 이모지 표시를 요청
            photonView.RPC(nameof(ShowEmojiRPC), RpcTarget.All, idx, emojiLifeTime);
        }
    }

    [PunRPC]
    void ShowEmojiRPC(int idx, float life)
    {
        if (emojiImage == null) return;
        if (idx < 0 || idx >= emojis.Length) return;

        // 이모지 이미지 설정 및 타이머 초기화
        emojiImage.sprite = emojis[idx];
        emojiImage.enabled = true;
        emojiTimer = life;
    }
}