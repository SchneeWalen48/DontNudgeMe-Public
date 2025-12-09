using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillItemUIManager : MonoBehaviour
{
    [SerializeField] SkillItemSlotUI[] slotUIs;

    public void UpdateUI(List<SkillItemData> items)
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (i < items.Count) slotUIs[i].SetSkillItem(items[i].skillItemIcon);
            else slotUIs[i].Clear();
        }
    }
}
