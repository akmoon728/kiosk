using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  PaymentManager_v3.cs
//  역할:
//  - 결제 방법 선택
//  - 결제금액 갱신
//  - 결제목록 팝업 표시
//  - 주문 완료 후 영수증 생성
//  - 홈으로 돌아가기 시 전체 초기화
// ================================================================
public class PaymentManager_v3 : MonoBehaviour
{
    [Header("매니저 참조")]
    public PanelManager_v3 panelManager;
    public ProductManager_v3 productManager;

    [Header("PaymentPanel UI")]
    public Button cardBtn;
    public Button cashBtn;
    public Button pointBtn;
    public Text payAmountText;
    public Button viewOrderBtn;
    public Button confirmBtn;
    public Button backBtn;

    [Header("결제방법 선택 표시(선택사항)")]
    public Image cardSelectImg;
    public Image cashSelectImg;
    public Image pointSelectImg;

    [Header("결제목록 팝업 UI")]
    public Text orderDeliveryText;
    public Transform orderListContent;
    public Text orderTotalText;
    public Text orderPayMethodText;
    public Button orderConfirmBtn;
    public GameObject orderRowPrefab;

    [Header("CompletePanel / ReceiptPopup UI")]
    public Text successText;
    public Text receiptContentText;
    public Button receiptBtn;
    public Button homeBtn;

    private readonly List<GameObject> _orderRows = new List<GameObject>();
    private string _payMethod = "";

    private static readonly Color COL_ON = new Color(0.18f, 0.42f, 0.31f, 1f);
    private static readonly Color COL_OFF = new Color(1f, 1f, 1f, 0f);

    void Start()
    {
        if (cardBtn) cardBtn.onClick.AddListener(() => SelectPay("카드"));
        if (cashBtn) cashBtn.onClick.AddListener(() => SelectPay("현금"));
        if (pointBtn) pointBtn.onClick.AddListener(() => SelectPay("포인트"));

        if (viewOrderBtn) viewOrderBtn.onClick.AddListener(OpenOrderListPopup);
        if (confirmBtn) confirmBtn.onClick.AddListener(Confirm);
        if (orderConfirmBtn) orderConfirmBtn.onClick.AddListener(Confirm);
        if (backBtn) backBtn.onClick.AddListener(panelManager.GoToProduct);
        if (receiptBtn) receiptBtn.onClick.AddListener(() => panelManager.OpenPopup(panelManager.receiptPopup));
        if (homeBtn) homeBtn.onClick.AddListener(GoHome);

        if (confirmBtn) confirmBtn.interactable = false;
        ClearSelectImages();
    }

    public void OnEnterPayment()
    {
        _payMethod = "";
        if (confirmBtn) confirmBtn.interactable = false;
        ClearSelectImages();

        int total = GetCartTotal();
        if (payAmountText) payAmountText.text = $"결제금액: {total:N0}원";
    }

    private void SelectPay(string method)
    {
        _payMethod = method;
        if (confirmBtn) confirmBtn.interactable = true;

        ClearSelectImages();
        if (method == "카드" && cardSelectImg) cardSelectImg.color = COL_ON;
        if (method == "현금" && cashSelectImg) cashSelectImg.color = COL_ON;
        if (method == "포인트" && pointSelectImg) pointSelectImg.color = COL_ON;
    }

    private void OpenOrderListPopup()
    {
        foreach (var row in _orderRows) Destroy(row);
        _orderRows.Clear();

        if (orderDeliveryText)
            orderDeliveryText.text = $"수령방법: {(panelManager.deliveryMethod == "pickup" ? "픽업" : "배송")}";

        if (orderPayMethodText)
            orderPayMethodText.text = $"결제방법: {(string.IsNullOrEmpty(_payMethod) ? "미선택" : _payMethod)}";

        int total = 0;
        if (orderListContent != null && orderRowPrefab != null)
        {
            foreach (var item in productManager.cart)
            {
                GameObject row = Instantiate(orderRowPrefab, orderListContent);
                Text[] txts = row.GetComponentsInChildren<Text>();

                if (txts.Length > 0) txts[0].text = item.name;
                if (txts.Length > 1) txts[1].text = item.GetOptionSummary();
                if (txts.Length > 2) txts[2].text = $"{item.quantity}개";
                if (txts.Length > 3) txts[3].text = $"{item.totalPrice:N0}원";

                _orderRows.Add(row);
                total += item.totalPrice;
            }
        }

        if (orderTotalText) orderTotalText.text = $"합계: {total:N0}원";
        panelManager.OpenPopup(panelManager.orderListPopup);
    }

    private void Confirm()
    {
        if (string.IsNullOrEmpty(_payMethod))
            return;

        panelManager.ClosePopup(panelManager.orderListPopup);
        BuildReceipt();
        if (successText) successText.text = "주문 완료! 🍎";
        panelManager.GoToComplete();
    }

    private void BuildReceipt()
    {
        if (receiptContentText == null) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[ Fruity Fresh 영수증 ]");
        sb.AppendLine($"일시: {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"수령: {(panelManager.deliveryMethod == "pickup" ? "픽업" : "배송")}");
        sb.AppendLine("----------------------------------------");

        int total = 0;
        foreach (var item in productManager.cart)
        {
            sb.AppendLine($"{item.name}");
            sb.AppendLine($"  옵션: {item.GetOptionSummary()}");
            sb.AppendLine($"  수량: {item.quantity}개 / 금액: {item.totalPrice:N0}원");
            total += item.totalPrice;
        }

        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"합계: {total:N0}원");
        sb.AppendLine($"결제: {_payMethod}");
        receiptContentText.text = sb.ToString();
    }

    public void GoHome()
    {
        productManager.ClearCart();
        _payMethod = "";
        if (confirmBtn) confirmBtn.interactable = false;
        ClearSelectImages();
        panelManager.GoToIntro();
    }

    private int GetCartTotal()
    {
        int total = 0;
        foreach (var item in productManager.cart)
            total += item.totalPrice;
        return total;
    }

    private void ClearSelectImages()
    {
        if (cardSelectImg) cardSelectImg.color = COL_OFF;
        if (cashSelectImg) cashSelectImg.color = COL_OFF;
        if (pointSelectImg) pointSelectImg.color = COL_OFF;
    }
}
