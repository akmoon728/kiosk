using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 첫 화면 - "시작하기" 버튼 및 방법 선택 (픽업 / 배송)
/// </summary>
public class IntroManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public GameObject methodPanel;       // 픽업 / 배송 선택 패널
    public Button pickupButton;
    public Button deliveryButton;
    public Text methodWarningText;       // 배송 최소 kg 안내

    private void Start()
    {
        methodPanel.SetActive(false);
        startButton.onClick.AddListener(OnStartClicked);
        pickupButton.onClick.AddListener(() => SelectMethod("pickup"));
        deliveryButton.onClick.AddListener(() => SelectMethod("delivery"));
    }

    private void OnStartClicked()
    {
        startButton.gameObject.SetActive(false);
        methodPanel.SetActive(true);
        methodWarningText.text = "";
    }

    private void SelectMethod(string method)
    {
        OrderSession.Instance.DeliveryMethod = method;

        if (method == "delivery")
        {
            // 배송은 최소 3kg 이상 안내 (메뉴 화면에서 최종 검증)
            methodWarningText.text = "※ 배송은 3kg 이상 주문 시 이용 가능합니다.";
        }

        SceneManager.LoadScene("MenuScene");
    }
}
