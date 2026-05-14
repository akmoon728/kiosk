using UnityEngine;
using UnityEngine.UI;

public class Panel3_Product : MonoBehaviour
{
    //	카테고리 패널 (하단 좌측)
    ///	[배송] 선택 시 카테고리 tabGift만 작동?
    public Button tabFruit;
    public Button tabCup;
    public Button tabGift;

    public Button homeBtn;          // 이전페이지 이동(시작)

    //	메뉴그리드 패널 (하단 중앙)
    public GameObject gridFruit;
    public GameObject gridCup;
    public GameObject gridGift;

    public Button menuBtn;                  // 메뉴 선택 버튼
    public GameObject optionPopup;          // 메뉴 세부 옵션 선택(팝업)
                                // 옵션 선택 버튼
        public Button optionPopupCloseBtn;          // 창 끄기
        public Button optionPopupCompleteBtn;      // 선택완료

    //	카트 패널 (하단 우측)
    public GameObject cartList;         // 메뉴 선택 내역(scrollview)
    public Text cartCount;              // 총 수량
    public Text cartTotal;              // 총 합계

    //	탑 배너 (상단)
    public Image startImg;              // 주문방법
    public Image productImg;            // 메뉴선택
    public Image paymentImg;            // 결제


    //	외부 페이지 참조
    public GameObject startPanel;             // (이전)시작화면
    public GameObject paymentPanel;           // (다음)결제화면


    //
    void Start()
    {
        GoToMenuSelection();
        if (menuBtn) menuBtn.onClick.AddListener(() => OpenPopup(optionPopup));

        if (homeBtn) homeBtn.onClick.AddListener(GoToStart);
    }

    //	Set Active
    private void SetActive(GameObject obj, bool active)
    {
        if (obj) obj.SetActive(active);
    }

    //  상단 섹션 (페이지 안내)
    public void BannerImage(int idx)
    {
        //if (fruitTabBar) fruitTabBar.gameObject.SetActive(idx == 0);
        //if (cupTabBar) cupTabBar.gameObject.SetActive(idx == 1);
        //if (giftTabBar) giftTabBar.gameObject.SetActive(idx == 2);
        
        //  상단 탭 이미지 변경(픽업/배송)

        //  상단 탭 이미지 변경(현재페이지 해당 이미지 하이라이트)

    }
    //	카테고리 선택
    public void SwitchTab(int idx)
    {
        SetActive(gridFruit, idx == 0);
        SetActive(gridCup, idx == 1);
        SetActive(gridGift, idx == 2);
    }

    //	메뉴 선택(팝업창 띄우기)
    public void OpenPopup(GameObject popup)
    {
        if (popup) 
        {
            popup.SetActive(true);
            SelectPopupOption();
        }
    }

    //	메뉴 옵션 선택
    public void SelectPopupOption()
	{
        //	옵션 버튼(카테고리에 따라 별개 옵션 카테고리)

        //  끄기 버튼
        if(optionPopupCloseBtn)
        {
            optionPopupCloseBtn.onClick.AddListener(GoToMenuSelection);
            ClosePopup(optionPopup);   
        }

		//	선택완료 버튼
        if(optionPopupCompleteBtn)
        {
            optionPopupCloseBtn.onClick.AddListener(GoToMenuSelection);
            ClosePopup(optionPopup);
            //  최종 가격 반영해 주문서에 누적
        }
	}


    //	메뉴 옵션 팝업창 끄기
    public void ClosePopup(GameObject popup)
    {
        if (popup) popup.SetActive(false);
        //{ optionPopupCloseBtn.onClick.AddListener(() => ClosePopup(optionPopup));
    }

    //  메뉴 선택 페이지
    public void GoToMenuSelection()
    {
        if (tabFruit) tabFruit.onClick.AddListener(() => SwitchTab(0));
        if (tabCup) tabCup.onClick.AddListener(() => SwitchTab(1));
        if (tabGift) tabGift.onClick.AddListener(() => SwitchTab(2));
    }
    //	결제페이지 이동
    public void GoToPayment() => paymentPanel.SetActive(true);

    //	시작화면 초기화
    public void GoToStart() => startPanel.SetActive(true);

}
