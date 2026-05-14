using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




    // ================================================================
    // KioskSimple.cs
    // [ 과일가게 키오스크 - 단순화 버전 ]
    //
    // Canvas
    // ├ StartPanel → PickupButton / DeliveryButton
    // ├ ProductPanel → 카테고리탭 / 상품버튼 / 장바구니
    // ├ PaymentPanel → Card / Cash / Point
    // └ CompletePanel → ReceiptButton / HomeButton
    //
    // - 클래스 1개
    // - 드롭다운 없음, 버튼으로만 옵션 선택
    // - 상품 카드 버튼 클릭 → 즉시 장바구니 추가
    // ================================================================
    public class Test : MonoBehaviour
    {
        // ── 패널 ──────────────────────────────────────────────────
        [Header("패널")]
        public GameObject startPanel;
        public GameObject productPanel;
        public GameObject paymentPanel;
        public GameObject completePanel;
        public GameObject tempPanel;

        // ── StartPanel ────────────────────────────────────────────
        [Header("StartPanel")]
        public Button pickupBtn;
        public Button deliveryBtn;

        // ── ProductPanel ──────────────────────────────────────────
        [Header("ProductPanel / 카테고리 탭")]
        public Button tabFruit;
        public Button tabCup;
        public Button tabGift;
        public GameObject gridFruit;   // 일반과일 버튼 그룹
        public GameObject gridCup;     // 컵과일 버튼 그룹
        public GameObject gridGift;    // 선물세트 버튼 그룹

        [Header("ProductPanel / 장바구니")]
        public Text cartCountText;    // "3개 담김"
        public Text cartTotalText;    // "합계: 12,000원"
        public Button goPayBtn;        // 결제하기 버튼

        // ── PaymentPanel ──────────────────────────────────────────
        [Header("PaymentPanel")]
        public Button cardBtn;
        public Button cashBtn;
        public Button pointBtn;
        public Text payAmountText;   // "결제금액: N원"
        public Button confirmBtn;      // 결제 확인
        public Button backBtn;         // 뒤로

        /*// ── CompletePanel ─────────────────────────────────────────
        [Header("CompletePanel")]
        public Text successText;
        public Text receiptText;     // 영수증 내용
        public Button homeBtn;*/

       // ── tempPanel ─────────────────────────────────────────
       [Header("tempPanel")]
        public Text successesText;
        public Text receiptesText;     // 영수증 내용
        public Button homeesBtn;
        public Button cancelBtn;


    // ── 내부 데이터 ───────────────────────────────────────────
    private string _delivery = "pickup";
        private string _payMethod = "";
        private readonly List<(string name, int price)> _cart = new();

        // =========================================================
        void Start()
        {
            // StartPanel 버튼
            pickupBtn.onClick.AddListener(() => { _delivery = "pickup"; Show(productPanel); });
            deliveryBtn.onClick.AddListener(() => { _delivery = "delivery"; Show(productPanel); });

            // 카테고리 탭
            tabFruit.onClick.AddListener(() => SwitchTab(0));
            tabCup.onClick.AddListener(() => SwitchTab(1));
            tabGift.onClick.AddListener(() => SwitchTab(2));

            // 결제 버튼
            cardBtn.onClick.AddListener(() => SelectPay("카드"));
            cashBtn.onClick.AddListener(() => SelectPay("현금"));
            pointBtn.onClick.AddListener(() => SelectPay("포인트"));
            confirmBtn.onClick.AddListener(Confirm);
            backBtn.onClick.AddListener(() => Show(productPanel));

            // 홈 버튼
            homeBtn.onClick.AddListener(GoHome);

            // 장바구니 이동
            goPayBtn.onClick.AddListener(() => Show(paymentPanel));

            // 초기 화면
            Show(startPanel);
            SwitchTab(0);
        }

        // =========================================================
        //  패널 전환
        // =========================================================
        void Show(GameObject target)
        {
            startPanel.SetActive(target == startPanel);
            productPanel.SetActive(target == productPanel);
            paymentPanel.SetActive(target == paymentPanel);
            completePanel.SetActive(target == completePanel);
            tempPanel.SetActive(target == tempPanel);

            if (target == paymentPanel) RefreshPayAmount();
            if (target == completePanel) ShowReceipt();
        }

        // =========================================================
        //  카테고리 탭 전환
        // =========================================================
        void SwitchTab(int idx)
        {
            gridFruit.SetActive(idx == 0);
            gridCup.SetActive(idx == 1);
            gridGift.SetActive(idx == 2);

        }

        // =========================================================
        //  상품 추가 (ProductGrid 안의 상품 버튼에서 호출)
        //
        //  상품 버튼 Inspector 설정:
        //  Button → OnClick() → KioskSimple.AddItem("사과", 8000)
        // =========================================================
        public void AddItem(string itemName, int price)
        {
            _cart.Add((itemName, price));
            RefreshCart();
        }

        void RefreshCart()
        {
            int total = 0;
            foreach (var item in _cart) total += item.price;
            cartCountText.text = $"{_cart.Count}개 담김";
            cartTotalText.text = $"합계: {total:N0}원";
            goPayBtn.interactable = _cart.Count > 0;
        }

        // =========================================================
        //  결제 방법 선택
        // =========================================================
        void SelectPay(string method)
        {
            _payMethod = method;
            confirmBtn.interactable = true;
        }

        void RefreshPayAmount()
        {
            int total = 0;
            foreach (var item in _cart) total += item.price;
            payAmountText.text = $"결제금액: {total:N0}원";
            confirmBtn.interactable = false;
        }

        void Confirm()
        {
            if (string.IsNullOrEmpty(_payMethod)) return;
            Show(completePanel);
        }

        // =========================================================
        //  영수증 생성
        // =========================================================
        void ShowReceipt()
        {
            successText.text = "주문 완료!";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{System.DateTime.Now:MM/dd HH:mm}]  수령:{_delivery}");
            sb.AppendLine("─────────────────");

            int total = 0;
            foreach (var item in _cart)
            {
                sb.AppendLine($"  {item.name}  {item.price:N0}원");
                total += item.price;
            }

            sb.AppendLine("─────────────────");
            sb.AppendLine($"  합계: {total:N0}원");
            sb.AppendLine($"  결제: {_payMethod}");
            receiptText.text = sb.ToString();
        }

        // =========================================================
        //  홈으로
        // =========================================================
        void GoHome()
        {
            _cart.Clear();
            _payMethod = "";
            RefreshCart();
            Show(startPanel);
        }

    }

