using UnityEngine;
using UnityEngine.UI;

public class Panel2_Start : MonoBehaviour
{
    [Header("Start Buttons")]
    public Button startButton1;
    public Button startButton2;

    [Header("Panels")]
    public GameObject NextPanel;
    public GameObject CurPanel;    // 현재 시작 화면 패널

    [Header("Order Buttons")]
    public Button deliveryButton;
    public Button pickupButton;

    [Header("Images")]
    public GameObject pickupImage;
    public GameObject deliveryImage;

    private void Start()
    {

        NextPanel.SetActive(false);
        pickupImage.SetActive(false);
        deliveryImage.SetActive(false);


        startButton1.onClick.AddListener(OnStartClicked);
        startButton2.onClick.AddListener(OnStartClicked);


        deliveryButton.onClick.AddListener(DeliveryiconAppear);
        pickupButton.onClick.AddListener(PickupiconAppear);
    }

    public void OnStartClicked()
    {
        // 시작 화면 끄기
        CurPanel.SetActive(false);

        // 다음 패널 켜기
        NextPanel.SetActive(true);
    }

    public void PickupiconAppear()
    {
        pickupImage.SetActive(true);
        deliveryImage.SetActive(false);
    }

    public void DeliveryiconAppear()
    {


        deliveryImage.SetActive(true);
        pickupImage.SetActive(false);
    }
}
