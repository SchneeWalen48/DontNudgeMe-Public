using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillItemAwakeRegister : MonoBehaviour
{
    [SerializeField] SkillItemPool siPool;

    private void Awake()
    {
        foreach (var si in siPool.pool)
        {
            SkillItemDatabase.SkillItemRegister(si);
        }
    }

}
