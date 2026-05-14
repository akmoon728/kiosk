using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  Script 1 : PanelManager.cs
//  [ 패널 전환 + 카테고리 탭 전환 + 팝업창 전환 ]
//
//  담당 역할:
//  - 메인 패널 4개 Show/Hide 전환
//  - ProductPanel 안의 카테고리 탭 전환 (일반/컵/선물세트)
//  - 팝업창(옵션 팝업, 구매목록 팝업, 영수증 팝업) 열기/닫기
//
//  다른 스크립트 참조:
//  - ProductManager.cs  : 상품 추가/옵션 기능
//  - PaymentManager.cs  : 결제 기능
// ================================================================
public class PanelManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //  메인 패널 4개
    //  Inspector에서 Canvas 자식 패널 4개를 각각 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== 메인 패널 =====")]
    public GameObject startPanel;       // 시작화면 (픽업/배송 선택)
    public GameObject productPanel;     // 상품 선택 화면
    public GameObject paymentPanel;     // 결제 화면
    public GameObject completePanel;    // 주문 완료 화면

    // ──────────────────────────────────────────────────────────
    //  StartPanel 버튼
    //  Inspector: StartPanel 안의 버튼 2개 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== StartPanel 버튼 =====")]
    public Button pickupBtn;            // 픽업 선택 → ProductPanel 이동
    public Button deliveryBtn;          // 배송 선택 → ProductPanel 이동

    // ──────────────────────────────────────────────────────────
    //  카테고리 탭 버튼
    //  Inspector: ProductPanel > LeftCategoryMenu 안의 버튼 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== 카테고리 탭 버튼 =====")]
    public Button tabFruit;             // 일반과일 탭 버튼
    public Button tabCup;               // 컵과일 탭 버튼
    public Button tabGift;              // 선물세트 탭 버튼

    // ──────────────────────────────────────────────────────────
    //  카테고리별 상품 그리드
    //  Inspector: ProductPanel > MainContent 안의 Grid 3개 연결
    //  각 Grid는 해당 카테고리 상품 카드 버튼들을 담고 있음
    // ──────────────────────────────────────────────────────────
    [Header("===== 카테고리 그리드 =====")]
    public GameObject gridFruit;        // 일반과일 상품 카드 그룹
    public GameObject gridCup;          // 컵과일 상품 카드 그룹
    public GameObject gridGift;         // 선물세트 상품 카드 그룹

    // ──────────────────────────────────────────────────────────
    //  탭 선택 인디케이터 (선택된 탭 강조 표시)
    //  Inspector: 각 탭 버튼 아래에 있는 초록 바 Image 연결
    //  없으면 비워둬도 됨
    // ──────────────────────────────────────────────────────────
    [Header("===== 탭 인디케이터 (선택사항) =====")]
    public Image fruitTabBar;           // 일반과일 탭 선택 표시 바
    public Image cupTabBar;             // 컵과일 탭 선택 표시 바
    public Image giftTabBar;            // 선물세트 탭 선택 표시 바

    // ──────────────────────────────────────────────────────────
    //  팝업창 3개
    //  Inspector: Canvas 맨 아래에 배치한 팝업 패널 연결
    //  모두 기본 SetActive(false) 상태로 시작
    // ──────────────────────────────────────────────────────────
    [Header("===== 팝업창 =====")]
    public GameObject optionPopup;      // 상품 클릭 시 옵션 선택 팝업
    public GameObject cartListPopup;    // "구매목록 보기" 팝업
    public GameObject receiptPopup;     // CompletePanel 영수증 팝업

    // ──────────────────────────────────────────────────────────
    //  팝업 닫기 버튼
    //  Inspector: 각 팝업 안의 닫기(X) 버튼 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== 팝업 닫기 버튼 =====")]
    public Button optionPopupCloseBtn;  // 옵션 팝업 닫기
    public Button cartListPopupCloseBtn;// 구매목록 팝업 닫기
    public Button receiptPopupCloseBtn; // 영수증 팝업 닫기

    // ──────────────────────────────────────────────────────────
    //  RightCartPanel 버튼
    //  Inspector: ProductPanel > RightCartPanel 안의 버튼 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== 장바구니 버튼 =====")]
    public Button goPayBtn;             // "결제하기" → PaymentPanel 이동
    public Button viewCartBtn;          // "구매목록 보기" → cartListPopup 열기

    // ──────────────────────────────────────────────────────────
    //  CompletePanel 버튼
    // ──────────────────────────────────────────────────────────
    [Header("===== CompletePanel 버튼 =====")]
    public Button receiptBtn;           // "영수증 보기" → receiptPopup 열기
    public Button homeBtn;              // "홈으로" → StartPanel 이동

    // ──────────────────────────────────────────────────────────
    //  PaymentPanel 뒤로가기
    // ──────────────────────────────────────────────────────────
    [Header("===== PaymentPanel 버튼 =====")]
    public Button payBackBtn;           // "뒤로" → ProductPanel 이동

    // ── 색상 상수 ─────────────────────────────────────────────
    private static readonly Color COL_ACTIVE   = new Color(0.18f, 0.42f, 0.31f); // 초록
    private static readonly Color COL_INACTIVE = new Color(0.55f, 0.55f, 0.55f); // 회색

    // 배송 방법 저장 (ProductManager, PaymentManager에서 접근 가능)
    [HideInInspector] public string deliveryMethod = "pickup";

    // =========================================================
    //  Awake: 모든 패널/팝업 먼저 비활성화
    //  Start보다 먼저 실행되어 깜빡임 방지
    // =========================================================
    void Awake()
    {
        // 메인 패널 전부 끄기
        startPanel.SetActive(false);
        productPanel.SetActive(false);
        paymentPanel.SetActive(false);
        completePanel.SetActive(false);

        // 팝업 전부 끄기
        if (optionPopup)    optionPopup.SetActive(false);
        if (cartListPopup)  cartListPopup.SetActive(false);
        if (receiptPopup)   receiptPopup.SetActive(false);
    }

    // =========================================================
    //  Start: 버튼 이벤트 연결 및 초기 화면 설정
    // =========================================================
    void Start()
    {
        // ── StartPanel 버튼 연결 ──────────────────────────────
        // 픽업 선택 시 deliveryMethod 저장 후 ProductPanel 이동
        pickupBtn.onClick.AddListener(() =>
        {
            deliveryMethod = "pickup";
            Show(productPanel);
        });

        // 배송 선택 시 deliveryMethod 저장 후 ProductPanel 이동
        deliveryBtn.onClick.AddListener(() =>
        {
            deliveryMethod = "delivery";
            Show(productPanel);
        });

        // ── 카테고리 탭 버튼 연결 ────────────────────────────
        tabFruit.onClick.AddListener(() => SwitchTab(0)); // 일반과일
        tabCup.onClick.AddListener(  () => SwitchTab(1)); // 컵과일
        tabGift.onClick.AddListener( () => SwitchTab(2)); // 선물세트

        // ── 장바구니 버튼 연결 ───────────────────────────────
        // 결제하기 버튼: ProductPanel → PaymentPanel
        goPayBtn.onClick.AddListener(() => Show(paymentPanel));

        // 구매목록 보기: cartListPopup 열기
        if (viewCartBtn)
            viewCartBtn.onClick.AddListener(() => OpenPopup(cartListPopup));

        // ── PaymentPanel 뒤로 버튼 ───────────────────────────
        payBackBtn.onClick.AddListener(() => Show(productPanel));

        // ── CompletePanel 버튼 연결 ──────────────────────────
        // 영수증 보기: receiptPopup 열기
        receiptBtn.onClick.AddListener(() => OpenPopup(receiptPopup));

        // 홈으로: StartPanel 이동
        homeBtn.onClick.AddListener(() => Show(startPanel));

        // ── 팝업 닫기 버튼 연결 ──────────────────────────────
        if (optionPopupCloseBtn)
            optionPopupCloseBtn.onClick.AddListener(() => ClosePopup(optionPopup));
        if (cartListPopupCloseBtn)
            cartListPopupCloseBtn.onClick.AddListener(() => ClosePopup(cartListPopup));
        if (receiptPopupCloseBtn)
            receiptPopupCloseBtn.onClick.AddListener(() => ClosePopup(receiptPopup));

        // ── 초기 화면: StartPanel 표시 ───────────────────────
        Show(startPanel);
        SwitchTab(0); // 첫 탭은 일반과일
    }

    // =========================================================
    //  메인 패널 전환
    //  target 패널만 켜고 나머지는 전부 끔
    // =========================================================
    public void Show(GameObject target)
    {
        startPanel.SetActive(   target == startPanel);
        productPanel.SetActive( target == productPanel);
        paymentPanel.SetActive( target == paymentPanel);
        completePanel.SetActive(target == completePanel);
    }

    // =========================================================
    //  카테고리 탭 전환
    //  idx: 0=일반과일, 1=컵과일, 2=선물세트
    // =========================================================
    public void SwitchTab(int idx)
    {
        // 해당 탭의 그리드만 활성화
        gridFruit.SetActive(idx == 0);
        gridCup.SetActive(  idx == 1);
        gridGift.SetActive( idx == 2);

        // 인디케이터 바 전환 (연결했을 때만)
        if (fruitTabBar) fruitTabBar.gameObject.SetActive(idx == 0);
        if (cupTabBar)   cupTabBar.gameObject.SetActive(  idx == 1);
        if (giftTabBar)  giftTabBar.gameObject.SetActive( idx == 2);

        // 탭 버튼 텍스트 컬러 변경
        SetTabColor(tabFruit, idx == 0);
        SetTabColor(tabCup,   idx == 1);
        SetTabColor(tabGift,  idx == 2);
    }

    // =========================================================
    //  팝업 열기
    //  팝업 오브젝트를 활성화
    // =========================================================
    public void OpenPopup(GameObject popup)
    {
        if (popup != null) popup.SetActive(true);
    }

    // =========================================================
    //  팝업 닫기
    //  팝업 오브젝트를 비활성화
    // =========================================================
    public void ClosePopup(GameObject popup)
    {
        if (popup != null) popup.SetActive(false);
    }

    // =========================================================
    //  탭 버튼 텍스트 컬러 변경 헬퍼
    //  활성 탭은 초록, 비활성 탭은 회색
    // =========================================================
    private void SetTabColor(Button btn, bool isActive)
    {
        var label = btn?.GetComponentInChildren<Text>();
        if (label) label.color = isActive ? COL_ACTIVE : COL_INACTIVE;
    }
}
