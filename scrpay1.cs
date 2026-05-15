using UnityEngine;
using UnityEngine.UI;

public class scrpay1 : MonoBehaviour
{
    [Header("패널 설정")]//
    public GameObject[] panels;
    private int currentPanelIndex = 0;

    [Header("버튼들 (결제수단)")]
    public Button[] choiceButtons;

    [Header("버튼들 (결과창 전용)")]
    public Button receiptButton;
    public Button onlyNumberButton;

    [Header("설정")]
    public int customerNumber = 100;

    void Start()
    {
        // 첫 번째 패널 활성화
        ShowPanel(0);

        // 1. 결제 수단 버튼들 이벤트 연결 (결제 수단 선택 시 다음 패널로)
        foreach (Button btn in choiceButtons)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(GoToNextPanel);
            }
        }

        // 2. 영수증/번호표 버튼 이벤트 연결
        if (receiptButton != null)
        {
            receiptButton.onClick.AddListener(OnClickReceipt);
        }

        if (onlyNumberButton != null)
        {
            onlyNumberButton.onClick.AddListener(OnClickOnlyNumber);
        }
    }

    public void GoToNextPanel()
    {
        if (currentPanelIndex < panels.Length - 1)
        {
            currentPanelIndex++;
            ShowPanel(currentPanelIndex);
            Debug.Log($"{currentPanelIndex}번 패널로 이동합니다.");
        }
        else
        {
            Debug.Log("더 이상 넘어갈 패널이 없습니다.");
        }
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(i == index);
            }
        }
    }

    // [영수증 출력] 버튼 클릭 시 실행
    private void OnClickReceipt()
    {
        Debug.Log("영수증이 출력 됐습니다.");
        Debug.Log(customerNumber + "번 고객님 감사합니다.");
        GoToNextPanel();
    }

    // [주문번호만 출력] 버튼 클릭 시 실행
    private void OnClickOnlyNumber()
    {
        Debug.Log(customerNumber + "번 고객님 감사합니다.");
        GoToNextPanel();
    }
}
