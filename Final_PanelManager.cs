using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  PanelManager_v4.cs
//  역할:
//  - IntroPanel -> StartPanel -> ProductPanel -> PaymentPanel -> CompletePanel 전환
//  - 각 페이지의 다음 이동 메서드 제공
//  - 카테고리 탭 전환
//  - 공통 팝업 열기/닫기
// ================================================================
public class PanelManager_v4 : MonoBehaviour
{
    [Header("메인 패널")]
    public GameObject introPanel;
    public GameObject startPanel;
    public GameObject productPanel;
    public GameObject paymentPanel;
    public GameObject completePanel;

    [Header("IntroPanel")]
    public Button startIntroBtn;
    public Button introNextBtn;

    [Header("StartPanel")]
    public Button pickupBtn;
    public Button deliveryBtn;
    public Button startNextBtn;

    [Header("카테고리 탭")]
    public Button tabFruit;
    public Button tabCup;
    public Button tabGift;

    [Header("카테고리 그리드")]
    public GameObject gridFruit;
    public GameObject gridCup;
    public GameObject gridGift;

    [Header("탭 인디케이터")]
    public Image fruitTabBar;
    public Image cupTabBar;
    public Image giftTabBar;

    [Header("팝업")]
    public GameObject optionPopup;
    public GameObject cartListPopup;
    public GameObject orderListPopup;
    public GameObject receiptPopup;
    public GameObject paymentGuidePopup;

    [Header("팝업 닫기 버튼")]
    public Button optionPopupCloseBtn;
    public Button cartListPopupCloseBtn;
    public Button orderListPopupCloseBtn;
    public Button receiptPopupCloseBtn;
    public Button paymentGuidePopupCloseBtn;

    [Header("페이지 이동 버튼")]
    public Button productNextBtn;
    public Button goPayBtn;
    public Button viewCartBtn;
    public Button payBackBtn;
    public Button receiptBtn;
    public Button homeBtn;
    public Button completeNextBtn;

    [Header("외부 참조")]
    public PaymentManager_v4 paymentManager;

    [HideInInspector] public string deliveryMethod = "pickup";

    private static readonly Color COL_ACTIVE = new Color(0.18f, 0.42f, 0.31f);
    private static readonly Color COL_INACTIVE = new Color(0.55f, 0.55f, 0.55f);

    void Awake()
    {
        SetActive(introPanel, false);
        SetActive(startPanel, false);
        SetActive(productPanel, false);
        SetActive(paymentPanel, false);
        SetActive(completePanel, false);

        SetActive(optionPopup, false);
        SetActive(cartListPopup, false);
        SetActive(orderListPopup, false);
        SetActive(receiptPopup, false);
        SetActive(paymentGuidePopup, false);
    }

    void Start()
    {
        if (startIntroBtn) startIntroBtn.onClick.AddListener(GoToStart);
        if (introNextBtn) introNextBtn.onClick.AddListener(GoToStart);

        if (pickupBtn) pickupBtn.onClick.AddListener(() => deliveryMethod = "pickup");
        if (deliveryBtn) deliveryBtn.onClick.AddListener(() => deliveryMethod = "delivery");
        if (startNextBtn) startNextBtn.onClick.AddListener(GoToProduct);

        if (tabFruit) tabFruit.onClick.AddListener(() => SwitchTab(0));
        if (tabCup) tabCup.onClick.AddListener(() => SwitchTab(1));
        if (tabGift) tabGift.onClick.AddListener(() => SwitchTab(2));

        if (productNextBtn) productNextBtn.onClick.AddListener(GoToPayment);
        if (goPayBtn) goPayBtn.onClick.AddListener(GoToPayment);
        if (payBackBtn) payBackBtn.onClick.AddListener(GoToProduct);
        if (viewCartBtn) viewCartBtn.onClick.AddListener(() => OpenPopup(cartListPopup));
        if (receiptBtn) receiptBtn.onClick.AddListener(() => OpenPopup(receiptPopup));
        if (homeBtn) homeBtn.onClick.AddListener(GoToIntro);
        if (completeNextBtn) completeNextBtn.onClick.AddListener(GoToIntro);

        if (optionPopupCloseBtn) optionPopupCloseBtn.onClick.AddListener(() => ClosePopup(optionPopup));
        if (cartListPopupCloseBtn) cartListPopupCloseBtn.onClick.AddListener(() => ClosePopup(cartListPopup));
        if (orderListPopupCloseBtn) orderListPopupCloseBtn.onClick.AddListener(() => ClosePopup(orderListPopup));
        if (receiptPopupCloseBtn) receiptPopupCloseBtn.onClick.AddListener(() => ClosePopup(receiptPopup));
        if (paymentGuidePopupCloseBtn) paymentGuidePopupCloseBtn.onClick.AddListener(() => ClosePopup(paymentGuidePopup));

        Show(introPanel);
        SwitchTab(0);
    }

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

    public void GoToPayment()
    {
        Show(paymentPanel);
        if (paymentManager != null)
            paymentManager.OnEnterPayment();
    }

    public void GoToComplete() => Show(completePanel);

    public void SwitchTab(int idx)
    {
        SetActive(gridFruit, idx == 0);
        SetActive(gridCup, idx == 1);
        SetActive(gridGift, idx == 2);

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

    private void SetActive(GameObject obj, bool active)
    {
        if (obj) obj.SetActive(active);
    }

    private void SetTabColor(Button btn, bool active)
    {
        var txt = btn ? btn.GetComponentInChildren<Text>() : null;
        if (txt) txt.color = active ? COL_ACTIVE : COL_INACTIVE;
    }
}
