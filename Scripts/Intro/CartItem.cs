/// <summary>
/// 장바구니 아이템 데이터 모델
/// </summary>
[System.Serializable]
public class CartItem
{
    public string ProductName;    // 상품명
    public string Category;       // "normal" | "cup" | "gift"
    public string OptionSummary;  // 옵션 요약 문자열 (표시용)
    public int    UnitPrice;      // 단가 (원)
    public int    Quantity;       // 수량
    public float  WeightKg;       // kg (배송 검증용)

    public CartItem(string name, string category, string option, int price, int qty, float kg)
    {
        ProductName   = name;
        Category      = category;
        OptionSummary = option;
        UnitPrice     = price;
        Quantity      = qty;
        WeightKg      = kg;
    }
}
