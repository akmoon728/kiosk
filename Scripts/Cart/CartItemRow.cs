using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장바구니 한 줄 (프리팹 스크립트)
/// 프리팹 구성: 상품명 Text / 옵션 Text / 수량 Text / 금액 Text / 삭제 Button
/// </summary>
public class CartItemRow : MonoBehaviour
{
    public Text   nameText;
    public Text   optionText;
    public Text   quantityText;
    public Text   priceText;
    public Button deleteButton;

    private CartItem    _item;
    private CartManager _manager;

    public void Setup(CartItem item, CartManager manager)
    {
        _item    = item;
        _manager = manager;

        nameText.text     = item.ProductName;
        optionText.text   = item.OptionSummary;
        quantityText.text = $"x{item.Quantity}";
        priceText.text    = $"{item.UnitPrice * item.Quantity:N0}원";

        deleteButton.onClick.AddListener(Delete);
    }

    private void Delete()
    {
        OrderSession.Instance.RemoveItem(_item);
        _manager.RenderCart();
    }
}
