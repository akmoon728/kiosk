using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script1_PanelManager.cs 스타일 정리 버전
/// - Intro / Start / Product / Payment / Complete 패널 전환 관리
/// - 일반과일 / 컵과일 / 선물세트 탭 전환 관리
/// - 옵션 / 장바구니 / 주문목록 / 영수증 / 결제안내 팝업 열기/닫기 관리
/// - 각 페이지의 다음 단계 이동 버튼 관리
/// 
/// 연결 예시
// - IntroPanel 의 시작하기 버튼 -> startIntroBtn 또는 introNextBtn
// - StartPanel 의 픽업/배송 선택 후 다음 버튼 -> startNextBtn
// - ProductPanel 의 결제하기 버튼 -> productNextBtn 또는 goPayBtn
// - PaymentPanel 의 결제 버튼 -> paymentNextBtn 또는 confirmBtn
/// </summary>
public class Script1_PanelManager : MonoBehaviour
{
    [Header("메인 패널")]
    public GameObject introPanel;          // 첫 시작 화면
    public GameObject startPanel;          // 픽업 / 배송 선택 화면
    public GameObject productPanel;        // 상품 선택 화면
    public GameObject paymentPanel;        // 결제 화면
    public GameObject completePanel;       // 결제 완료 화면

    [Header("인트로")]
    public Button startIntroBtn;           // IntroPanel 시작하기 버튼
    public Button introNextBtn;            // IntroPanel 다음 버튼(선택사항)

    [Header("시작 화면")]
    public Button pickupBtn;               // 픽업 선택 버튼
    public Button deliveryBtn;             // 배송 선택 버튼
    public Button startNextBtn;            // 상품 화면으로 이동하는 다음 버튼

    [Header("카테고리 탭")]
    public Button tabFruit;                // 일반과일 탭
    public Button tabCup;                  // 컵과일 탭
    public Button tabGift;                 // 선물세트 탭

    [Header("카테고리 패널")]
    public GameObject gridFruit;           // 일반과일 목록 영역
    public GameObject gridCup;             // 컵과일 목록 영역
    public GameObject gridGift;            // 선물세트 목록 영역

    /*[Header("탭 인디케이터")]
    public Image fruitTabBar;              // 일반과일 탭 인디케이터
    public Image cupTabBar;                // 컵과일 탭 인디케이터
    public Image giftTabBar;               // 선물세트 탭 인디케이터*/

    [Header("팝업")]
    public GameObject optionPopup;         // 상품 옵션 팝업
    public GameObject cartListPopup;       // 장바구니 팝업
    public GameObject orderListPopup;      // 주문목록 팝업
    public GameObject receiptPopup;        // 영수증 팝업
    public GameObject paymentGuidePopup;   // 결제 안내 팝업

    [Header("팝업 닫기 버튼")]
    public Button optionPopupCloseBtn;
    public Button cartListPopupCloseBtn;
    public Button orderListPopupCloseBtn;
    public Button receiptPopupCloseBtn;
    public Button paymentGuidePopupCloseBtn;

    [Header("이동 버튼")]
    public Button productNextBtn;          // Product -> Payment 이동 버튼
    public Button goPayBtn;                // Product -> Payment 이동 버튼(대체용)
    public Button viewCartBtn;             // 장바구니 팝업 열기 버튼
    public Button payBackBtn;              // Payment -> Product 뒤로가기
    public Button receiptBtn;              // 영수증 팝업 열기 버튼
    public Button homeBtn;                 // 첫 화면으로 이동 버튼
    public Button completeNextBtn;         // 완료 후 다시 첫 화면 이동 버튼

    [Header("외부 참조")]
    public Script3_PaymentManager paymentManager; // 결제화면 진입 시 정보 갱신용 참조

    [HideInInspector] public string deliveryMethod = "pickup";  // 픽업 또는 배송 저장

    private static readonly Color ActiveColor = new Color(0.18f, 0.42f, 0.31f);
    private static readonly Color InactiveColor = new Color(0.55f, 0.55f, 0.55f);

    private void Awake()
    {
        SetGroup(false,
            introPanel, startPanel, productPanel, paymentPanel, completePanel,
            optionPopup, cartListPopup, orderListPopup, receiptPopup, paymentGuidePopup);
    }

    private void Start()
    {
        BindButtons();
        Show(introPanel);
        SwitchTab(0);
    }

    private void SwitchTab(int v)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 버튼 클릭 이벤트를 한 번에 연결한다.
    /// </summary>
    private void BindButtons()
    {
        AddClick(startIntroBtn, GoToStart);
        AddClick(introNextBtn, GoToStart);
        // 픽업/배송 버튼은 deliveryMethod 값만 변경
        AddClick(pickupBtn, () => deliveryMethod = "pickup");
        AddClick(deliveryBtn, () => deliveryMethod = "delivery");
        // 시작 화면 다음 버튼
        AddClick(startNextBtn, GoToProduct);
        // 카테고리 탭 버튼 연결
        AddClick(tabFruit, () => SwitchTab(0));
        AddClick(tabCup, () => SwitchTab(1));
        AddClick(tabGift, () => SwitchTab(2));
        // 페이지 이동 버튼 연결
        AddClick(productNextBtn, GoToPayment);
        AddClick(goPayBtn, GoToPayment);
        AddClick(payBackBtn, GoToProduct);
        AddClick(viewCartBtn, () => OpenPopup(cartListPopup));
        AddClick(receiptBtn, () => OpenPopup(receiptPopup));
        AddClick(homeBtn, GoToIntro);
        AddClick(completeNextBtn, GoToIntro);
        // 팝업 닫기 버튼 연결
        AddClick(optionPopupCloseBtn, () => ClosePopup(optionPopup));
        AddClick(cartListPopupCloseBtn, () => ClosePopup(cartListPopup));
        AddClick(orderListPopupCloseBtn, () => ClosePopup(orderListPopup));
        AddClick(receiptPopupCloseBtn, () => ClosePopup(receiptPopup));
        AddClick(paymentGuidePopupCloseBtn, () => ClosePopup(paymentGuidePopup));
    }

    /// <summary>
    /// 전달된 패널만 활성화하고 나머지 메인 패널은 비활성화한다.
    /// </summary>
    public void Show(GameObject target)
    {
        SetActive(introPanel, target == introPanel);
        SetActive(startPanel, target == startPanel);
        SetActive(productPanel, target == productPanel);
        SetActive(paymentPanel, target == paymentPanel);
        SetActive(completePanel, target == completePanel);
    }

    public void GoToIntro() => Show(introPanel);
    public void GoToStart() => Show(startPanel);
    public void GoToProduct() => Show(productPanel);

    /// <summary>
    /// 결제 화면으로 이동하면서 결제 정보도 갱신한다.
    /// </summary>
    public void GoToPayment()
    {
        Show(paymentPanel);
        paymentManager?.OnEnterPayment();
    }

    public void GoToComplete() => Show(completePanel);

    /// <summary>
    /// 카테고리 탭을 전환한다.
    /// 0 = 일반과일, 1 = 컵과일, 2 = 선물세트
    /// </summary>
  /*  public void SwitchTab(int index)
    {
        SetActive(gridFruit, index == 0);
        SetActive(gridCup, index == 1);
        SetActive(gridGift, index == 2);
        // 선택된 탭만 인디케이터 표시
        SetIndicator(fruitTabBar, index == 0);
        SetIndicator(cupTabBar, index == 1);
        SetIndicator(giftTabBar, index == 2);
        // 선택된 탭만 강조 색상 적용
        SetTabColor(tabFruit, index == 0);
        SetTabColor(tabCup, index == 1);
        SetTabColor(tabGift, index == 2);
    }*/
    // 공통 팝업 열기/닫기
    public void OpenPopup(GameObject popup) => SetActive(popup, true);
    public void ClosePopup(GameObject popup) => SetActive(popup, false);

    private void AddClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private void SetIndicator(Image image, bool visible)
    {
        if (image != null) image.gameObject.SetActive(visible);
    }

    private void SetTabColor(Button button, bool isActive)
    {
        var label = button ? button.GetComponentInChildren<Text>() : null;
        if (label != null) label.color = isActive ? ActiveColor : InactiveColor;
    }

    private void SetGroup(bool active, params GameObject[] objects)
    {
        foreach (var obj in objects) SetActive(obj, active);
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }
}
