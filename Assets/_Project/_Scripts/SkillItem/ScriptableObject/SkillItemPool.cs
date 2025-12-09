using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/SkillItem/SkillItemPool")]
public class SkillItemPool : ScriptableObject
{
    public SkillItemData[] pool;

    public SkillItemData GetRandomSkillItem()
    {
        if (pool == null || pool.Length == 0) return null;
        return pool[Random.Range(0, pool.Length)];
    }
}
