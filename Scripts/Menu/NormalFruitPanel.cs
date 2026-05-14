using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일반과일 옵션: 등급(당도/크기) + 커팅 여부
/// </summary>
public class NormalFruitPanel : MonoBehaviour
{
    [Header("상품 정보")]
    public string productName = "사과";
    public int    basePrice   = 8000;
    public float  weightKg    = 1.5f;

    [Header("등급 옵션")]
    public Dropdown gradeDropdown;  // 특상(당도↑)/상(보통)/중(일반)

    [Header("크기 옵션")]
    public Dropdown sizeDropdown;   // 대/중/소

    [Header("커팅 옵션")]
    public Toggle   cuttingToggle;
    public int      cuttingExtraPrice = 1000;

    [Header("수량")]
    public InputField quantityInput;

    [Header("담기 버튼")]
    public Button addToCartButton;

    [Header("가격 표시")]
    public Text priceText;

    private readonly string[] grades = { "특상(고당도)", "상(보통)", "중(일반)" };
    private readonly string[] sizes  = { "대", "중", "소" };
    private readonly int[]    gradeExtraPrice = { 2000, 1000, 0 };

    private void Start()
    {
        PopulateDropdown(gradeDropdown, grades);
        PopulateDropdown(sizeDropdown,  sizes);

        gradeDropdown.onValueChanged.AddListener(_ => UpdatePrice());
        cuttingToggle.onValueChanged.AddListener(_ => UpdatePrice());
        quantityInput.onValueChanged.AddListener(_ => UpdatePrice());
        addToCartButton.onClick.AddListener(AddToCart);

        quantityInput.text = "1";
        UpdatePrice();
    }

    private void PopulateDropdown(Dropdown dd, string[] options)
    {
        dd.ClearOptions();
        var list = new System.Collections.Generic.List<string>(options);
        dd.AddOptions(list);
    }

    private int CalcUnitPrice()
    {
        int price = basePrice + gradeExtraPrice[gradeDropdown.value];
        if (cuttingToggle.isOn) price += cuttingExtraPrice;
        return price;
    }

    private void UpdatePrice()
    {
        int qty  = Mathf.Max(1, int.TryParse(quantityInput.text, out int q) ? q : 1);
        priceText.text = $"합계: {CalcUnitPrice() * qty:N0}원";
    }

    private void AddToCart()
    {
        int qty = Mathf.Max(1, int.TryParse(quantityInput.text, out int q) ? q : 1);

        string option = $"등급:{grades[gradeDropdown.value]} / 크기:{sizes[sizeDropdown.value]} / 커팅:{(cuttingToggle.isOn ? "O" : "X")}";
        var item = new CartItem(productName, "normal", option, CalcUnitPrice(), qty, weightKg * qty);

        OrderSession.Instance.AddItem(item);
        FindObjectOfType<MenuManager>()?.RefreshBadge();
        Debug.Log($"[장바구니] {productName} 추가 완료");
    }
}
