using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
#region 신버전
public class PlayerSkillItemInventory : MonoBehaviourPunCallbacks
{
    [SerializeField] int maxSlots = 2;

    private SkillItemUIManager siUIManager;
    private List<SkillItemData> skillItemSlots = new List<SkillItemData>();
    private const string INVENTORY_KEY = "items";

    private void Awake()
    {
        siUIManager = FindObjectOfType<SkillItemUIManager>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            // 로컬플레이어가 이미 커스텀프로퍼티를 가지고 있으면 불러오기
            LoadFromProperties();
            UpdateSkillItemUISlot();
        }
    }

    // --PlayerController에서 Update 부분 아이템 뭔지 판단하려는 메서드
    public SkillItemData PeekSkillItem(int index)
    {
        if (index < 0 || index >= skillItemSlots.Count) return null;
        return skillItemSlots[index];
    }

    // --아이템 추가
    public void AddSkillItem(SkillItemData item)
    {
        if (!photonView.IsMine) return;
        if (skillItemSlots.Count >= maxSlots) return;

        skillItemSlots.Add(item);

        SaveToProperties();
        UpdateSkillItemUISlot();
    }

    // --슬롯 스왑
    public void SwapSlots(int indexA, int indexB)
    {
        if (!photonView.IsMine) return;
        if (indexA < 0 || indexA >= skillItemSlots.Count) return;
        if (indexB < 0 || indexB >= skillItemSlots.Count) return;

        SkillItemData temp = skillItemSlots[indexA];
        skillItemSlots[indexA] = skillItemSlots[indexB];
        skillItemSlots[indexB] = temp;

        SaveToProperties();
        UpdateSkillItemUISlot();
    }

    // --UI 업데이트
    private void UpdateSkillItemUISlot()
    {
        if (siUIManager != null && photonView.IsMine) siUIManager.UpdateUI(skillItemSlots);
    }

    // --커스텀 프로퍼티
    // 저장 -> Photon CustomProperties
    private void SaveToProperties()
    {
        string[] ids = skillItemSlots.Select(si => si.skillItemId).ToArray();
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[INVENTORY_KEY] = ids;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // 불러오기 <- Photon CustomProperties
    private void LoadFromProperties()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(INVENTORY_KEY, out object obj))
        {
            string[] ids = (string[])obj;
            skillItemSlots.Clear();

            foreach (string id in ids)
            {
                SkillItemData data = SkillItemDatabase.GetSkillItem(id);
                if (data != null)
                    skillItemSlots.Add(data);
            }
        }
    }

    // 게임이 끝나면 인벤토리 초기화 -> 그냥 정적 클래스를 스크립트로 따로 만듬
    //public void ClearInventory()
    //{
    //    skillItemSlots.Clear();
    //    ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
    //    props[INVENTORY_KEY] = new string[0];
    //    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    //    UpdateSkillItemUISlot();
    //}

    // --다른 플레이어의 속성 변경 감지
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (photonView.Owner == targetPlayer && changedProps.ContainsKey(INVENTORY_KEY))
        {
            LoadFromProperties();
            UpdateSkillItemUISlot();
        }
    }

    // --아이템 사용
    public void UseSkillItem(int slotIndex, Transform target = null)
    {
        if (!photonView.IsMine) return;
        if (slotIndex < 0 || slotIndex >= skillItemSlots.Count) return;

        SkillItemData siData = skillItemSlots[slotIndex];
        if (siData == null) return;

        // RPC로 아이템 활성화
        int ownerViewId = photonView.ViewID;

        int targetViewId = -1;
        if (target != null)
        {
            PhotonView targetView = target.GetComponent<PhotonView>();
            if (targetView != null) targetViewId = targetView.ViewID;
        }

        // Trap : 마스터클라이언트에게 설치 요청
        if (siData.castType == SkillItemType.EnemyInstant && siData.skillItemId == "Trap")
        {
            Vector3 spawnPos = (transform.position - transform.forward * 2f) + Vector3.up;
            photonView.RPC("RequestTrapSpawn", RpcTarget.MasterClient, siData.skillItemId, ownerViewId, spawnPos);

            skillItemSlots.RemoveAt(slotIndex);
            SaveToProperties();
            UpdateSkillItemUISlot();

            return;
        }

        // Targeted(Target == null) : 아이템 소비만
        if (siData.castType == SkillItemType.Targeted && target == null)
        {
            skillItemSlots.RemoveAt(slotIndex);
            SaveToProperties();
            UpdateSkillItemUISlot();

            return;
        }

        // Self/EnemyInstant, Targeted(Target != null) 아이템 사용
        Vector3 spawnPosDefault = transform.position;

        if (siData.skillItemPrefab == null)
        {
            skillItemSlots.RemoveAt(slotIndex);
            SaveToProperties();
            UpdateSkillItemUISlot();

            return;
        }

        GameObject gObj = PhotonNetwork.Instantiate(siData.skillItemPrefab.name, spawnPosDefault, Quaternion.identity);
        PhotonView itemView = gObj.GetComponent<PhotonView>();

        if (itemView == null)
        {
            skillItemSlots.RemoveAt(slotIndex);
            SaveToProperties();
            UpdateSkillItemUISlot();

            return;
        }

        photonView.RPC("ActivateSkillItem", RpcTarget.All, siData.skillItemId, ownerViewId, targetViewId, itemView.ViewID);

        skillItemSlots.RemoveAt(slotIndex);
        SaveToProperties();
        UpdateSkillItemUISlot();

    }


    // --RPCs
    [PunRPC]
    private void ActivateSkillItem(string skillItemId, int ownerViewId, int targetViewId, int itemViewId)
    {
        PhotonView itemView = PhotonView.Find(itemViewId); if (itemView == null) return;
        SkillItemBase siBase = itemView.GetComponent<SkillItemBase>(); if (siBase == null) return;
        PhotonView ownerView = PhotonView.Find(ownerViewId);

        Debug.Log($"{ownerViewId} Use ActivateSkillItem");

        siBase.Init(ownerView); 
        if (targetViewId != -1)
        {
            siBase.Activate(new object[] { targetViewId });
        }
        else
        {
            siBase.Activate(null);
        }
    }

    [PunRPC]
    private void RequestTrapSpawn(string skillItemId, int ownerViewId, Vector3 pos)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        SkillItemData data = SkillItemDatabase.GetSkillItem(skillItemId);
        if (data == null) return;

        Debug.Log($"{ownerViewId} Use RequestTrapSpawn");

        GameObject trap = PhotonNetwork.Instantiate(data.skillItemPrefab.name, pos, Quaternion.identity);
        PhotonView trapView = trap.GetComponent<PhotonView>();

        trapView.RPC("RpcInitTrap", RpcTarget.All, ownerViewId);
    }

}
#endregion