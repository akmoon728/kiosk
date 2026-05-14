using UnityEngine;

public enum ProductCategory
{
    NormalFruit,
    CupFruit,
    GiftFruit
}

[System.Serializable]

public class ProductData
{
    public string productName;
    public string productDescription;


    public Sprite productImage;

    public int basePrice;

    public ProductCategory category;
}