using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scr_ProductPanelManager : MonoBehaviour
{
    [Header("Category Buttons")]
    public Button originalFruitButton;
    public Button cupFruitButton;
    public Button giftFruitButton;

    [Header("Category Grid")]
    public GameObject gridOriginalFruit;
    public GameObject gridCupFruit;
    public GameObject gridGiftFruit;

    [Header("Option Popup")]
    public GameObject optionPopup; //상품 클릭했을 때 뜨는 창

    public GameObject OriginalFruitOptionPanel;
    public GameObject cupFruitOptionPanel;
    public GameObject giftFruitOptionPanel;

    public Image popupProductImage;
    public TMP_Text popupProductNameText;
    public TMP_Text popupProductDescriptionText;

    public TMP_Text popupPriceText;

    [Header("Selected Option")]

    private string selectedGrade = "";
    //private string selectedPackage = "";

    private int optionPrice = 0;   //선택값 저장 변수 

    [Header("Original Fruit Grade Buttons")]
    public Button middleButton;
    public Button highButton;
    public Button premiumButton;

    [Header("Quantity")]
    public Button OriginalplusButton;
    public Button OriginalminusButton;

    public Button GiftplusButton;
    public Button GiftminusButton;

    public TMP_Text quantityText;

    private int quantity = 1;

    [Header("Popup Button")]
    public Button closeButton;


    [Header("Add Cart Button")]
    public Button addButton;


    [Header("Cart")]
    public Transform cartContent; // scrollview - viewport- content
    public GameObject cartItemPrefab; // 장바구니 한줄 프리팹

    public TMP_Text cartCountText;
    public TMP_Text cartTotalText;

    private ProductData currentProduct; //지금 내가 누른 상품

    private List<Scr_CartItemData> cartItems =
        new List<Scr_CartItemData>();


    [Header("Cart Clear Button")]
    public Button clearCartButton;


    private int totalCount;
    private int totalPrice;

    void Start()
    {
        ShowOriginalFruit();
        optionPopup.SetActive(false);
        closeButton.onClick.AddListener(ClosePopup);

        originalFruitButton.onClick.AddListener(ShowOriginalFruit);
        cupFruitButton.onClick.AddListener(ShowCupFruit);
        giftFruitButton.onClick.AddListener(ShowGiftFruit);


        middleButton.onClick.AddListener(() => SelectGrade("중"));
        highButton.onClick.AddListener(() => SelectGrade("상"));
        premiumButton.onClick.AddListener(() => SelectGrade("최상"));

        OriginalplusButton.onClick.AddListener(PlusQuantity);
        OriginalminusButton.onClick.AddListener(MinusQuantity);
     
        GiftplusButton.onClick.AddListener(PlusQuantity);
        GiftminusButton.onClick.AddListener(MinusQuantity);

        addButton.onClick.AddListener(AddToCart);
        clearCartButton.onClick.AddListener(ClearCart);
    }

    // 카테고리 변경

    public void ShowOriginalFruit()
    {
        gridOriginalFruit.SetActive(true);

        gridCupFruit.SetActive(false);
        gridGiftFruit.SetActive(false);
    }

    public void ShowCupFruit()
    {
        Debug.Log("컵");
        gridOriginalFruit.SetActive(false);

        gridCupFruit.SetActive(true);
        gridGiftFruit.SetActive(false);
    }

    public void ShowGiftFruit()
    {
        gridOriginalFruit.SetActive(false);
        gridCupFruit.SetActive(false);

        gridGiftFruit.SetActive(true);
    }


    // 상품 클릭
    

    public void OpenProduct(ProductData product)
    {
        currentProduct = product; //현재 상품 저장

        quantity = 1;
        selectedGrade = "중";
        optionPrice = 0;

        

        optionPopup.SetActive(true);

        popupProductImage.sprite =
            product.productImage;

        popupProductNameText.text =
            product.productName;
        popupProductDescriptionText.text =
            product.productDescription;

        popupPriceText.text =
            product.basePrice.ToString("N0") + "원"; // 숫자에 쉼표 넣기

        OriginalFruitOptionPanel.SetActive(false);
        cupFruitOptionPanel.SetActive(false);
        giftFruitOptionPanel.SetActive(false); // 우선 모든 옵션 패널을 끈 상태로 시작

        switch (product.category) // 상품 종류룰 검사해서 해당 옵션 패널 키기
        {
            case ProductCategory.NormalFruit:
                OriginalFruitOptionPanel.SetActive(true);
                break;

            case ProductCategory.CupFruit:
                //cupFruitOptionPanel.SetActive(true);
                break;

            case ProductCategory.GiftFruit:
                giftFruitOptionPanel.SetActive(true);
                break;

        }
        UpdatePrice();
    }

    // 팝업 닫기


    public void ClosePopup()
    {
        optionPopup.SetActive(false);
    }
    // 일반 과일 변수 함수 

    //private string selectedGrade = "중";
    //private int optionPrice = 0;
    private int finalPrice = 0;

    public void SelectGrade(string grade)
    {
        selectedGrade = grade;

        switch (grade)
        {
            case "중":
                optionPrice = 0;
                break;

            case "상":
                optionPrice = 2000;
                break;

            case "최상":
                optionPrice = 4000;
                break;
        }

        UpdatePrice();
    }

    void UpdatePrice()
    {
        finalPrice = (currentProduct.basePrice + optionPrice)*quantity;

        popupPriceText.text = finalPrice.ToString("N0") + "원";
        quantityText.text =
    quantity.ToString();

    }

    public void PlusQuantity()
    {
        Debug.Log(" 눌림");
        quantity++;

        UpdatePrice();
    }
    public void MinusQuantity()
    {
        if (quantity > 1)
        {
            quantity--;
        }

        UpdatePrice();
    }

    // 장바구니 추가


    public void AddToCart()
    {
        
        GameObject obj =
            Instantiate(cartItemPrefab, cartContent); // 새로운 장바구니 칸 생성
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;

        Scr_CartItem_UI ui =
            obj.GetComponent<Scr_CartItem_UI>(); // 생성된 클론 프리팹 안의 UI 가져오기

        ui.productImage.sprite =
            currentProduct.productImage;

        ui.productNameText.text =
            currentProduct.productName;

       

        ui.optionText.text = selectedGrade;

        ui.quantityText.text =
        quantity + "개";


        ui.priceText.text =
         finalPrice.ToString("N0") + "원"; 

        totalCount += quantity; //개수 증가
        totalPrice += finalPrice; //가격 증가

        cartCountText.text =
            totalCount + "개";

        cartTotalText.text =
            totalPrice.ToString("N0") + "원";

        //optionPopup.SetActive(false);
    }
    // 컵 과일만 따로 추가
    public void AddCupFruitToCart(ProductData product, int quantity)
    {
        if (quantity <= 0)
            return;

        GameObject obj =
            Instantiate(cartItemPrefab, cartContent);

        Scr_CartItem_UI ui =
            obj.GetComponent<Scr_CartItem_UI>();

        int price = product.basePrice * quantity;

        ui.productImage.sprite = product.productImage;
        ui.productNameText.text = product.productName;

        //ui.optionText.text = "컵과일";
        ui.quantityText.text = quantity + "개";
        ui.priceText.text = price.ToString("N0") + "원";

        totalCount += 1;
        totalPrice += product.basePrice;

        cartCountText.text = totalCount + "개";
        cartTotalText.text = totalPrice.ToString("N0") + "원";
    }
    //장바구니 삭제 

    public void ClearCart()
    {
        foreach (Transform child in cartContent)
        {
            Destroy(child.gameObject);
        }

        totalCount = 0;
        totalPrice = 0;

        cartCountText.text = "0개";
        cartTotalText.text = "0원";
    }
}