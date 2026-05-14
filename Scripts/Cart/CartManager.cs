using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 장바구니 화면 – 아이템 목록, 삭제, 합계, 결제 이동
/// </summary>
public class CartManager : MonoBehaviour
{
    [Header("UI")]
    public Transform   itemListParent;    // Scroll View > Content
    public GameObject  cartItemPrefab;    // CartItemRow 프리팹
    public Text        totalPriceText;
    public Text        deliveryWarningText;

    [Header("버튼")]
    public Button      goPaymentButton;
    public Button      backMenuButton;

    private List<GameObject> rowObjects = new List<GameObject>();

    private void Start()
    {
        goPaymentButton.onClick.AddListener(GoToPayment);
        backMenuButton.onClick.AddListener( () => SceneManager.LoadScene("MenuScene"));
        RenderCart();
    }

    public void RenderCart()
    {
        // 기존 row 제거
        foreach (var obj in rowObjects) Destroy(obj);
        rowObjects.Clear();

        var items = OrderSession.Instance.CartItems;

        foreach (var item in items)
        {
            var row = Instantiate(cartItemPrefab, itemListParent);
            row.GetComponent<CartItemRow>().Setup(item, this);
            rowObjects.Add(row);
        }

        UpdateTotal();
    }

    public void UpdateTotal()
    {
        int total = OrderSession.Instance.TotalPrice;
        totalPriceText.text = $"합계: {total:N0}원";

        bool validDelivery = OrderSession.Instance.ValidateDeliveryWeight();
        deliveryWarningText.gameObject.SetActive(!validDelivery);
        goPaymentButton.interactable = validDelivery;
    }

    private void GoToPayment()
    {
        if (OrderSession.Instance.CartItems.Count == 0) return;
        SceneManager.LoadScene("PaymentScene");
    }
}
