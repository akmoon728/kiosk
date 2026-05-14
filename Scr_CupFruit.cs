using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scr_CupFruit : MonoBehaviour
{
    public ProductData productData;
    public Scr_ProductPanelManager manager;

    public Button plusButton;
    public Button minusButton;
    public TMP_Text quantityText;

    private int quantity = 0;

    void Start()
    {
        plusButton.onClick.AddListener(PlusCup);
        minusButton.onClick.AddListener(MinusCup);

        quantityText.text = quantity.ToString();
    }

    public void PlusCup()
    {
        Debug.Log("플러스 눌림");
        quantity++;
        quantityText.text = quantity.ToString();

        manager.AddCupFruitToCart(productData, quantity);
    }

    public void MinusCup()
    {
        if (quantity > 0)
        {
            quantity--;
        }

        quantityText.text = quantity.ToString();

        manager.AddCupFruitToCart(productData, quantity);
    }
}