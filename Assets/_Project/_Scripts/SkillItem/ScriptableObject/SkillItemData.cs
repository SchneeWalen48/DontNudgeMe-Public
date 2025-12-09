using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/SkillItem/SkillItemData")]
public class SkillItemData : ScriptableObject
{
    public string skillItemId;
    public Sprite skillItemIcon;
    public GameObject skillItemPrefab;
    public SkillItemType castType;
    [Header("Area")]
    public bool isAreaEffect;
    public float radius;

}
