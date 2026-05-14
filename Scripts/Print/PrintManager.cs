using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 출력 화면 – 주문서 + 영수증 생성 및 출력
/// </summary>
public class PrintManager : MonoBehaviour
{
    [Header("탭 버튼")]
    public Button orderSheetTabButton;  // 주문서 탭
    public Button receiptTabButton;     // 영수증 탭

    [Header("출력 영역")]
    public Text   documentText;         // ScrollView > Text (주문서 or 영수증)

    [Header("버튼")]
    public Button printButton;          // 출력 (실제 프린터 연동 가능)
    public Button newOrderButton;       // 새 주문 (인트로로 이동)

    private bool _isOrderSheet = true;

    private void Start()
    {
        orderSheetTabButton.onClick.AddListener(() => ShowDocument(true));
        receiptTabButton.onClick.AddListener(   () => ShowDocument(false));
        printButton.onClick.AddListener(PrintDocument);
        newOrderButton.onClick.AddListener(StartNewOrder);

        ShowDocument(true); // 기본: 주문서
    }

    private void ShowDocument(bool isOrderSheet)
    {
        _isOrderSheet    = isOrderSheet;
        documentText.text = isOrderSheet ? BuildOrderSheet() : BuildReceipt();
    }

    // ───────────────────────────────────────────────
    // 주문서 생성
    // ───────────────────────────────────────────────
    private string BuildOrderSheet()
    {
        var sb = new StringBuilder();
        var session = OrderSession.Instance;
        string now  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        sb.AppendLine("=============================");
        sb.AppendLine("          📋 주 문 서          ");
        sb.AppendLine("=============================");
        sb.AppendLine($"주문 일시 : {now}");
        sb.AppendLine($"수령 방법 : {(session.DeliveryMethod == "pickup" ? "픽업" : "배송")}");
        sb.AppendLine("-----------------------------");
        sb.AppendLine($"{"상품명",-12} {"옵션",-24} {"수량",4} {"금액",8}");
        sb.AppendLine("-----------------------------");

        foreach (var item in session.CartItems)
        {
            string line = $"{item.ProductName,-12} {item.OptionSummary,-24} {item.Quantity,4} {item.UnitPrice * item.Quantity,7:N0}원";
            sb.AppendLine(line);
        }

        sb.AppendLine("-----------------------------");
        sb.AppendLine($"{"합  계",-36} {session.TotalPrice,7:N0}원");
        sb.AppendLine("=============================");

        return sb.ToString();
    }

    // ───────────────────────────────────────────────
    // 영수증 생성
    // ───────────────────────────────────────────────
    private string BuildReceipt()
    {
        var sb = new StringBuilder();
        var session = OrderSession.Instance;
        string now  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int rawTotal = 0;
        foreach (var item in session.CartItems)
            rawTotal += item.UnitPrice * item.Quantity;

        sb.AppendLine("=============================");
        sb.AppendLine("          🍎 영 수 증          ");
        sb.AppendLine("=============================");
        sb.AppendLine("  과일가게 키오스크");
        sb.AppendLine($"  일시: {now}");
        sb.AppendLine("-----------------------------");

        foreach (var item in session.CartItems)
            sb.AppendLine($"  {item.ProductName} x{item.Quantity}  {item.UnitPrice * item.Quantity:N0}원");

        sb.AppendLine("-----------------------------");
        sb.AppendLine($"  소계       : {rawTotal:N0}원");

        if (session.UsedPoints > 0)
            sb.AppendLine($"  포인트 할인: -{session.UsedPoints:N0}P");

        sb.AppendLine($"  결제 금액  : {session.TotalPrice:N0}원");
        sb.AppendLine($"  결제 방법  : {PaymentLabel(session.PaymentMethod)}");
        sb.AppendLine("=============================");
        sb.AppendLine("  감사합니다! 또 방문해주세요 😊");
        sb.AppendLine("=============================");

        return sb.ToString();
    }

    private string PaymentLabel(string m) => m switch
    {
        "cash"  => "입금",
        "card"  => "카드",
        "point" => "포인트",
        _       => "기타"
    };

    private void PrintDocument()
    {
        // 실제 프린터 연동 시 이 메서드에서 SDK 호출
        // 현재는 콘솔 로그로 대체
        Debug.Log(_isOrderSheet
            ? "[출력] 주문서 인쇄 요청"
            : "[출력] 영수증 인쇄 요청");
        Debug.Log(documentText.text);
    }

    private void StartNewOrder()
    {
        OrderSession.Instance.ClearCart();
        SceneManager.LoadScene("IntroScene");
    }
}
