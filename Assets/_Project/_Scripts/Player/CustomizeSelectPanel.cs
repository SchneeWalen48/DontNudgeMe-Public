using Firebase;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeSelectPanel : MonoBehaviour
{
    public CharacterCustom customizer;
    public CustomizePanelController panelController;

    [Header("Hue Sliders")]
    public Slider head, body, shoes;

    public void NextHead() => customizer.Next(ItemCategory.Head);
    public void PrevHead() => customizer.Prev(ItemCategory.Head);
    public void NextBody() => customizer.Next(ItemCategory.Body);
    public void PrevBody() => customizer.Prev(ItemCategory.Body);
    public void NextShoes() => customizer.Next(ItemCategory.Shoes);
    public void PrevShoes() => customizer.Prev(ItemCategory.Shoes);

    // Color 실시간 미리보기 (저장 전까지 로컬만 반영)
    public void OnHeadColorChanged()
    {
        Color c = HueToColor(head.value);
        customizer.SetColor(ItemCategory.Head, c, false); 
        UpdateSliderHandleColor(head, c);
    }

    public void OnBodyColorChanged()
    {
        Color c = HueToColor(body.value);
        customizer.SetColor(ItemCategory.Body, c, false);
        UpdateSliderHandleColor(body, c);
    }

    public void OnShoesColorChanged()
    {
        Color c = HueToColor(shoes.value);
        customizer.SetColor(ItemCategory.Shoes, c, false);
        UpdateSliderHandleColor(shoes, c);
    }

    public async void OnConfirmCustomization()
    {
        // Firebase 저장 + Photon 전송
        try
        {
            await customizer.SaveToFirebase();
            //위의 과정에서 이미 CustomizationData.Local은 변함.
            //저장이 잘 되고 나면 패널컨트롤러가 커스터마이징패널을 닫도록 함(오버레이도 같이 사라짐)
            //포톤네트워크: 로컬의 커스텀프로퍼티 주작
            PhotonNetwork.SetPlayerCustomProperties(CustomizationData.Local.ToPhoton());
           
            panelController.ClosePanel();
        }
        catch (FirebaseException fe)
        {
            Debug.Log($"뭔지 몰라도 파이어베이스 에러남: {fe.Message}");
        }

        
    }

    public void OnCloseCustomization()
    {
        ResetSlidersToDefault();
        if (customizer != null) customizer.ResetCustomization();
    }
    public void Open()
    {
        // 항상 리셋하고 열기
        ResetSlidersToDefault();
        if (customizer != null) customizer.ResetCustomization();

        gameObject.SetActive(true);
    }

    void OnDisable()
    {
        // 만약 Confirm으로 저장 안 하고 그냥 닫힌 경우 → 원래대로
        ResetSlidersToDefault();
        if (customizer != null) customizer.ResetCustomization();
    }

    Color HueToColor(float v)
    {
        return Color.HSVToRGB(v, 1f, 1f); 
    }

    void UpdateSliderHandleColor(Slider slider, Color color)
    {
        if (slider.handleRect != null)
        {
            var handleImage = slider.handleRect.GetComponent<Image>();
            if (handleImage != null)
                handleImage.color = color; // 핸들 색 = 현재 선택 색상
        }
    }
    void ResetSlidersToDefault()
    {
        if (head) head.SetValueWithoutNotify(0f);
        if (body) body.SetValueWithoutNotify(0f);
        if (shoes) shoes.SetValueWithoutNotify(0f);

        UpdateSliderHandleColor(head, Color.white);
        UpdateSliderHandleColor(body, Color.white);
        UpdateSliderHandleColor(shoes, Color.white);
    }
}
