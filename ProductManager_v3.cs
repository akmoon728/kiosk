using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ================================================================
//  ProductManager_v3.cs
//  역할:
//  - 일반과일 / 컵과일 / 선물세트 상품 버튼 클릭 처리
//  - 옵션 팝업에서 크기/당도/구성/포장 선택
//  - 수량 조절 후 장바구니 담기
//  - 장바구니 목록/합계 갱신
//
//  상품 버튼 연결 예시:
//  일반과일 버튼 OnClick -> OpenFruitOption("사과", 5000)
//  컵과일 버튼 OnClick   -> OpenCupOption("딸기컵", 7000)
//  선물세트 버튼 OnClick -> OpenGiftOption("프리미엄세트", 30000)
// ================================================================
public class ProductManager_v3 : MonoBehaviour
{
    [Header("매니저 참조")]
    public PanelManager_v3 panelManager;

    [Header("옵션 팝업 공통 UI")]
    public Text optionTitleText;          // 팝업 제목: 사과 / 딸기컵 / 선물세트
    public Text optionCategoryText;       // 카테고리명 표시
    public Text optionUnitPriceText;      // 단가 표시
    public Text optionSelectedText;       // 현재 선택 옵션 요약
    public Text optionTotalPriceText;     // 최종 합계 표시

    [Header("수량 UI")]
    public Button minusBtn;
    public Text quantityText;
    public Button plusBtn;
    public Button addToCartBtn;

    [Header("옵션 버튼 그룹 - 크기")]
    public Button sizeSmallBtn;
    public Button sizeMediumBtn;
    public Button sizeLargeBtn;

    [Header("옵션 버튼 그룹 - 당도")]
    public Button sweetLowBtn;
    public Button sweetMediumBtn;
    public Button sweetHighBtn;

    [Header("옵션 버튼 그룹 - 컵 구성")]
    public Button cupSingleBtn;
    public Button cupMixBtn;
    public Button cupPremiumBtn;

    [Header("옵션 버튼 그룹 - 포장")]
    public Button packBasicBtn;
    public Button packRibbonBtn;
    public Button packLuxuryBtn;

    [Header("옵션 영역 그룹 오브젝트")]
    public GameObject sizeGroup;
    public GameObject sweetGroup;
    public GameObject cupGroup;
    public GameObject packGroup;

    [Header("장바구니 요약 UI")]
    public Text cartCountText;
    public Text cartTotalText;
    public Button goPayBtn;

    [Header("구매목록 팝업 UI")]
    public Transform cartListContent;
    public Text cartListTotalText;
    public GameObject cartListRowPrefab;

    public class CartItem
    {
        public string name;
        public string category;
        public string size;
        public string sweet;
        public string composition;
        public string packageType;
        public int quantity;
        public int unitPrice;
        public int totalPrice;

        public string GetOptionSummary()
        {
            List<string> options = new List<string>();
            if (!string.IsNullOrEmpty(size)) options.Add($"크기:{size}");
            if (!string.IsNullOrEmpty(sweet)) options.Add($"당도:{sweet}");
            if (!string.IsNullOrEmpty(composition)) options.Add($"구성:{composition}");
            if (!string.IsNullOrEmpty(packageType)) options.Add($"포장:{packageType}");
            return string.Join(" / ", options);
        }
    }

    [HideInInspector] public readonly List<CartItem> cart = new List<CartItem>();

    private readonly List<GameObject> _cartRows = new List<GameObject>();

    private string _currentCategory = "";
    private string _currentName = "";
    private int _basePrice = 0;
    private int _quantity = 1;

    private string _selectedSize = "중";
    private string _selectedSweet = "보통";
    private string _selectedComposition = "혼합";
    private string _selectedPackage = "기본";

    void Start()
    {
        if (minusBtn) minusBtn.onClick.AddListener(() =>
        {
            _quantity = Mathf.Max(1, _quantity - 1);
            RefreshOptionUI();
        });

        if (plusBtn) plusBtn.onClick.AddListener(() =>
        {
            _quantity++;
            RefreshOptionUI();
        });

        if (addToCartBtn) addToCartBtn.onClick.AddListener(AddCurrentItemToCart);

        if (sizeSmallBtn) sizeSmallBtn.onClick.AddListener(() => { _selectedSize = "소"; RefreshOptionUI(); });
        if (sizeMediumBtn) sizeMediumBtn.onClick.AddListener(() => { _selectedSize = "중"; RefreshOptionUI(); });
        if (sizeLargeBtn) sizeLargeBtn.onClick.AddListener(() => { _selectedSize = "대"; RefreshOptionUI(); });

        if (sweetLowBtn) sweetLowBtn.onClick.AddListener(() => { _selectedSweet = "낮음"; RefreshOptionUI(); });
        if (sweetMediumBtn) sweetMediumBtn.onClick.AddListener(() => { _selectedSweet = "보통"; RefreshOptionUI(); });
        if (sweetHighBtn) sweetHighBtn.onClick.AddListener(() => { _selectedSweet = "높음"; RefreshOptionUI(); });

        if (cupSingleBtn) cupSingleBtn.onClick.AddListener(() => { _selectedComposition = "단일"; RefreshOptionUI(); });
        if (cupMixBtn) cupMixBtn.onClick.AddListener(() => { _selectedComposition = "혼합"; RefreshOptionUI(); });
        if (cupPremiumBtn) cupPremiumBtn.onClick.AddListener(() => { _selectedComposition = "프리미엄"; RefreshOptionUI(); });

        if (packBasicBtn) packBasicBtn.onClick.AddListener(() => { _selectedPackage = "기본"; RefreshOptionUI(); });
        if (packRibbonBtn) packRibbonBtn.onClick.AddListener(() => { _selectedPackage = "리본"; RefreshOptionUI(); });
        if (packLuxuryBtn) packLuxuryBtn.onClick.AddListener(() => { _selectedPackage = "고급"; RefreshOptionUI(); });

        if (goPayBtn) goPayBtn.interactable = false;
    }

    // ---------------------------
    // 일반과일 옵션 팝업 열기
    // 크기 + 당도 선택창 표시
    // ---------------------------
    public void OpenFruitOption(string productName, int basePrice)
    {
        _currentCategory = "일반과일";
        _currentName = productName;
        _basePrice = basePrice;
        _quantity = 1;
        _selectedSize = "중";
        _selectedSweet = "보통";
        _selectedComposition = "";
        _selectedPackage = "";

        SetOptionGroups(showSize: true, showSweet: true, showCup: false, showPack: false);
        RefreshOptionUI();
        panelManager.OpenPopup(panelManager.optionPopup);
    }

    // ---------------------------
    // 컵과일 옵션 팝업 열기
    // 크기 + 구성 선택창 표시
    // ---------------------------
    public void OpenCupOption(string productName, int basePrice)
    {
        _currentCategory = "컵과일";
        _currentName = productName;
        _basePrice = basePrice;
        _quantity = 1;
        _selectedSize = "중";
        _selectedSweet = "";
        _selectedComposition = "혼합";
        _selectedPackage = "";

        SetOptionGroups(showSize: true, showSweet: false, showCup: true, showPack: false);
        RefreshOptionUI();
        panelManager.OpenPopup(panelManager.optionPopup);
    }

    // ---------------------------
    // 선물세트 옵션 팝업 열기
    // 크기 + 포장 선택창 표시
    // ---------------------------
    public void OpenGiftOption(string productName, int basePrice)
    {
        _currentCategory = "선물세트";
        _currentName = productName;
        _basePrice = basePrice;
        _quantity = 1;
        _selectedSize = "중";
        _selectedSweet = "";
        _selectedComposition = "";
        _selectedPackage = "기본";

        SetOptionGroups(showSize: true, showSweet: false, showCup: false, showPack: true);
        RefreshOptionUI();
        panelManager.OpenPopup(panelManager.optionPopup);
    }

    private void SetOptionGroups(bool showSize, bool showSweet, bool showCup, bool showPack)
    {
        if (sizeGroup) sizeGroup.SetActive(showSize);
        if (sweetGroup) sweetGroup.SetActive(showSweet);
        if (cupGroup) cupGroup.SetActive(showCup);
        if (packGroup) packGroup.SetActive(showPack);
    }

    private void RefreshOptionUI()
    {
        if (optionTitleText) optionTitleText.text = _currentName;
        if (optionCategoryText) optionCategoryText.text = _currentCategory;
        if (quantityText) quantityText.text = _quantity.ToString();

        int unitPrice = CalculateUnitPrice();
        int totalPrice = unitPrice * _quantity;

        if (optionUnitPriceText) optionUnitPriceText.text = $"단가: {unitPrice:N0}원";
        if (optionSelectedText) optionSelectedText.text = BuildCurrentOptionSummary();
        if (optionTotalPriceText) optionTotalPriceText.text = $"합계: {totalPrice:N0}원";
    }

    private int CalculateUnitPrice()
    {
        int price = _basePrice;

        if (_selectedSize == "소") price += 0;
        else if (_selectedSize == "중") price += 1000;
        else if (_selectedSize == "대") price += 2000;

        if (_selectedSweet == "높음") price += 1000;

        if (_selectedComposition == "프리미엄") price += 2000;
        else if (_selectedComposition == "혼합") price += 1000;

        if (_selectedPackage == "리본") price += 2000;
        else if (_selectedPackage == "고급") price += 5000;

        return price;
    }

    private string BuildCurrentOptionSummary()
    {
        List<string> options = new List<string>();

        if (sizeGroup && sizeGroup.activeSelf) options.Add($"크기:{_selectedSize}");
        if (sweetGroup && sweetGroup.activeSelf) options.Add($"당도:{_selectedSweet}");
        if (cupGroup && cupGroup.activeSelf) options.Add($"구성:{_selectedComposition}");
        if (packGroup && packGroup.activeSelf) options.Add($"포장:{_selectedPackage}");

        return string.Join(" / ", options);
    }

    private void AddCurrentItemToCart()
    {
        CartItem item = new CartItem();
        item.name = _currentName;
        item.category = _currentCategory;
        item.size = sizeGroup && sizeGroup.activeSelf ? _selectedSize : "";
        item.sweet = sweetGroup && sweetGroup.activeSelf ? _selectedSweet : "";
        item.composition = cupGroup && cupGroup.activeSelf ? _selectedComposition : "";
        item.packageType = packGroup && packGroup.activeSelf ? _selectedPackage : "";
        item.quantity = _quantity;
        item.unitPrice = CalculateUnitPrice();
        item.totalPrice = item.unitPrice * item.quantity;

        cart.Add(item);

        RefreshCartPanel();
        RefreshCartListPopup();
        panelManager.ClosePopup(panelManager.optionPopup);
    }

    public void RefreshCartPanel()
    {
        int totalCount = 0;
        int totalPrice = 0;

        foreach (var item in cart)
        {
            totalCount += item.quantity;
            totalPrice += item.totalPrice;
        }

        if (cartCountText) cartCountText.text = $"{totalCount}개 담김";
        if (cartTotalText) cartTotalText.text = $"합계: {totalPrice:N0}원";
        if (goPayBtn) goPayBtn.interactable = cart.Count > 0;
    }

    public void RefreshCartListPopup()
    {
        foreach (var row in _cartRows) Destroy(row);
        _cartRows.Clear();

        if (cartListContent == null || cartListRowPrefab == null) return;

        int totalPrice = 0;

        foreach (var item in cart)
        {
            GameObject row = Instantiate(cartListRowPrefab, cartListContent);
            Text[] txts = row.GetComponentsInChildren<Text>();

            if (txts.Length > 0) txts[0].text = item.name;
            if (txts.Length > 1) txts[1].text = item.GetOptionSummary();
            if (txts.Length > 2) txts[2].text = $"{item.quantity}개";
            if (txts.Length > 3) txts[3].text = $"{item.totalPrice:N0}원";

            _cartRows.Add(row);
            totalPrice += item.totalPrice;
        }

        if (cartListTotalText) cartListTotalText.text = $"합계: {totalPrice:N0}원";
    }

    public void ClearCart()
    {
        cart.Clear();
        RefreshCartPanel();
        RefreshCartListPopup();
    }
}
