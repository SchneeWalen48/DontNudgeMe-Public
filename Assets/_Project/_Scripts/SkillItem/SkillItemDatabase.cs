using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 획득 흐름 
/// SkillItemBox (OnTriggerEnter) -> SkillItemPool에서 랜덤 뽑기 ->
/// PlayerSkillItemInventory.AddSkillItem() 호출
/// 인벤토리 들어가면 PlayerSkillItemInventory에 [PunRPC] SyncSkillItems()로 다른 클라이언트 동기화
/// 로컬 UI인 SkillItemUIManager 업데이트
/// 
/// 아이템 사용 흐름
/// PlayerSkillInventory.UseSkillItem() -> 타입에 따라 분기 Self.., Enemy.., Targeted. 
/// PhotonViewID 전달 -> [PunRPC] ActivateSkillItem() -> Instantiate로 SkillItemPrefab 생성 ->
/// 각 생성된 아이템은 SkillItemBase를 상속 받은 스크립트의 Init(owner), Activate 호출
/// 발사자와 타겟 모두 일괄되게 만들어봤음
/// 
/// 데이터 관리
/// SkillItemData (아이템 정보) + SkillItemPool (랜덤아이템풀)
/// SkillItemDatabase (런타임 등록 / 조회)
/// SkillItemAwakeRegister 씬 시작시 모든 아이템 등록
/// 확장성 -> 아이템 추가할 때 SkillItemData 만 만들고 SO_SkillItemPool에 넣으면 됨
/// 
/// UI
/// SkillItemUIManager + SkillItemSlotUI
/// 아이템 슬롯 아이콘만 업데이트
/// 
/// </summary>
/// 
public static class SkillItemDatabase
{
    private static Dictionary<string, SkillItemData> skillItem = new Dictionary<string, SkillItemData>();

    public static void SkillItemRegister(SkillItemData data)
    {
        if (!skillItem.ContainsKey(data.skillItemId)) skillItem.Add(data.skillItemId, data);
    }

    public static SkillItemData GetSkillItem(string id)
    {
        return skillItem[id];
    }
}
