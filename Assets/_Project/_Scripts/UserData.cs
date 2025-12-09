using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserData
{
    //아래 모든 정보는 JSON으로 바꿔서 DB에 입력하고, DB에서 JSON으로 받아와 여기에 주입할 것임.

    public static UserData Local { get; set; }

    

    public string userName;
    public int level = 1;
    public float exp = 0;
    public float nextExp = 1000;
    public bool isGuest;

    //추가 - 칭호(칭호 기능을 위해 추가합니다)
    public string userTitle;


    [NonSerialized] public Action<UserData> onDataChanged;

    public UserData(bool isGuest = false)
    {
        this.isGuest = isGuest;
    }

    public void ChangeUserName(string newName)
    {
        this.userName = newName;
        onDataChanged?.Invoke(this);
    }

    public void ChangeUserTitle(string newTitle)
    {
        this.userTitle = newTitle;
        onDataChanged?.Invoke(this);
    }

    /// <summary>
    /// 유저가 경험치를 획득하는 처리를 말함. (로컬에서 먼저 처리 후 파이어베이스DB에 올림)
    /// </summary>
    /// <param name="exp">획득할 경험치의 총량</param>
    public async Task GainExp(float exp)
    {
        this.exp += exp;
        if (this.exp >= nextExp)
        {
            this.exp -= nextExp;
            nextExp += 100;
            await LevelUp();
        } else
        {
            onDataChanged?.Invoke(this);
            await FirebaseManager.Instance.UpdateUserData(this);
        }
    }

    /// <summary>
    /// 유저가 레벨업하는 처리. (로컬에서 먼저 처리 후 파이어베이스DB에 올림)
    /// </summary>
    /// <returns></returns>
    public async Task LevelUp()
    {
        level++;
        onDataChanged?.Invoke(this);
        await FirebaseManager.Instance.UpdateUserData(this);
    }

    public static async void RefetchUserData()
    {
        await FirebaseManager.Instance.RefetchUserData();
        UserData.Local.onDataChanged?.Invoke(UserData.Local);
    }
}

