using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// <summary>
// Script2_ProductManager.cs 스타일 정리 버전
// - 일반과일 / 컵과일 / 선물세트 옵션 팝업 관리
// - 크기 / 당도 / 구성 / 포장 / 수량 선택 처리
// - 장바구니 추가 및 장바구니 목록 갱신
//
// 버튼 연결 예시
// - 사과 이미지 버튼     -> OpenFruitOption("사과", 5000)
// - 컵과일 M 버튼       -> OpenCupOption("혼합컵 M", 6500)
// - 선물세트 대 버튼    -> OpenGiftOption("프리미엄 세트", 30000)
// ================================================================
/// </summary>
public class Script2_ProductManager : MonoBehaviour
{
    public enum ProductCategory
    {
        Fruit,
        Cup,
        Gift
    }

    [System.Serializable]
    public class CartItem
    {
        public string name;             // 상품명
        public ProductCategory category;// 상품 타입
        public string size;             // 크기
        public string sweetness;        // 당도
        public string composition;      // 컵과일 구성
        public string packageType;      // 선물세트 포장
        public int quantity;            // 수량
        public int unitPrice;           // 단가
        public int totalPrice;          // 총 금액

        /// <summary>
        /// 선택된 옵션을 문자열로 반환한다.
        /// </summary>
        public string OptionSummary()
        {
            var options = new List<string>();
            if (!string.IsNullOrEmpty(size)) options.Add($"크기:{size}");
            if (!string.IsNullOrEmpty(sweetness)) options.Add($"당도:{sweetness}");
            if (!string.IsNullOrEmpty(composition)) options.Add($"구성:{composition}");
            if (!string.IsNullOrEmpty(packageType)) options.Add($"포장:{packageType}");
            return string.Join(" / ", options);
        }
    }

    [Header("외부 참조")]
    public Script1_PanelManager panelManager;  //팝업 제어용 panelmanager 참조

    [Header("옵션 팝업 텍스트")]
    public Text optionTitleText;       // 팝업 상단 상품명
    public Text optionCategoryText;    // 카테고리명
    public Text optionUnitPriceText;   // 계산된 단가
    public Text optionSelectedText;    // 선택한 옵션 요약
    public Text optionTotalPriceText;  // 최종 결제 금액

    [Header("수량")]
    public Button minusBtn;            // 수량 감소 버튼
    public Text quantityText;          // 현재 수량 표시
    public Button plusBtn;             // 수량 증가 버튼
    public Button addToCartBtn;        // 장바구니 담기 버튼

    [Header("크기 버튼")]
    public Button sizeSmallBtn;
    public Button sizeMediumBtn;
    public Button sizeLargeBtn;

    [Header("당도 버튼")]
    public Button sweetLowBtn;
    public Button sweetMediumBtn;
    public Button sweetHighBtn;

    [Header("컵과일 구성 버튼")]
    public Button cupSingleBtn;
    public Button cupMixBtn;
    public Button cupPremiumBtn;

    [Header("포장 버튼")]
    public Button packBasicBtn;
    public Button packRibbonBtn;
    public Button packLuxuryBtn;

    [Header("옵션 그룹")]
    public GameObject sizeGroup;       // 크기 옵션 그룹
    public GameObject sweetGroup;      // 당도 옵션 그룹
    public GameObject cutGroup;        // 커팅 옵션 그룹
    public GameObject cupGroup;        // 컵과일 구성 그룹
    public GameObject packGroup;       // 포장 옵션 그룹

    [Header("장바구니 요약")]
    public Text cartCountText;         // 장바구니 총 수량
    public Text cartTotalText;         // 장바구니 총 금액
    public Button goPayBtn;            // 결제하기 버튼

    [Header("장바구니 팝업")]
    public Transform cartListContent;  // 장바구니 항목이 생성될 부모
    public Text cartListTotalText;     // 장바구니 총합 텍스트
    public GameObject cartListRowPrefab;// 장바구니 한 줄 프리팹

    [HideInInspector] public readonly List<CartItem> cart = new();

    private readonly List<GameObject> cartRows = new();

    private ProductCategory currentCategory;
    private string currentName;
    private int basePrice;
    private int quantity = 1;

    private string size = "중";
    private string sweetness = "보통";
    private string composition = "혼합";
    private string packageType = "기본";

    // ── 필드 선언부 (클래스 상단에 위치) ────────────────────────

    [Header("일반과일 버튼")]
    public Button[] fruitButtons;

    [Header("컵과일 버튼")]
    public Button[] cupButtons;

    [Header("선물세트 버튼")]
    public Button[] giftButtons;

    private readonly string[] fruitNames = { "사과", "딸기", "포도", "수박", "복숭아", "오렌지" };
    private readonly int[] fruitPrices = { 5000, 6000, 7000, 12000, 8000, 6500 };

    private readonly string[] cupNames = { "딸기컵", "과일컵", "멜론컵", "수박컵", "망고컵", "믹스컵" };
    private readonly int[] cupPrices = { 4500, 5000, 8000, 7000, 9000, 5500 };

    private readonly string[] giftNames = { "사과세트", "배세트", "감귤세트", "포도세트", "혼합세트A", "혼합세트B" };
    private readonly int[] giftPrices = { 25000, 28000, 22000, 30000, 35000, 40000 };


    // ── BindButtons() : 3개를 1개로 합친 버전 ───────────────────

    /// <summary>모든 버튼 이벤트를 한 곳에서 연결한다.</summary>
    private void BindButtons()
    {
        // 수량 / 장바구니
        AddClick(minusBtn, () => ChangeQuantity(-1));
        AddClick(plusBtn, () => ChangeQuantity(1));
        AddClick(addToCartBtn, AddCurrentItemToCart);

        // 크기
        AddClick(sizeSmallBtn, () => SetSize("소"));
        AddClick(sizeMediumBtn, () => SetSize("중"));
        AddClick(sizeLargeBtn, () => SetSize("대"));

        // 당도 (일반과일 전용)
        AddClick(sweetLowBtn, () => SetSweetness("낮음"));
        AddClick(sweetMediumBtn, () => SetSweetness("보통"));
        AddClick(sweetHighBtn, () => SetSweetness("높음"));

        // 컵과일 구성
        AddClick(cupSingleBtn, () => SetComposition("단일"));
        AddClick(cupMixBtn, () => SetComposition("혼합"));
        AddClick(cupPremiumBtn, () => SetComposition("프리미엄"));

        // 선물세트 포장
        AddClick(packBasicBtn, () => SetPackage("기본"));
        AddClick(packRibbonBtn, () => SetPackage("리본"));
        AddClick(packLuxuryBtn, () => SetPackage("고급"));

        // 일반과일 버튼 6개
        for (int i = 0; i < fruitButtons.Length; i++)
        {
            int idx = i; // 클로저 캡처 방지 - 삭제 금지
            AddClick(fruitButtons[idx], () => OpenFruitOption(fruitNames[idx], fruitPrices[idx]));
        }

        // 컵과일 버튼 6개
        for (int i = 0; i < cupButtons.Length; i++)
        {
            int idx = i;
            AddClick(cupButtons[idx], () => OpenCupOption(cupNames[idx], cupPrices[idx]));
        }

        // 선물세트 버튼 6개
        for (int i = 0; i < giftButtons.Length; i++)
        {
            int idx = i;
            AddClick(giftButtons[idx], () => OpenGiftOption(giftNames[idx], giftPrices[idx]));
        }
    }


    // ── 옵션 팝업 호출 메서드 ────────────────────────────────────

    /// <summary>일반과일 선택 시 호출</summary>
    public void OpenFruitOption(string name, int price)
    {
        SetupOption(ProductCategory.Fruit, name, price, true, true, false, false);
    }

    /// <summary>컵과일 선택 시 호출</summary>
    public void OpenCupOption(string name, int price)
    {
        SetupOption(ProductCategory.Cup, name, price, true, false, true, false);
    }

    /// <summary>선물세트 선택 시 호출</summary>
    public void OpenGiftOption(string name, int price)
    {
        SetupOption(ProductCategory.Gift, name, price, true, false, false, true);
    }

    /// <summary>
    /// 선택한 상품에 맞게 옵션 팝업을 초기화한다.
    /// </summary>
    // 옵션 팝업 공통 세팅
    private void SetupOption(ProductCategory category, string name, int price, bool showSize, bool showSweet, bool showCup, bool showPack)
    {
        currentCategory = category;
        currentName = name;
        basePrice = price;
        quantity = 1;
        // 카테고리별 기본 옵션 초기화
        size = "중";
        sweetness = category == ProductCategory.Fruit ? "보통" : "";
        composition = category == ProductCategory.Cup ? "혼합" : "";
        packageType = category == ProductCategory.Gift ? "기본" : "";

        // 카테고리에 맞는 옵션 그룹만 화면에 표시
        SetActive(sizeGroup, showSize);
        SetActive(sweetGroup, showSweet);
        SetActive(cupGroup, showCup);
        SetActive(packGroup, showPack);

        RefreshOptionUI();
        //panelManager.OpenPopup(panelManager.optionPopup);
    }

    // 수량 변경, 최소 1개 유지
    private void ChangeQuantity(int delta)
    {
        quantity = Mathf.Max(1, quantity + delta);
        RefreshOptionUI();
    }

    private void SetSize(string value) { size = value; RefreshOptionUI(); } //size 변수값 바꿈, 현재 옵션 요약 다시 그림, 가격 다시 계산, 화면 텍스트 다시 갱신
    private void SetSweetness(string value) { sweetness = value; RefreshOptionUI(); }
    private void SetComposition(string value) { composition = value; RefreshOptionUI(); }
    private void SetPackage(string value) { packageType = value; RefreshOptionUI(); }

    /// <summary>
    /// 옵션 팝업의 텍스트와 가격을 갱신한다.
    /// </summary>
    /// 옵션 팝업 안의 텍스트를 전체 갱신
    private void RefreshOptionUI()
    {
        int unitPrice = CalculateUnitPrice();
        int totalPrice = unitPrice * quantity;

        if (optionTitleText != null) optionTitleText.text = currentName;
        if (optionCategoryText != null) optionCategoryText.text = CategoryName(currentCategory);
        if (quantityText != null) quantityText.text = quantity.ToString();
        if (optionUnitPriceText != null) optionUnitPriceText.text = $"단가: {unitPrice:N0}원";
        if (optionSelectedText != null) optionSelectedText.text = CurrentOptionSummary();
        if (optionTotalPriceText != null) optionTotalPriceText.text = $"합계: {totalPrice:N0}원";
    }

    /// <summary>
    /// 선택된 옵션 기준으로 단가를 계산한다.
    /// </summary>
    private int CalculateUnitPrice()
    {
        int price = basePrice;

        price += size switch
        {
            "중" => 1000,
            "대" => 2000,
            _ => 0
        };

        if (sweetness == "높음") price += 1000;
        if (composition == "혼합") price += 1000;
        if (composition == "프리미엄") price += 2000;
        if (packageType == "리본") price += 2000;
        if (packageType == "고급") price += 5000;

        return price;
    }

    private string CurrentOptionSummary()
    {
        var options = new List<string>();
        if (IsActive(sizeGroup)) options.Add($"크기:{size}");
        if (IsActive(sweetGroup)) options.Add($"당도:{sweetness}");
        if (IsActive(cupGroup)) options.Add($"구성:{composition}");
        if (IsActive(packGroup)) options.Add($"포장:{packageType}");
        return string.Join(" / ", options);
    }

    /// <summary>
    /// 현재 선택한 상품을 장바구니에 추가한다.
    /// </summary>
    private void AddCurrentItemToCart()
    {
        int unitPrice = CalculateUnitPrice();

        cart.Add(new CartItem
        {
            name = currentName,
            category = currentCategory,
            size = IsActive(sizeGroup) ? size : "",
            sweetness = IsActive(sweetGroup) ? sweetness : "",
            composition = IsActive(cupGroup) ? composition : "",
            packageType = IsActive(packGroup) ? packageType : "",
            quantity = quantity,
            unitPrice = unitPrice,
            totalPrice = unitPrice * quantity
        });

        RefreshCartPanel();
        RefreshCartPopup();
        //panelManager.ClosePopup(panelManager.optionPopup);
    }
    // ProductPanel의 장바구니 요약 갱신
    public void RefreshCartPanel()
    {
        int totalCount = 0;
        int totalPrice = 0;

        foreach (var item in cart)
        {
            totalCount += item.quantity;
            totalPrice += item.totalPrice;
        }

        if (cartCountText != null) cartCountText.text = $"{totalCount}개 담김";
        if (cartTotalText != null) cartTotalText.text = $"합계: {totalPrice:N0}원";
        if (goPayBtn != null) goPayBtn.interactable = cart.Count > 0;
    }

    /// <summary>
    /// 장바구니 팝업 목록을 다시 그린다.
    /// </summary>
    /// ProductPanel의 장바구니 요약 갱신
    public void RefreshCartPopup()
    {
        ClearRows(cartRows);
        if (cartListContent == null || cartListRowPrefab == null) return;

        int totalPrice = 0;
        foreach (var item in cart)
        {
            var row = Instantiate(cartListRowPrefab, cartListContent);
            var texts = row.GetComponentsInChildren<Text>();

            SetText(texts, 0, item.name);
            SetText(texts, 1, item.OptionSummary());
            SetText(texts, 2, $"{item.quantity}개");
            SetText(texts, 3, $"{item.totalPrice:N0}원");

            cartRows.Add(row);
            totalPrice += item.totalPrice;
        }

        if (cartListTotalText != null) cartListTotalText.text = $"합계: {totalPrice:N0}원";
    }
    // 새 주문 시작 시 장바구니 비우기
    public void ClearCart()
    {
        cart.Clear();
        RefreshCartPanel();
        RefreshCartPopup();
    }

    private string CategoryName(ProductCategory category)
    {
        return category switch
        {
            ProductCategory.Fruit => "일반과일",
            ProductCategory.Cup => "컵과일",
            ProductCategory.Gift => "선물세트",
            _ => "상품"
        };
    }

    private void AddClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    private bool IsActive(GameObject obj)
    {
        return obj != null && obj.activeSelf;
    }

    private void ClearRows(List<GameObject> rows)
    {
        foreach (var row in rows) Destroy(row);
        rows.Clear();
    }

    private void SetText(Text[] texts, int index, string value)
    {
        if (texts.Length > index) texts[index].text = value;
    }
}
