using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  Script 3 : PaymentManager.cs
//  [ 결제 처리 + 결제목록 팝업 + 영수증 팝업 ]
//
//  담당 역할:
//  - 결제 방법 선택 (카드/현금/포인트)
//  - 결제 확인 → CompletePanel 이동
//  - 결제목록 팝업: 결제 전 최종 확인 팝업
//  - 영수증 팝업: 결제 완료 후 영수증 표시
//  - 홈으로: 장바구니 초기화 + StartPanel 이동
// ================================================================
public class PaymentManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //  PanelManager, ProductManager 참조
    //  Inspector: KioskController 오브젝트에서 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== 매니저 참조 =====")]
    public PanelManager  panelManager;     // 패널/팝업 전환
    public ProductManager productManager;  // 장바구니 데이터 접근

    // ──────────────────────────────────────────────────────────
    //  PaymentPanel 내부 UI
    //  Inspector: PaymentPanel 안의 UI 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== PaymentPanel UI =====")]
    public Button cardBtn;                 // 카드 결제 선택 버튼
    public Button cashBtn;                 // 현금 결제 선택 버튼
    public Button pointBtn;                // 포인트 결제 선택 버튼

    // 선택된 결제방법 강조 표시용 테두리 Image
    // Inspector: 각 버튼 안의 Border Image 연결
    public Image  cardBorderImg;
    public Image  cashBorderImg;
    public Image  pointBorderImg;

    public Text   payAmountText;           // "결제금액: 12,000원"
    public Button viewOrderBtn;            // "결제 목록 확인" → 결제목록 팝업 열기
    public Button confirmBtn;             // "결제하기" → CompletePanel 이동
    public Button backBtn;                 // "뒤로" → ProductPanel 이동

    // ──────────────────────────────────────────────────────────
    //  결제목록 팝업 내부 UI
    //  결제 전 최종 주문 확인 팝업
    //
    //  OrderListPopup 구조:
    //  OrderListPopup
    //  ├ TitleText        "최종 주문 확인"
    //  ├ DeliveryText     ← orderDeliveryText
    //  ├ ScrollView
    //  │  └ Content       ← orderListContent
    //  ├ TotalText        ← orderTotalText
    //  ├ PayMethodText    ← orderPayMethodText
    //  ├ ConfirmButton    ← orderConfirmBtn (여기서 최종 결제)
    //  └ CloseButton      ← PanelManager.ClosePopup
    // ──────────────────────────────────────────────────────────
    [Header("===== 결제목록 팝업 내부 UI =====")]
    public GameObject orderListPopup;      // 결제목록 팝업 오브젝트
    public Text       orderDeliveryText;   // "수령방법: 픽업" 텍스트
    public Transform  orderListContent;    // 주문 목록 Content (동적 생성 위치)
    public Text       orderTotalText;      // "합계: N원"
    public Text       orderPayMethodText;  // "결제방법: 카드"
    public Button     orderConfirmBtn;     // 팝업 안 "결제 확인" 버튼
    public Button     orderCloseBtn;       // 팝업 닫기 버튼
    public GameObject orderRowPrefab;      // 목록 한 줄 프리팹

    // ──────────────────────────────────────────────────────────
    //  영수증 팝업 내부 UI
    //  CompletePanel에서 "영수증 보기" 클릭 시 열림
    //
    //  ReceiptPopup 구조:
    //  ReceiptPopup
    //  ├ ReceiptContentText  ← receiptContentText
    //  └ CloseButton
    // ──────────────────────────────────────────────────────────
    [Header("===== 영수증 팝업 내부 UI =====")]
    public Text   receiptContentText;      // 영수증 전체 내용 텍스트

    // ──────────────────────────────────────────────────────────
    //  CompletePanel UI
    // ──────────────────────────────────────────────────────────
    [Header("===== CompletePanel UI =====")]
    public Text   successText;             // "주문 완료! 🍎"
    public Button homeBtn;                 // "홈으로" 버튼

    // ── 색상 상수 ─────────────────────────────────────────────
    private static readonly Color COL_SELECTED   = new Color(0.18f, 0.42f, 0.31f); // 선택 초록
    private static readonly Color COL_UNSELECTED = new Color(0f, 0f, 0f, 0f);      // 투명 (미선택)

    // ── 내부 상태 ─────────────────────────────────────────────
    private string _payMethod = "";        // 선택된 결제 방법

    // 생성된 주문목록 행 오브젝트 추적
    private readonly System.Collections.Generic.List<GameObject> _orderRows
        = new System.Collections.Generic.List<GameObject>();

    // =========================================================
    //  Start: 버튼 이벤트 연결
    // =========================================================
    void Start()
    {
        // ── 결제 방법 버튼 ────────────────────────────────────
        cardBtn.onClick.AddListener(  () => SelectPay("카드"));
        cashBtn.onClick.AddListener(  () => SelectPay("현금"));
        pointBtn.onClick.AddListener( () => SelectPay("포인트"));

        // ── 결제목록 팝업 버튼 ───────────────────────────────
        // "결제 목록 확인" 버튼: 팝업 열고 내용 갱신
        if (viewOrderBtn)
            viewOrderBtn.onClick.AddListener(OpenOrderListPopup);

        // 팝업 안 "결제 확인" 버튼: 실제 결제 실행
        if (orderConfirmBtn)
            orderConfirmBtn.onClick.AddListener(Confirm);

        // 팝업 닫기 버튼
        if (orderCloseBtn)
            orderCloseBtn.onClick.AddListener(() =>
                panelManager.ClosePopup(orderListPopup));

        // ── 결제하기 / 뒤로가기 버튼 ─────────────────────────
        confirmBtn.onClick.AddListener(Confirm);
        backBtn.onClick.AddListener(   () => panelManager.Show(panelManager.productPanel));

        // ── CompletePanel 버튼 ───────────────────────────────
        // 홈으로: 장바구니 초기화 후 StartPanel 이동
        homeBtn.onClick.AddListener(GoHome);

        // ── 초기 상태 ─────────────────────────────────────────
        confirmBtn.interactable = false; // 결제방법 미선택 시 비활성
    }

    // =========================================================
    //  PaymentPanel 진입 시 호출
    //  PanelManager.Show(paymentPanel) 뒤에 호출 필요
    //  또는 PanelManager.Show()에서 자동 호출하도록 연결
    // =========================================================
    public void OnEnterPayment()
    {
        // 결제 방법 초기화
        _payMethod = "";
        ClearPayBorders();
        confirmBtn.interactable = false;

        // 결제금액 최신 합계로 표시
        int total = GetTotal();
        payAmountText.text = $"결제금액: {total:N0}원";
    }

    // =========================================================
    //  결제 방법 선택
    //  선택된 버튼 테두리를 초록으로 강조
    // =========================================================
    private void SelectPay(string method)
    {
        _payMethod = method;

        // 모든 테두리 초기화 후 선택된 것만 강조
        ClearPayBorders();
        switch (method)
        {
            case "카드":
                if (cardBorderImg)  cardBorderImg.color  = COL_SELECTED; break;
            case "현금":
                if (cashBorderImg)  cashBorderImg.color  = COL_SELECTED; break;
            case "포인트":
                if (pointBorderImg) pointBorderImg.color = COL_SELECTED; break;
        }

        // 결제방법 선택 시 결제 버튼 활성화
        confirmBtn.interactable = true;
    }

    // =========================================================
    //  결제목록 팝업 열기
    //  최종 주문 내역 + 결제방법 표시
    // =========================================================
    private void OpenOrderListPopup()
    {
        // 기존 목록 삭제
        foreach (var row in _orderRows) Destroy(row);
        _orderRows.Clear();

        // 수령방법 표시
        if (orderDeliveryText)
            orderDeliveryText.text =
                $"수령방법: {(panelManager.deliveryMethod == "pickup" ? "픽업" : "배송")}";

        // 결제방법 표시
        if (orderPayMethodText)
            orderPayMethodText.text =
                $"결제방법: {(string.IsNullOrEmpty(_payMethod) ? "미선택" : _payMethod)}";

        // 주문 목록 행 동적 생성
        int total = 0;
        if (orderListContent != null && orderRowPrefab != null)
        {
            foreach (var item in productManager.cart)
            {
                var row  = Instantiate(orderRowPrefab, orderListContent);
                var txts = row.GetComponentsInChildren<Text>();

                // 텍스트 인덱스 약속:
                // 0 = 상품명, 1 = 가격
                if (txts.Length > 0) txts[0].text = item.name;
                if (txts.Length > 1) txts[1].text = $"{item.price:N0}원";

                _orderRows.Add(row);
                total += item.price;
            }
        }

        // 합계 표시
        if (orderTotalText)
            orderTotalText.text = $"합계: {total:N0}원";

        // 팝업 열기
        panelManager.OpenPopup(orderListPopup);
    }

    // =========================================================
    //  결제 확인 (최종 결제 실행)
    //  결제방법 미선택 시 실행 안 됨
    // =========================================================
    private void Confirm()
    {
        if (string.IsNullOrEmpty(_payMethod))
        {
            Debug.LogWarning("[결제] 결제방법을 먼저 선택하세요.");
            return;
        }

        // 결제목록 팝업 닫기
        panelManager.ClosePopup(orderListPopup);

        // 영수증 생성
        BuildReceipt();

        // CompletePanel로 이동
        panelManager.Show(panelManager.completePanel);

        // 완료 텍스트 설정
        if (successText) successText.text = "주문 완료! 🍎";
    }

    // =========================================================
    //  영수증 텍스트 생성
    //  CompletePanel 진입 시 receiptContentText에 내용 채움
    // =========================================================
    private void BuildReceipt()
    {
        if (receiptContentText == null) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("================================");
        sb.AppendLine("     🍎  Fruity Fresh");
        sb.AppendLine("================================");
        sb.AppendLine($"  일시: {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"  수령: {(panelManager.deliveryMethod == "pickup" ? "픽업" : "배송")}");
        sb.AppendLine("────────────────────────────────");

        int total = 0;
        foreach (var item in productManager.cart)
        {
            sb.AppendLine($"  {item.name,-10}  {item.price:N0}원");
            total += item.price;
        }

        sb.AppendLine("────────────────────────────────");
        sb.AppendLine($"  합   계:  {total:N0}원");
        sb.AppendLine($"  결제방법: {_payMethod}");
        sb.AppendLine("================================");
        sb.AppendLine("  감사합니다! 또 방문해주세요 😊");

        receiptContentText.text = sb.ToString();
    }

    // =========================================================
    //  홈으로 돌아가기
    //  장바구니 초기화 + StartPanel 이동
    // =========================================================
    private void GoHome()
    {
        // ProductManager의 장바구니 초기화
        productManager.ClearCart();

        // 결제 상태 초기화
        _payMethod = "";
        ClearPayBorders();
        confirmBtn.interactable = false;

        // StartPanel로 이동
        panelManager.Show(panelManager.startPanel);
    }

    // =========================================================
    //  결제방법 테두리 전체 초기화 헬퍼
    // =========================================================
    private void ClearPayBorders()
    {
        if (cardBorderImg)  cardBorderImg.color  = COL_UNSELECTED;
        if (cashBorderImg)  cashBorderImg.color  = COL_UNSELECTED;
        if (pointBorderImg) pointBorderImg.color = COL_UNSELECTED;
    }

    // =========================================================
    //  장바구니 합계 계산 헬퍼
    // =========================================================
    private int GetTotal()
    {
        int total = 0;
        foreach (var item in productManager.cart) total += item.price;
        return total;
    }
}
