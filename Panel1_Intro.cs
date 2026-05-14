using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Panel1_Intro : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public GameObject panelStart;       // 픽업 / 배송 선택 패널
    public GameObject panelIntro;

    private void Start()
    {
        panelStart.SetActive(false);
        startButton.onClick.AddListener(OnStartClicked);

    }

    public void OnStartClicked()
    {
        startButton.gameObject.SetActive(false);
        panelStart.SetActive(true);
        panelIntro.SetActive(false);

    }


}
