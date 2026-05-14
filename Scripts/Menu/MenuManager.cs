using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 메뉴 화면 – 탭 전환 및 상품 목록 로드
/// 카테고리: 일반과일 / 컵과일 / 선물세트
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("탭 버튼")]
    public Button tabNormal;
    public Button tabCup;
    public Button tabGift;

    [Header("패널")]
    public GameObject panelNormal;
    public GameObject panelCup;
    public GameObject panelGift;

    [Header("장바구니 이동")]
    public Button cartButton;
    public Text   cartBadgeText;  // 장바구니 아이템 수 표시

    private void Start()
    {
        tabNormal.onClick.AddListener(() => SwitchTab(0));
        tabCup.onClick.AddListener(   () => SwitchTab(1));
        tabGift.onClick.AddListener(  () => SwitchTab(2));
        cartButton.onClick.AddListener(GoToCart);

        SwitchTab(0);
        RefreshBadge();
    }

    private void OnEnable() => RefreshBadge();

    public void SwitchTab(int index)
    {
        panelNormal.SetActive(index == 0);
        panelCup.SetActive(   index == 1);
        panelGift.SetActive(  index == 2);
    }

    public void RefreshBadge()
    {
        int count = OrderSession.Instance.CartItems.Count;
        cartBadgeText.text = count > 0 ? count.ToString() : "";
    }

    private void GoToCart()
    {
        if (OrderSession.Instance.CartItems.Count == 0)
        {
            Debug.Log("장바구니가 비어 있습니다.");
            return;
        }
        SceneManager.LoadScene("CartScene");
    }
}
