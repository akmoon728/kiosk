using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 컵과일 옵션: 구성(단품/혼합) + 크기(소/중/대)
/// </summary>
public class CupFruitPanel : MonoBehaviour
{
    [Header("상품 정보")]
    public string productName = "컵과일";
    public float  weightKg    = 0.3f;

    [Header("구성 옵션")]
    public Dropdown compositionDropdown; // 단품 / 혼합

    [Header("크기 옵션")]
    public Dropdown sizeDropdown;        // 소 / 중 / 대

    [Header("수량")]
    public InputField quantityInput;

    [Header("담기 버튼")]
    public Button addToCartButton;

    [Header("가격 표시")]
    public Text priceText;

    private readonly string[] compositions  = { "단품", "혼합" };
    private readonly string[] sizes         = { "소(200g)", "중(350g)", "대(500g)" };
    private readonly int[]    basePrices    = { 3000, 5000, 7000 };  // 크기 기준 가격
    private readonly int[]    compExtra     = { 0, 500 };            // 혼합 추가 요금

    private void Start()
    {
        PopulateDropdown(compositionDropdown, compositions);
        PopulateDropdown(sizeDropdown, sizes);

        compositionDropdown.onValueChanged.AddListener(_ => UpdatePrice());
        sizeDropdown.onValueChanged.AddListener(_ => UpdatePrice());
        quantityInput.onValueChanged.AddListener(_ => UpdatePrice());
        addToCartButton.onClick.AddListener(AddToCart);

        quantityInput.text = "1";
        UpdatePrice();
    }

    private void PopulateDropdown(Dropdown dd, string[] options)
    {
        dd.ClearOptions();
        dd.AddOptions(new System.Collections.Generic.List<string>(options));
    }

    private int CalcUnitPrice() =>
        basePrices[sizeDropdown.value] + compExtra[compositionDropdown.value];

    private void UpdatePrice()
    {
        int qty = Mathf.Max(1, int.TryParse(quantityInput.text, out int q) ? q : 1);
        priceText.text = $"합계: {CalcUnitPrice() * qty:N0}원";
    }

    private void AddToCart()
    {
        int qty = Mathf.Max(1, int.TryParse(quantityInput.text, out int q) ? q : 1);
        string option = $"구성:{compositions[compositionDropdown.value]} / 크기:{sizes[sizeDropdown.value]}";
        var item = new CartItem(productName, "cup", option, CalcUnitPrice(), qty, weightKg * qty);
        OrderSession.Instance.AddItem(item);
        FindObjectOfType<MenuManager>()?.RefreshBadge();
    }
}
