using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 결제 화면 – 입금 / 카드 / 포인트 선택 및 결제 처리
/// </summary>
public class PaymentManager : MonoBehaviour
{
    [Header("결제 요약")]
    public Text totalAmountText;
    public Text methodSelectedText;

    [Header("결제 방법 버튼")]
    public Button cashButton;     // 입금
    public Button cardButton;     // 카드
    public Button pointButton;    // 포인트

    [Header("포인트 입력 패널")]
    public GameObject  pointPanel;
    public InputField  pointInputField;
    public Text        availablePointText;
    public Button      applyPointButton;
    private int _availablePoints = 5000;  // 실제 연동 시 서버 조회

    [Header("최종 결제 버튼")]
    public Button confirmButton;
    public Button backButton;

    private string _selectedMethod = "";

    private void Start()
    {
        cashButton.onClick.AddListener(  () => SelectMethod("cash"));
        cardButton.onClick.AddListener(  () => SelectMethod("card"));
        pointButton.onClick.AddListener( () => SelectMethod("point"));
        applyPointButton.onClick.AddListener(ApplyPoints);
        confirmButton.onClick.AddListener(ConfirmPayment);
        backButton.onClick.AddListener( () => SceneManager.LoadScene("CartScene"));

        pointPanel.SetActive(false);
        confirmButton.interactable = false;

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        totalAmountText.text = $"결제 금액: {OrderSession.Instance.TotalPrice:N0}원";
    }

    private void SelectMethod(string method)
    {
        _selectedMethod = method;
        OrderSession.Instance.PaymentMethod = method;
        methodSelectedText.text = $"선택된 결제 방법: {MethodLabel(method)}";

        pointPanel.SetActive(method == "point");
        if (method == "point")
        {
            availablePointText.text = $"보유 포인트: {_availablePoints:N0}P";
            pointInputField.text    = "";
            OrderSession.Instance.UsedPoints = 0;
        }

        confirmButton.interactable = (method != "point"); // 포인트는 Apply 후 활성화
        RefreshSummary();
    }

    private void ApplyPoints()
    {
        if (!int.TryParse(pointInputField.text, out int pts) || pts <= 0)
        {
            Debug.LogWarning("포인트 입력값이 올바르지 않습니다.");
            return;
        }

        pts = Mathf.Min(pts, _availablePoints, OrderSession.Instance.TotalPrice);
        OrderSession.Instance.UsedPoints = pts;
        confirmButton.interactable = true;
        RefreshSummary();
        Debug.Log($"포인트 {pts:N0}P 적용");
    }

    private void ConfirmPayment()
    {
        if (string.IsNullOrEmpty(_selectedMethod))
        {
            Debug.LogWarning("결제 방법을 선택하세요.");
            return;
        }

        Debug.Log($"[결제 완료] 방법:{_selectedMethod}, 금액:{OrderSession.Instance.TotalPrice:N0}원");
        SceneManager.LoadScene("PrintScene");
    }

    private string MethodLabel(string m) => m switch
    {
        "cash"  => "입금",
        "card"  => "카드",
        "point" => "포인트",
        _       => m
    };
}
