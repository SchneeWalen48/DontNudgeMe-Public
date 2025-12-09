using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillItemSlotUI : MonoBehaviour
{
    [SerializeField] Image icon;

    public void SetSkillItem(Sprite sprite)
    {
        icon.sprite = sprite;
        icon.enabled = true;
    }

    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
    }
}
