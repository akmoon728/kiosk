using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script3_PaymentManager.cs 스타일 정리 버전
/// - 카드 / 포인트 / 애플페이 / 삼성페이 선택
/// - 주문목록 팝업 생성
/// - 결제안내 팝업 생성
/// - 결제 완료 후 완료 패널 이동
/// - 영수증 생성
/// 
// 결제 흐름
// 1) 결제수단 선택
// 2) 결제 버튼 클릭
// 3) 결제안내 팝업 표시
// 4) 안내 팝업에서 확인
// 5) CompletePanel 이동
/// </summary>
public class Script3_PaymentManager : MonoBehaviour
{
    [Header("외부 참조")]
    public Script1_PanelManager panelManager;     // 패널/팝업 전환용
    public Script2_ProductManager productManager; // 장바구니 데이터 참조용

    [Header("결제 UI")]
    public Button cardBtn;            // 카드 결제 버튼
    public Button pointBtn;           // 포인트 결제 버튼
    public Button applePayBtn;        // 애플페이 버튼
    public Button samsungPayBtn;      // 삼성페이 버튼
    public Text payAmountText;        // 결제금액 표시 텍스트
    public Button viewOrderBtn;       // 주문목록 보기 버튼
    public Button confirmBtn;         // 결제 진행 버튼
    public Button backBtn;            // 상품 화면으로 돌아가기 버튼
    public Button paymentNextBtn;     // confirmBtn 대체용 다음 버튼

    /*[Header("선택 표시 이미지")]
    public Image cardSelectImg;       // 카드 선택 강조 이미지
    public Image pointSelectImg;      // 포인트 선택 강조 이미지
    public Image applePaySelectImg;   // 애플페이 선택 강조 이미지
    public Image samsungPaySelectImg; // 삼성페이 선택 강조 이미지*/

    [Header("결제 안내 팝업")]
    public Text paymentGuideTitleText;    // 팝업 제목
    public Text paymentGuideMessageText;  // 팝업 안내 문구
    public Button paymentGuideConfirmBtn; // 팝업 확인 버튼

    [Header("주문목록 팝업")]
    public Text orderDeliveryText;    // 수령방법(픽업/배송)
    public Transform orderListContent;// 주문 목록 생성 위치
    public Text orderTotalText;       // 총 결제금액
    public Text orderPayMethodText;   // 선택된 결제수단
    public Button orderConfirmBtn;    // 주문목록 팝업 내 결제 버튼
    public GameObject orderRowPrefab; // 주문 항목 프리팹

    [Header("완료 / 영수증")]
    public Text successText;          // 완료 메시지
    public Text receiptContentText;   // 영수증 내용
    public Button receiptBtn;         // 영수증 팝업 버튼
    public Button homeBtn;            // 홈으로 이동 버튼;

    private readonly List<GameObject> orderRows = new();
    private string payMethod = string.Empty;     // 현재 선택한 결제수단

    private static readonly Color SelectedColor = new Color(0.18f, 0.42f, 0.31f, 1f);
    private static readonly Color ClearColor = new Color(1f, 1f, 1f, 0f);

    private void Start()
    {
        BindButtons();
        ResetPaymentState();
    }

    /// <summary>
    /// 결제 관련 버튼 이벤트를 연결한다.
    /// </summary>
    private void BindButtons()
    {
        AddClick(cardBtn, () => SelectPay("카드"));
        AddClick(pointBtn, () => SelectPay("포인트"));
        AddClick(applePayBtn, () => SelectPay("애플페이"));
        AddClick(samsungPayBtn, () => SelectPay("삼성페이"));

        AddClick(viewOrderBtn, OpenOrderPopup);
        AddClick(confirmBtn, OpenPaymentGuidePopup);
        AddClick(paymentNextBtn, OpenPaymentGuidePopup);
        AddClick(paymentGuideConfirmBtn, FinalizePayment);
        AddClick(orderConfirmBtn, OpenPaymentGuidePopup);
        AddClick(backBtn, () => panelManager.GoToProduct());
        //AddClick(receiptBtn, () => panelManager.OpenPopup(panelManager.receiptPopup));
        AddClick(homeBtn, GoHome);
    }

    /// <summary>
    /// 결제 화면 진입 시 결제 상태와 결제금액을 초기화한다.
    /// </summary>
    public void OnEnterPayment()
    {
        ResetPaymentState();
        if (payAmountText != null)
            payAmountText.text = $"결제금액: {GetCartTotal():N0}원";
    }

    // 결제수단 선택 상태 초기화
    private void ResetPaymentState()
    {
        payMethod = string.Empty;
        if (confirmBtn != null) confirmBtn.interactable = false;
        //ClearSelectImages();
    }

    /// <summary>
    /// 결제수단을 선택하고 선택 표시를 갱신한다.
    /// </summary>
    private void SelectPay(string method)
    {
        payMethod = method;
        if (confirmBtn != null) confirmBtn.interactable = true;

        //ClearSelectImages();
        SetSelectImage(method);
    }

    private void SetSelectImage(string method)
    {
        throw new NotImplementedException();
    }

    // 선택한 결제수단의 강조 이미지 활성화
    /*private void SetSelectImage(string method)
    {
        if (method == "카드" && cardSelectImg != null) cardSelectImg.color = SelectedColor;
        if (method == "포인트" && pointSelectImg != null) pointSelectImg.color = SelectedColor;
        if (method == "애플페이" && applePaySelectImg != null) applePaySelectImg.color = SelectedColor;
        if (method == "삼성페이" && samsungPaySelectImg != null) samsungPaySelectImg.color = SelectedColor;
    }*/

    /// <summary>
    /// 주문목록 팝업을 생성하고 연다.
    /// </summary>
    private void OpenOrderPopup()
    {
        ClearRows(orderRows);
        BuildOrderPopup();
        //panelManager.OpenPopup(panelManager.orderListPopup);
    }

    // 주문목록 팝업 안에 현재 장바구니 정보를 표시
    private void BuildOrderPopup()
    {
        if (orderDeliveryText != null)
            orderDeliveryText.text = $"수령방법: {(panelManager.deliveryMethod == "pickup" ? "픽업" : "배송")}";

        if (orderPayMethodText != null)
            orderPayMethodText.text = $"결제방법: {(string.IsNullOrEmpty(payMethod) ? "미선택" : payMethod)}";

        if (orderListContent == null || orderRowPrefab == null)
        {
            if (orderTotalText != null) orderTotalText.text = $"합계: {GetCartTotal():N0}원";
            return;
        }

        int total = 0;
        foreach (var item in productManager.cart)
        {
            var row = Instantiate(orderRowPrefab, orderListContent);
            var texts = row.GetComponentsInChildren<Text>();

            SetText(texts, 0, item.name);
            SetText(texts, 1, item.OptionSummary());
            SetText(texts, 2, $"{item.quantity}개");
            SetText(texts, 3, $"{item.totalPrice:N0}원");

            orderRows.Add(row);
            total += item.totalPrice;
        }

        if (orderTotalText != null) orderTotalText.text = $"합계: {total:N0}원";
    }

    /// <summary>
    /// 선택한 결제수단에 맞는 안내 팝업을 띄운다.
    /// </summary>
    private void OpenPaymentGuidePopup()
    {
        if (string.IsNullOrEmpty(payMethod)) return;

        if (paymentGuideTitleText != null)
            paymentGuideTitleText.text = $"{payMethod} 결제";

        if (paymentGuideMessageText != null)
        {
            switch (payMethod)
            {
                case "카드":
                    paymentGuideMessageText.text = "카드를 넣어주세요. 또는 카드 단말기에 태그해주세요.";
                    break;
                case "포인트":
                    paymentGuideMessageText.text = "포인트를 사용합니다. 확인 버튼을 눌러 결제를 완료해주세요.";
                    break;
                case "애플페이":
                    paymentGuideMessageText.text = "아이폰 또는 애플워치를 단말기에 대주세요.";
                    break;
                case "삼성페이":
                    paymentGuideMessageText.text = "휴대폰을 단말기에 대주세요.";
                    break;
                default:
                    paymentGuideMessageText.text = "결제를 진행해주세요.";
                    break;
            }
        }

        //panelManager.OpenPopup(panelManager.paymentGuidePopup);
    }

    /// <summary>
    /// 결제안내 팝업 확인 후 결제를 완료한다.
    /// </summary>
    private void FinalizePayment()
    {
        //panelManager.ClosePopup(panelManager.paymentGuidePopup);
        //panelManager.ClosePopup(panelManager.orderListPopup);

        BuildReceipt();

        if (successText != null)
            successText.text = "결제 완료! 🍎";

        panelManager.GoToComplete();
    }

    /// <summary>
    /// 영수증 문자열을 생성한다.
    /// </summary>
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
            sb.AppendLine(item.name);
            sb.AppendLine($"  옵션: {item.OptionSummary()}");
            sb.AppendLine($"  수량: {item.quantity}개 / 금액: {item.totalPrice:N0}원");
            total += item.totalPrice;
        }

        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"합계: {total:N0}원");
        sb.AppendLine($"결제: {payMethod}");

        receiptContentText.text = sb.ToString();
    }

    // 첫 화면으로 돌아갈 때 상태 초기화
    public void GoHome()
    {
        productManager.ClearCart();
        ResetPaymentState();
        panelManager.GoToIntro();
    }

    // 현재 장바구니 총액 계산
    private int GetCartTotal()
    {
        int total = 0;
        foreach (var item in productManager.cart) total += item.totalPrice;
        return total;
    }

    // 선택 표시 이미지 초기화
    /*private void ClearSelectImages()
    {
        if (cardSelectImg != null) cardSelectImg.color = ClearColor;
        if (pointSelectImg != null) pointSelectImg.color = ClearColor;
        if (applePaySelectImg != null) applePaySelectImg.color = ClearColor;
        if (samsungPaySelectImg != null) samsungPaySelectImg.color = ClearColor;
    }*/

    // 주문목록에 생성한 행 제거
    private void ClearRows(List<GameObject> rows)
    {
        foreach (var row in rows) Destroy(row);
        rows.Clear();
    }

    private void AddClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private void SetText(Text[] texts, int index, string value)
    {
        if (texts.Length > index) texts[index].text = value;
    }
}
