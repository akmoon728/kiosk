using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 전환 시에도 주문 데이터를 유지하는 싱글톤
/// DontDestroyOnLoad 적용
/// </summary>
public class OrderSession : MonoBehaviour
{
    public static OrderSession Instance { get; private set; }

    // ── 배송 방법 ──────────────────────────────
    public string DeliveryMethod { get; set; } = "pickup"; // "pickup" | "delivery"

    // ── 장바구니 ──────────────────────────────
    public List<CartItem> CartItems { get; private set; } = new List<CartItem>();

    // ── 결제 ──────────────────────────────────
    public string PaymentMethod { get; set; } = "";   // "cash" | "card" | "point"
    public int UsedPoints       { get; set; } = 0;
    public int TotalPrice       => CalculateTotal();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(CartItem item)
    {
        // 동일 상품+옵션 존재 시 수량 증가
        var existing = CartItems.Find(c =>
            c.ProductName == item.ProductName && c.OptionSummary == item.OptionSummary);
        if (existing != null) existing.Quantity += item.Quantity;
        else CartItems.Add(item);
    }

    public void RemoveItem(CartItem item) => CartItems.Remove(item);

    public void ClearCart()
    {
        CartItems.Clear();
        PaymentMethod = "";
        UsedPoints    = 0;
    }

    private int CalculateTotal()
    {
        int total = 0;
        foreach (var item in CartItems)
            total += item.UnitPrice * item.Quantity;
        return total - UsedPoints;
    }

    /// 배송 최소 kg 검증 (3 kg 이상)
    public bool ValidateDeliveryWeight()
    {
        if (DeliveryMethod != "delivery") return true;
        float totalKg = 0f;
        foreach (var item in CartItems) totalKg += item.WeightKg * item.Quantity;
        return totalKg >= 3f;
    }
}
