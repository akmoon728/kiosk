using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  PanelManager_v3.cs
//  역할:
//  - IntroPanel(시작하기) -> StartPanel -> ProductPanel -> PaymentPanel -> CompletePanel 전환
//  - 카테고리 탭 전환
//  - 옵션/구매목록/결제목록/영수증 팝업 열기/닫기
// ================================================================
public class PanelManager_v3 : MonoBehaviour
{
    [Header("메인 패널")]
    public GameObject introPanel;
    public GameObject startPanel;
    public GameObject productPanel;
    public GameObject paymentPanel;
    public GameObject completePanel;

    [Header("IntroPanel")]
    public Button startIntroBtn;   // 시작하기 버튼

    [Header("StartPanel")]
    public Button pickupBtn;       // 픽업 버튼
    public Button deliveryBtn;     // 배송 버튼

    [Header("카테고리 탭")]
    public Button tabFruit;        // 일반과일 탭
    public Button tabCup;          // 컵과일 탭
    public Button tabGift;         // 선물세트 탭

    [Header("카테고리 그리드")]
    public GameObject gridFruit;   // 일반과일 상품 리스트
    public GameObject gridCup;     // 컵과일 상품 리스트
    public GameObject gridGift;    // 선물세트 상품 리스트

    [Header("탭 인디케이터")]
    public Image fruitTabBar;
    public Image cupTabBar;
    public Image giftTabBar;

    [Header("팝업")]
    public GameObject optionPopup;     // 상품 옵션 선택 팝업
    public GameObject cartListPopup;   // 장바구니 목록 팝업
    public GameObject orderListPopup;  // 결제 전 주문 확인 팝업
    public GameObject receiptPopup;    // 영수증 팝업

    [Header("팝업 닫기 버튼")]
    public Button optionPopupCloseBtn;
    public Button cartListPopupCloseBtn;
    public Button orderListPopupCloseBtn;
    public Button receiptPopupCloseBtn;

    [Header("이동 버튼")]
    public Button goPayBtn;        // 결제하기 버튼
    public Button viewCartBtn;     // 구매목록 보기 버튼
    public Button payBackBtn;      // 결제창 뒤로가기 버튼
    public Button receiptBtn;      // 영수증 보기 버튼
    public Button homeBtn;         // 홈으로 버튼

    [Header("외부 참조")]
    public PaymentManager_v3 paymentManager;

    [HideInInspector] public string deliveryMethod = "pickup";

    private static readonly Color COL_ACTIVE = new Color(0.18f, 0.42f, 0.31f);
    private static readonly Color COL_INACTIVE = new Color(0.55f, 0.55f, 0.55f);

    void Awake()
    {
        SetPanelActive(introPanel, false);
        SetPanelActive(startPanel, false);
        SetPanelActive(productPanel, false);
        SetPanelActive(paymentPanel, false);
        SetPanelActive(completePanel, false);

        SetPanelActive(optionPopup, false);
        SetPanelActive(cartListPopup, false);
        SetPanelActive(orderListPopup, false);
        SetPanelActive(receiptPopup, false);
    }

    void Start()
    {
        if (startIntroBtn) startIntroBtn.onClick.AddListener(GoToStart);

        if (pickupBtn) pickupBtn.onClick.AddListener(() =>
        {
            deliveryMethod = "pickup";
            GoToProduct();
        });

        if (deliveryBtn) deliveryBtn.onClick.AddListener(() =>
        {
            deliveryMethod = "delivery";
            GoToProduct();
        });

        if (tabFruit) tabFruit.onClick.AddListener(() => SwitchTab(0));
        if (tabCup) tabCup.onClick.AddListener(() => SwitchTab(1));
        if (tabGift) tabGift.onClick.AddListener(() => SwitchTab(2));

        if (goPayBtn) goPayBtn.onClick.AddListener(GoToPayment);
        if (payBackBtn) payBackBtn.onClick.AddListener(GoToProduct);
        if (viewCartBtn) viewCartBtn.onClick.AddListener(() => OpenPopup(cartListPopup));
        if (receiptBtn) receiptBtn.onClick.AddListener(() => OpenPopup(receiptPopup));
        if (homeBtn) homeBtn.onClick.AddListener(GoToIntro);

        if (optionPopupCloseBtn) optionPopupCloseBtn.onClick.AddListener(() => ClosePopup(optionPopup));
        if (cartListPopupCloseBtn) cartListPopupCloseBtn.onClick.AddListener(() => ClosePopup(cartListPopup));
        if (orderListPopupCloseBtn) orderListPopupCloseBtn.onClick.AddListener(() => ClosePopup(orderListPopup));
        if (receiptPopupCloseBtn) receiptPopupCloseBtn.onClick.AddListener(() => ClosePopup(receiptPopup));

        Show(introPanel);
        SwitchTab(0);
    }

    public void Show(GameObject target)
    {
        SetPanelActive(introPanel, target == introPanel);
        SetPanelActive(startPanel, target == startPanel);
        SetPanelActive(productPanel, target == productPanel);
        SetPanelActive(paymentPanel, target == paymentPanel);
        SetPanelActive(completePanel, target == completePanel);
    }

    public void GoToIntro()   => Show(introPanel);
    public void GoToStart()   => Show(startPanel);
    public void GoToProduct() => Show(productPanel);

    public void GoToPayment()
    {
        Show(paymentPanel);
        if (paymentManager != null)
            paymentManager.OnEnterPayment();
    }

    public void GoToComplete() => Show(completePanel);

    public void SwitchTab(int idx)
    {
        SetPanelActive(gridFruit, idx == 0);
        SetPanelActive(gridCup, idx == 1);
        SetPanelActive(gridGift, idx == 2);

        if (fruitTabBar) fruitTabBar.gameObject.SetActive(idx == 0);
        if (cupTabBar) cupTabBar.gameObject.SetActive(idx == 1);
        if (giftTabBar) giftTabBar.gameObject.SetActive(idx == 2);

        SetTabColor(tabFruit, idx == 0);
        SetTabColor(tabCup, idx == 1);
        SetTabColor(tabGift, idx == 2);
    }

    public void OpenPopup(GameObject popup)
    {
        if (popup) popup.SetActive(true);
    }

    public void ClosePopup(GameObject popup)
    {
        if (popup) popup.SetActive(false);
    }

    private void SetPanelActive(GameObject obj, bool active)
    {
        if (obj) obj.SetActive(active);
    }

    private void SetTabColor(Button btn, bool active)
    {
        var txt = btn ? btn.GetComponentInChildren<Text>() : null;
        if (txt) txt.color = active ? COL_ACTIVE : COL_INACTIVE;
    }
}
