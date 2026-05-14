using UnityEngine;
using UnityEngine.UI;

public class Scr_ProductButton : MonoBehaviour // 모든 상품에 다 붙일 것 //나는 사과 버튼이다 알려주는 거
{
    public ProductData productData;

    public Scr_ProductPanelManager manager;

    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();

        btn.onClick.AddListener(OnClickProduct);
    }

    void OnClickProduct()
    {
        manager.OpenProduct(productData);
    }
}