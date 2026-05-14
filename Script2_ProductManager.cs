using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  Script 2 : ProductManager.cs
//  [ 상품 추가 + 옵션 설정 팝업창 ]
//
//  담당 역할:
//  - 상품 카드 버튼 클릭 → 옵션 팝업 열기
//  - 옵션 팝업에서 수량 조절 + 담기 버튼
//  - 장바구니 목록 RightCartPanel에 표시
//  - 구매목록 팝업(cartListPopup) 내용 갱신
//
//  상품 카드 버튼 연결 방법:
//  Button → OnClick() → ProductManager.OpenOptionPopup("사과", 8000)
// ================================================================
public class ProductManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //  PanelManager 참조
    //  팝업 열기/닫기, 패널 전환을 PanelManager에 위임
    //  Inspector: KioskController 오브젝트의 PanelManager 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== PanelManager 참조 =====")]
    public PanelManager panelManager;

    // ──────────────────────────────────────────────────────────
    //  옵션 팝업창 내부 UI
    //  Inspector: OptionPopup 안의 각 UI 요소 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== 옵션 팝업 내부 UI =====")]
    public Text    optionProductNameText;  // 상품명 표시 텍스트
    public Text    optionUnitPriceText;    // 단가 표시 텍스트 "8,000원"
    public Text    optionTotalPriceText;   // 합계 표시 텍스트 "합계: 16,000원"

    // 수량 조절 버튼
    public Button  minusBtn;               // - 버튼 (수량 감소)
    public Text    quantityText;           // 수량 숫자 표시
    public Button  plusBtn;                // + 버튼 (수량 증가)

    // 담기 버튼
    public Button  addToCartBtn;           // "장바구니 담기" 버튼

    // ──────────────────────────────────────────────────────────
    //  RightCartPanel UI
    //  Inspector: ProductPanel > RightCartPanel 안의 UI 연결
    // ──────────────────────────────────────────────────────────
    [Header("===== RightCartPanel UI =====")]
    public Text    cartCountText;          // "3개 담김"
    public Text    cartTotalText;          // "합계: 12,000원"
    public Button  goPayBtn;              // "결제하기" 버튼 (장바구니 있을 때만 활성)

    // ──────────────────────────────────────────────────────────
    //  구매목록 팝업 내부 UI
    //  Inspector: CartListPopup 안의 UI 연결
    //  CartListPopup 구조:
    //  CartListPopup
    //  ├ TitleText      "구매 목록"
    //  ├ ScrollView
    //  │  └ Content     ← cartListContent 연결
    //  ├ TotalText      ← cartListTotalText 연결
    //  └ CloseButton
    // ──────────────────────────────────────────────────────────
    [Header("===== 구매목록 팝업 내부 UI =====")]
    public Transform cartListContent;      // ScrollView > Content (항목 동적 생성 위치)
    public Text      cartListTotalText;    // 팝업 안 합계 텍스트
    public GameObject cartListRowPrefab;   // 목록 한 줄 프리팹 (Text 2개: 상품명, 가격)

    // ──────────────────────────────────────────────────────────
    //  내부 데이터
    // ──────────────────────────────────────────────────────────

    // 장바구니 리스트: (상품명, 단가) 튜플 리스트
    [HideInInspector]
    public readonly List<(string name, int price)> cart = new();

    // 현재 옵션 팝업에 표시 중인 상품 정보
    private string _currentName  = "";
    private int    _currentPrice = 0;
    private int    _quantity     = 1;       // 현재 선택 수량

    // 생성된 목록 행 오브젝트 추적 (갱신 시 삭제용)
    private readonly List<GameObject> _cartRowObjs = new();

    // =========================================================
    //  Start: 버튼 이벤트 연결
    // =========================================================
    void Start()
    {
        // ── 수량 버튼 ──────────────────────────────────────────
        // - 버튼: 수량 1 감소 (최소 1)
        minusBtn.onClick.AddListener(() =>
        {
            _quantity = Mathf.Max(1, _quantity - 1);
            RefreshOptionPrice();
        });

        // + 버튼: 수량 1 증가
        plusBtn.onClick.AddListener(() =>
        {
            _quantity++;
            RefreshOptionPrice();
        });

        // ── 담기 버튼 ─────────────────────────────────────────
        // 클릭 시 현재 상품을 수량만큼 장바구니에 추가
        addToCartBtn.onClick.AddListener(AddToCart);

        // ── 결제하기 버튼 초기 비활성 ────────────────────────
        // 장바구니가 비어 있으면 결제 불가
        goPayBtn.interactable = false;
    }

    // =========================================================
    //  옵션 팝업 열기
    //  상품 카드 버튼의 OnClick에서 호출
    //  예) ProductManager.OpenOptionPopup("사과", 8000)
    // =========================================================
    public void OpenOptionPopup(string productName, int unitPrice)
    {
        // 현재 상품 정보 저장
        _currentName  = productName;
        _currentPrice = unitPrice;
        _quantity     = 1; // 수량 초기화

        // 팝업 UI 갱신
        optionProductNameText.text = productName;
        optionUnitPriceText.text   = $"{unitPrice:N0}원";
        RefreshOptionPrice();

        // 팝업 열기 (PanelManager에 위임)
        panelManager.OpenPopup(panelManager.optionPopup);
    }

    // =========================================================
    //  옵션 팝업 가격 갱신
    //  수량 변경 시마다 호출
    // =========================================================
    private void RefreshOptionPrice()
    {
        // 수량 텍스트 갱신
        quantityText.text = _quantity.ToString();

        // 합계 = 단가 × 수량
        int total = _currentPrice * _quantity;
        optionTotalPriceText.text = $"합계: {total:N0}원";
    }

    // =========================================================
    //  장바구니 담기
    //  "담기" 버튼 클릭 시 실행
    // =========================================================
    private void AddToCart()
    {
        // 수량만큼 반복 추가 (동일 상품을 수량만큼)
        for (int i = 0; i < _quantity; i++)
            cart.Add((_currentName, _currentPrice));

        // RightCartPanel 텍스트 갱신
        RefreshCartPanel();

        // 구매목록 팝업 내용도 갱신
        RefreshCartListPopup();

        // 옵션 팝업 닫기
        panelManager.ClosePopup(panelManager.optionPopup);

        Debug.Log($"[담기] {_currentName} x{_quantity} = {_currentPrice * _quantity:N0}원");
    }

    // =========================================================
    //  RightCartPanel 텍스트 갱신
    //  상품 담을 때마다 카운트/합계 업데이트
    // =========================================================
    public void RefreshCartPanel()
    {
        int total = 0;
        foreach (var item in cart) total += item.price;

        cartCountText.text = $"{cart.Count}개 담김";
        cartTotalText.text = $"합계: {total:N0}원";

        // 장바구니에 상품 있을 때만 결제 버튼 활성
        goPayBtn.interactable = cart.Count > 0;
    }

    // =========================================================
    //  구매목록 팝업 내용 갱신
    //  팝업 열릴 때 또는 상품 추가/삭제 시 호출
    // =========================================================
    public void RefreshCartListPopup()
    {
        // 기존 행 삭제
        foreach (var row in _cartRowObjs) Destroy(row);
        _cartRowObjs.Clear();

        // 상품이 없으면 빈 상태로 종료
        if (cartListContent == null || cartListRowPrefab == null) return;

        int total = 0;

        foreach (var item in cart)
        {
            // 프리팹으로 한 줄 생성
            var row  = Instantiate(cartListRowPrefab, cartListContent);
            var txts = row.GetComponentsInChildren<Text>();

            // 텍스트 인덱스 약속:
            // 0 = 상품명, 1 = 가격
            if (txts.Length > 0) txts[0].text = item.name;
            if (txts.Length > 1) txts[1].text = $"{item.price:N0}원";

            _cartRowObjs.Add(row);
            total += item.price;
        }

        // 팝업 합계 텍스트 갱신
        if (cartListTotalText)
            cartListTotalText.text = $"합계: {total:N0}원";
    }

    // =========================================================
    //  장바구니 초기화 (GoHome 시 PaymentManager에서 호출)
    // =========================================================
    public void ClearCart()
    {
        cart.Clear();
        RefreshCartPanel();
        RefreshCartListPopup();
    }
}
