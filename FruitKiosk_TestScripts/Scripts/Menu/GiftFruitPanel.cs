using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 선물세트 옵션: 크기(소/중/대) + 포장(기본/리본/고급)
/// </summary>
public class GiftFruitPanel : MonoBehaviour
{
    [Header("상품 정보")]
    public string productName = "선물세트";
    public float  weightKg    = 2.5f;

    [Header("크기 옵션")]
    public Dropdown sizeDropdown;      // 소 / 중 / 대

    [Header("포장 옵션")]
    public Dropdown packagingDropdown; // 기본 / 리본 / 고급

    [Header("수량")]
    public InputField quantityInput;

    [Header("담기 버튼")]
    public Button addToCartButton;

    [Header("가격 표시")]
    public Text priceText;

    private readonly string[] sizes      = { "소(1.5kg)", "중(3kg)", "대(5kg)" };
    private readonly string[] packaging  = { "기본포장", "리본포장(+2,000)", "고급포장(+5,000)" };
    private readonly int[]    basePrices = { 25000, 45000, 70000 };
    private readonly int[]    packExtra  = { 0, 2000, 5000 };

    private void Start()
    {
        PopulateDropdown(sizeDropdown,      sizes);
        PopulateDropdown(packagingDropdown, packaging);

        sizeDropdown.onValueChanged.AddListener(      _ => UpdatePrice());
        packagingDropdown.onValueChanged.AddListener( _ => UpdatePrice());
        quantityInput.onValueChanged.AddListener(     _ => UpdatePrice());
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
        basePrices[sizeDropdown.value] + packExtra[packagingDropdown.value];

    private void UpdatePrice()
    {
        int qty = Mathf.Max(1, int.TryParse(quantityInput.text, out int q) ? q : 1);
        priceText.text = $"합계: {CalcUnitPrice() * qty:N0}원";
    }

    private void AddToCart()
    {
        int qty = Mathf.Max(1, int.TryParse(quantityInput.text, out int q) ? q : 1);
        string option = $"크기:{sizes[sizeDropdown.value]} / 포장:{packaging[packagingDropdown.value]}";
        var item = new CartItem(productName, "gift", option, CalcUnitPrice(), qty, weightKg * qty);
        OrderSession.Instance.AddItem(item);
        FindObjectOfType<MenuManager>()?.RefreshBadge();
    }
}
