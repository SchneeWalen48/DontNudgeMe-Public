using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Firebase.Database;
using PhotonTable = ExitGames.Client.Photon.Hashtable;
using Firebase;

public enum ItemCategory { Head, Body, Shoes }

// 파츠 하나(“아이템 옵션”): 여러 GameObject(헬멧+자켓+바지)도 한 세트로 묶을 수 있음
[Serializable]
public class ItemOption
{
    [Tooltip("외부 저장/네트워크 동기화를 위한 고유 ID (공백 X)")]
    public string id;

    [Tooltip("파츠 오브젝트")]
    public GameObject[] parts;

    [Header("Colour Option")]
    [Tooltip("URP Lit이면 _BaseColor")]
    public string colorProperty = "_BaseColor";
}

// 카테고리별 아이템 목록
[Serializable]
public class ItemCategorySet
{
    public ItemCategory category;
    public ItemOption[] options;

    [HideInInspector] public int currentIndex = -1; // -1이면 아무것도 선택 안 함
    [HideInInspector] public Color currentColor = Color.white;
}

public class CharacterCustom : MonoBehaviourPunCallbacks
{
    [Header("Categories")]
    public ItemCategorySet[] categories;

    public GameObject stickmanBody;

    // Inner cache
    private Dictionary<string, GameObject> nameToGO;
    private List<GameObject> tempList = new List<GameObject>();
    private MaterialPropertyBlock mpb;

    // Firebase
    private DatabaseReference DB => FirebaseManager.Instance.DB.RootReference;
    private DatabaseReference CustomizeDataRef => FirebaseManager.Instance.CurrentUserCustomizationDataRef;

    private HashSet<GameObject> toggleables;

    // Initialize
    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        BuildNameLookup();

        if (stickmanBody) stickmanBody.SetActive(true);

        ToggleablesList();
        SetAllOff();
    }

    void BuildNameLookup()
    {
        nameToGO = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        var all = GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            // 마지막 거 덮어쓰기 안 되게
            if (!nameToGO.ContainsKey(t.name))
                nameToGO.Add(t.name, t.gameObject);
        }
    }

    void Start()
    {
        //커스터마이즈용 캐릭터 말고, 실제 게임플레이로 네트워크 인스턴시에이트 되고 나서도 생각해야 함.
        //지금은 스타트에서 무조건 로컬 데이터를 적용시키는 것으로 처리.
        //스폰되는 LocalCharacter는 이 스크립트를 달고 있음...
        //각 클라이언트가 가진 커스텀프로퍼티를 받아오는 작업은 스테이지매니저에서 먼저 해두고,
        //얘는 해당 스테이지매니저 인스턴스가 가진 커스텀프로퍼티 목록을 받아와서 데이터를 적용시켜주면 되겠다.
        
        //TODO: 강욱 - 0929: 사실상 지금 모든 캐릭터의 외형을 로컬데이터로 한번 적용시킨 뒤에 해당 캐릭터의 커스텀프로퍼티로 반영하는 처리 중임.
        //상당히 불필요하고 더러운 처리이므로 방법 강구 필요

        if (TryGetComponent<PhotonView>(out PhotonView view))
        {
            //만약 캐릭터커스텀에 포톤뷰까지 붙어있다면? => 게임스테이지 진행 중인 상태.
            ApplyData(PhotonManager.Instance.customizationDict[view.Owner.UserId]);
        }
        else
        {
            //포톤뷰가 안 붙어있다면 => 로컬에서만 보이는 상태임.
            //그냥 로컬의 커스터마이제이션데이터 적용.
            ApplyData(CustomizationData.Local);
        }
    }

    // Stickman제외 모두 off
    private void ToggleablesList()
    {
        toggleables = new HashSet<GameObject>();
        if (categories == null) return;
        foreach (var set in categories)
        {
            if (set?.options == null) continue;
            foreach (var opt in set.options)
            {
                if (opt?.parts == null) continue;
                foreach (var go in opt.parts)
                    if (go) toggleables.Add(go);
            }
        }
    }

    void SetAllOff()
    {
        if (toggleables == null) ToggleablesList();
        foreach (var go in toggleables)
        {
            if (go) go.SetActive(false);
        }

        if (stickmanBody) stickmanBody.SetActive(true);

        foreach (var cat in categories)
        {
            cat.currentIndex = -1;
            cat.currentColor = Color.white;
        }
    }

    // Prev / Next UI
    public void Next(ItemCategory cat)
    {
        var set = GetSet(cat);
        if (set == null || set.options == null || set.options.Length == 0) return;

        int next = set.currentIndex + 1;
        if (next >= set.options.Length) next = -1; // 넘어가면 '해제' 상태

        set.currentColor = Color.white;
        ApplySelection(cat, next);

        //HACK: 0929-강욱: 다음 선택/이전 선택을 할 때마다 커스텀프로퍼티를 바꿀 필요는 없음. 저장 시점에 해야 함.
        //BroadcastToPhoton();
    }

    public void Prev(ItemCategory cat)
    {
        var set = GetSet(cat);
        if (set == null || set.options == null || set.options.Length == 0) return;

        int prev = set.currentIndex - 1;
        if (prev < -1) prev = set.options.Length - 1;
        set.currentColor = Color.white;
        ApplySelection(cat, prev);
        //HACK: 0929-강욱: 다음 선택/이전 선택을 할 때마다 커스텀프로퍼티를 바꿀 필요는 없음. 저장 시점에 해야 함.
        //BroadcastToPhoton();
    }

    ItemCategorySet GetSet(ItemCategory cat)
    {
        foreach (var s in categories) if (s.category == cat) return s;
        return null;
    }

    // Apply Items
    public void ApplySelection(ItemCategory cat, int index)
    {
        var set = GetSet(cat);
        if (set == null) return;

        // off
        if (set.currentIndex >= 0 && set.currentIndex < set.options.Length)
            ToggleItem(set.options[set.currentIndex], false);

        set.currentIndex = index;

        // on
        if (index >= 0 && index < set.options.Length)
        {
            ToggleItem(set.options[index], true);
            ApplyColor(cat, set.currentColor, set.options[index]);
        }

        if (stickmanBody) stickmanBody.SetActive(true);
    }

    bool AllCategoriesOff()
    {
        foreach (var c in categories) if (c.currentIndex >= 0) return false;
        return true;
    }

    void ToggleItem(ItemOption opt, bool on)
    {
        if (opt?.parts == null) return;

        foreach (var go in opt.parts)
        {
            if (!go) continue;
            go.SetActive(on);
        }
    }

    // Color Settings
    // 대표색만 / 전체색, 둘 다 지원. UI에서 컬러피커 값 들어오면 호출.
    private Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();

    void CacheOriginalColors(ItemOption opt, string prop)
    {
        if (opt == null || opt.parts == null) return;

        foreach (var go in opt.parts)
        {
            if (!go) continue;

            var rends = go.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                if (!originalColors.ContainsKey(r))
                {
                    var mats = r.sharedMaterials;
                    Color[] colors = new Color[mats.Length];
                    for (int i = 0; i < mats.Length; i++)
                    {
                        colors[i] = mats[i].GetColor(prop);
                    }
                    originalColors[r] = colors; // 원본 저장
                }
            }
        }
    }
    public void SetColor(ItemCategory cat, Color color, bool overrideAllMats = false)
    {
        var set = GetSet(cat);
        if (set == null) return;

        set.currentColor = color;

        if (set.currentIndex >= 0 && set.currentIndex < set.options.Length)
        {
            var opt = set.options[set.currentIndex];
            ApplyColor(cat, color, opt);
        }
    }

    void ApplyColor(ItemCategory cat, Color color, ItemOption opt)
    {
        if (opt == null || opt.parts == null) return;

        foreach (var go in opt.parts)
        {
            if (!go || !go.activeInHierarchy) continue;

            var rends = go.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                string prop =
                    HasColor(r, opt.colorProperty) ? opt.colorProperty :
                    (HasColor(r, "_BaseColor") ? "_BaseColor" :
                    (HasColor(r, "_Color") ? "_Color" : null));

                if (prop == null) continue;

                // 원래 색상 캐싱
                CacheOriginalColors(opt, prop);

                // 원래 색상 가져오기
                if (!originalColors.ContainsKey(r)) continue;
                Color[] originals = originalColors[r];
                var mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Color baseColor = originals[i];  // 항상 원본 기준
                    Color multiplied = baseColor * color;
                    mats[i].SetColor(prop, multiplied);
                    //Color baseColor = originals[i];
                    //Color.RGBToHSV(baseColor, out float hBase, out float s, out float v);
                    //Color.RGBToHSV(color, out float hNew, out _, out _);

                    //// Hue만 교체, 원래 S/V 유지
                    //Color final = Color.HSVToRGB(hNew, s, v);

                    //mats[i].SetColor(prop, final);
                }
            }
        }
    }

    bool HasColor(Renderer r, string prop)
    {
        if (string.IsNullOrEmpty(prop)) return false;
        var mats = r.sharedMaterials;
        if (mats == null) return false;
        foreach (var m in mats)
        {
            if (!m) continue;
            if (m.HasProperty(prop)) return true;
        }
        return false;
    }

    //TODO: 로직 수정 필요함. 무조건 리셋하는 게 아니라, 로컬의 커스터마이징 정보를 받아와서 리셋해야 함.
    //CustomizationData.Local이 의미하는 것: 초기 Firebase DB에서 받아온 이 클라이언트 인스턴스만의 CustomizationData
    //TODO: 커스터마이징 창을 그냥 닫으면 색상이 리셋되는 버그가 있음. 로직에서 해당 처리하는 부분을 찾아서 좀 바꿔줘야 할 것같음.
    public void ResetCustomization()
    {
        //HACK: 0928-강욱: 로컬 커스터마이제이션 데이터가 있으면 로컬의 데이터를 적용하는 로직으로 덮어씌웁니다.
        //아마 else문 아래로는 정상적인 상황이라면 실행 안 될 것임
        if (CustomizationData.Local != null)
        {
            ApplyData(CustomizationData.Local);
        }

        
        else
        {
            foreach (var kv in originalColors)
            {
                Renderer r = kv.Key;
                Color[] origs = kv.Value;

                var mats = r.materials;
                for (int i = 0; i < mats.Length && i < origs.Length; i++)
                {
                    if (mats[i] != null && mats[i].HasProperty("_BaseColor"))
                        mats[i].SetColor("_BaseColor", origs[i]);
                }
            }

            foreach (var cat in categories)
            {
                if (cat.currentIndex >= 0 && cat.currentIndex < cat.options.Length)
                    ToggleItem(cat.options[cat.currentIndex], false);

                cat.currentIndex = -1;
                cat.currentColor = Color.white;
            }
        }

        if (stickmanBody) stickmanBody.SetActive(true);
    }
    public void ResetColors()
    {
        foreach (var kv in originalColors)
        {
            Renderer r = kv.Key;
            Color[] originals = kv.Value;
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor("_BaseColor", originals[i]);
            }
        }

        // UI 상태도 초기화
        foreach (var cat in categories)
        {
            cat.currentColor = Color.white;
        }
    }

    // Firebase Realtime DB (Save/Load)
    public async Task SaveToFirebase()
    {
        try
        {
            var data = BuildData();
            string json = JsonUtility.ToJson(data);
            await CustomizeDataRef.SetRawJsonValueAsync(json);
            Debug.Log("[Customizer] Saved to Firebase: " + json);

            //여기서 문제가 안 터졌으면 잘 저장이 된 것이므로
            //로컬 커스터마이징데이터를 덮어씌움.
            CustomizationData.Local = data;
            Debug.Log("[Customizer] Override Local Data: " + json);
        } catch (FirebaseException fe)
        {
            Debug.Log(fe.Message);
        }
    }

    //HACK: 0929-강욱: 이제 FirebaseManager의 FetchCustomizationDataFromFirebase()가 이 일을 대신 수행합니다.
    //public async Task LoadFromFirebase(bool alsoBroadcastPhoton = true)
    //{
    //    var snap = await CustomizeDataRef.GetValueAsync();
    //    if (snap == null || !snap.Exists) return;

    //    var data = JsonUtility.FromJson<CustomizationData>(snap.GetRawJsonValue());
    //    ApplyData(data);

    //    if (alsoBroadcastPhoton) BroadcastToPhoton();

    //    Debug.Log("[Customizer] Loaded from Firebase.");
    //}

    // PUN2 동기화
    void BroadcastToPhoton()
    {
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.LocalPlayer.SetCustomProperties(BuildData().ToPhoton());
    }

    //HACK: 0929-강욱: 필요 없습니다.
    //public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonTable changedProps)
    //{
    //    if (photonView != null && targetPlayer == photonView.Owner)
    //    {
    //        if (CustomizationData.TryFromPhoton(changedProps, out var data))
    //            ApplyData(data);
    //    }
    //}

    // 직렬화/역직렬화
    CustomizationData BuildData()
    {
        var d = new CustomizationData();
        foreach (var set in categories)
        {
            switch (set.category)
            {
                case ItemCategory.Head:
                    d.headId = IdOf(set);
                    d.headColor = ColorUtil.ToHex(set.currentColor);
                    d.headAll = CurrentRecolorAll(set);
                    break;
                case ItemCategory.Body:
                    d.bodyId = IdOf(set);
                    d.bodyColor = ColorUtil.ToHex(set.currentColor);
                    d.bodyAll = CurrentRecolorAll(set);
                    break;
                case ItemCategory.Shoes:
                    d.shoesId = IdOf(set);
                    d.shoesColor = ColorUtil.ToHex(set.currentColor);
                    d.shoesAll = CurrentRecolorAll(set);
                    break;
            }
        }
        return d;
    }

    string IdOf(ItemCategorySet set)
    {
        if (set.currentIndex < 0 || set.options == null || set.options.Length == 0) return "";
        var opt = set.options[set.currentIndex];
        return string.IsNullOrEmpty(opt.id) ? "" : opt.id;
    }

    bool CurrentRecolorAll(ItemCategorySet set)
    {
        return true;
    }

    //TODO: 이거 중요한듯.
    //파이어베이스 로그인 할 때 인증정보 받아오고 나서
    //커스터마이징 정보도 같이 받아오기.
    //커스터마이징 정보 받아오고 나면 로컬로 저장(게스트는 해당 없음.)
    //로컬로 저장된 커스터마이징 정보는 포톤 로그인하면서 커스텀프로퍼티화해서 각각의 유저에게 저장.
    //이렇게 하면 포톤으로 새로 로그인할 때마다 DB에서도 커스터마이징 정보를 끌어와서 적용하게 됨.
    //실제 게임 장면 내에서 유저들이 다른 유저의 Firebase DB를 건드리지 않고 포톤으로만 필요한 정보를 가져오기 좋은 방법 같음.
    void ApplyData(CustomizationData d)
    {
        // 전부 끄고 다시 선택
        foreach (var c in categories) ApplySelection(c.category, -1);

        ApplyOne(ItemCategory.Head, d.headId, d.headColor, d.headAll);
        ApplyOne(ItemCategory.Body, d.bodyId, d.bodyColor, d.bodyAll);
        ApplyOne(ItemCategory.Shoes, d.shoesId, d.shoesColor, d.shoesAll);

        //BroadcastToPhoton();
        if (stickmanBody) stickmanBody.SetActive(true);
    }

    void ApplyOne(ItemCategory cat, string id, string hexColor, bool all)
    {
        var set = GetSet(cat);
        if (set == null) return;

        int idx = IndexOfId(set, id);
        ApplySelection(cat, idx);

        var color = ColorUtil.FromHexOr(hexColor, Color.white);
        set.currentColor = color;

        if (idx >= 0)
        {
            ApplyColor(cat, color, set.options[idx]);
        }
    }

    int IndexOfId(ItemCategorySet set, string id)
    {
        if (string.IsNullOrEmpty(id) || set.options == null) return -1;
        for (int i = 0; i < set.options.Length; i++)
            if (set.options[i] != null && set.options[i].id == id) return i;
        return -1;
    }
}

// 직렬화 모델
[Serializable]
public class CustomizationData
{

    public static CustomizationData Local;

    public string headId;
    public string bodyId;
    public string shoesId;

    public string headColor;
    public string bodyColor;
    public string shoesColor;

    public bool headAll;
    public bool bodyAll;
    public bool shoesAll;

    public PhotonTable ToPhoton()
    {
        var t = new PhotonTable
        {
            { "hId", headId ?? "" },
            { "bId", bodyId ?? "" },
            { "sId", shoesId ?? "" },
            { "hCol", headColor ?? "" },
            { "bCol", bodyColor ?? "" },
            { "sCol", shoesColor ?? "" },
            { "hAll", headAll },
            { "bAll", bodyAll },
            { "sAll", shoesAll },
        };
        
        return t;
    }

    /// <summary>
    /// 로컬플레이어의 커스터마이징 정보(스태틱)을 포톤 커스텀프로퍼티화해서 저장.
    /// </summary>
    public static void LocalToPhotonCP()
    {
        if (Local == null) { Debug.Log("로컬 커스터마이징 데이터가 null임"); return; }
        if (Local != null) PhotonNetwork.SetPlayerCustomProperties(Local.ToPhoton());
    }

    public static bool TryFromPhoton(PhotonTable p, out CustomizationData d)
    {
        d = null;
        if (p == null) return false;
        try
        {
            d = new CustomizationData
            {
                headId = p.TryGetValue("hId", out var hId) ? (string)hId : "",
                bodyId = p.TryGetValue("bId", out var bId) ? (string)bId : "",
                shoesId = p.TryGetValue("sId", out var sId) ? (string)sId : "",
                headColor = p.TryGetValue("hCol", out var hC) ? (string)hC : "",
                bodyColor = p.TryGetValue("bCol", out var bC) ? (string)bC : "",
                shoesColor = p.TryGetValue("sCol", out var sC) ? (string)sC : "",
                headAll = p.TryGetValue("hAll", out var ha) && (bool)ha,
                bodyAll = p.TryGetValue("bAll", out var ba) && (bool)ba,
                shoesAll = p.TryGetValue("sAll", out var sa) && (bool)sa,
            };
            return true;

        }
        catch { return false; }
    }
}

public static class ColorUtil
{
    // #RRGGBB or #RRGGBBAA
    public static string ToHex(Color c)
    {
        Color32 c32 = c;
        return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}{c32.a:X2}";
    }

    public static Color FromHexOr(string hex, Color fallback)
    {
        if (string.IsNullOrEmpty(hex)) return fallback;
        if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
        return fallback;
    }
}
