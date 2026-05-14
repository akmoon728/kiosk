using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class KioskDrinkMenu : MonoBehaviour
{
    private const string AllCategory = "\uC804\uCCB4";
    private const string RecommendedCategory = "\uC624\uB298\uC758 \uCD94\uCC9C \uBA54\uB274";
    private const string CoffeeCategory = "\uCEE4\uD53C";
    private const string AdeCategory = "\uC5D0\uC774\uB4DC";
    private const string SideCategory = "\uC0AC\uC774\uB4DC";
    private const string AdminPassword = "1234";

    private static bool initialized;

    private Canvas canvas;
    private GameObject menuRoot;
    private GameObject adminLoginRoot;
    private GameObject adminLoginAlertRoot;
    private GameObject adminRoot;
    private GameObject adminSalesView;
    private GameObject adminMenuView;
    private GameObject salesGraphRoot;
    private GameObject gridRoot;
    private GameObject alertRoot;
    private GameObject confirmRoot;
    private GameObject paymentRoot;
    private GameObject receiptRoot;
    private GameObject detailRoot;
    private GameObject temperatureGroup;
    private GameObject customGroup;
    private GameObject[] startScreenRoots;
    private Text orderTypeText;
    private Text selectedItemsText;
    private Text totalText;
    private Text confirmItemsText;
    private Text confirmTotalText;
    private Text paymentTotalText;
    private Text detailTitleText;
    private Text detailPriceText;
    private Text adminLoginErrorText;
    private Text salesSummaryText;
    private InputField adminPasswordInput;
    private Font uiFont;
    private string selectedCategory = AllCategory;
    private int adminClickCount;
    private int totalSalesAmount;
    private int totalOrderCount;
    private int totalSoldQuantity;
    private int lastOrderAmount;
    private int detailItemIndex;
    private string detailTemperature = "\uC544\uC774\uC2A4";
    private bool detailLarge;
    private bool detailShot;
    private bool detailExtraShot;
    private bool detailSyrup;

    private readonly Dictionary<string, Button> categoryButtons = new Dictionary<string, Button>();
    private readonly Dictionary<int, Text> quantityTexts = new Dictionary<int, Text>();
    private readonly Dictionary<int, Text> adminStatusTexts = new Dictionary<int, Text>();
    private readonly Dictionary<int, int> quantities = new Dictionary<int, int>();
    private readonly List<OrderLine> orderLines = new List<OrderLine>();
    private readonly List<int> recentSaleAmounts = new List<int>();
    private readonly List<Button> temperatureButtons = new List<Button>();
    private readonly List<Button> sizeButtons = new List<Button>();
    private readonly List<Button> customButtons = new List<Button>();
    private bool[] soldOutItems;

    private readonly string[] categories =
    {
        AllCategory,
        RecommendedCategory,
        CoffeeCategory,
        AdeCategory,
        SideCategory,
    };

    private readonly MenuItem[] items =
    {
        new MenuItem("\uC544\uBA54\uB9AC\uCE74\uB178", CoffeeCategory, 3500, new Color32(98, 59, 35, 255), new Color32(245, 245, 245, 255), false, false),
        new MenuItem("\uCE74\uD398\uB77C\uB5BC", CoffeeCategory, 4000, new Color32(188, 137, 83, 255), new Color32(248, 241, 227, 255), false, true),
        new MenuItem("\uBC14\uB2D0\uB77C\uB77C\uB5BC", CoffeeCategory, 4500, new Color32(219, 174, 91, 255), new Color32(255, 249, 232, 255), false, false),
        new MenuItem("\uB808\uBAAC\uC5D0\uC774\uB4DC", AdeCategory, 4800, new Color32(237, 204, 64, 255), new Color32(251, 255, 226, 255), false, true),
        new MenuItem("\uCCAD\uD3EC\uB3C4\uC5D0\uC774\uB4DC", AdeCategory, 5000, new Color32(94, 184, 92, 255), new Color32(239, 255, 234, 255), false, false),
        new MenuItem("\uC790\uBABD\uC5D0\uC774\uB4DC", AdeCategory, 5000, new Color32(235, 102, 82, 255), new Color32(255, 237, 232, 255), false, false),
        new MenuItem("\uCE58\uC988\uCF00\uC774\uD06C", SideCategory, 5500, new Color32(255, 220, 120, 255), new Color32(246, 246, 246, 255), true, true),
        new MenuItem("\uCD08\uCF54\uCFE0\uD0A4", SideCategory, 2500, new Color32(92, 54, 35, 255), new Color32(246, 246, 246, 255), true, false),
        new MenuItem("\uD5C8\uB2C8\uBE0C\uB808\uB4DC", SideCategory, 6500, new Color32(218, 157, 75, 255), new Color32(246, 246, 246, 255), true, false),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        BuildForActiveScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuildForActiveScene();
    }

    private static void BuildForActiveScene()
    {
        Canvas foundCanvas = FindObjectOfType<Canvas>();
        if (foundCanvas == null || foundCanvas.transform.Find("DrinkMenu") != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(KioskDrinkMenu));
        KioskDrinkMenu menu = managerObject.AddComponent<KioskDrinkMenu>();
        menu.canvas = foundCanvas;
        menu.Build();
    }

    private void Build()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        soldOutItems = new bool[items.Length];

        startScreenRoots = new GameObject[canvas.transform.childCount];
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            startScreenRoots[i] = canvas.transform.GetChild(i).gameObject;
        }

        CreateMenuRoot();
        CreateHeader();
        CreateCategoryTabs();
        CreateDrinkGrid();
        CreateFooter();
        CreateAlert();
        CreateConfirmModal();
        CreatePaymentModal();
        CreateReceiptModal();
        CreateDetailModal();
        CreateAdminLoginScreen();
        CreateAdminScreen();
        ConnectOrderButtons();
        ConnectBackgroundAdminTrigger();
        menuRoot.SetActive(false);
        adminLoginRoot.SetActive(false);
        adminRoot.SetActive(false);
    }

    private void Update()
    {
        if (adminLoginRoot == null || !adminLoginRoot.activeInHierarchy)
        {
            return;
        }

        if (adminLoginAlertRoot != null && adminLoginAlertRoot.activeInHierarchy)
        {
            return;
        }

        if (IsSubmitKeyPressed())
        {
            SubmitAdminLogin();
        }
    }

    private void ConnectOrderButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            Text label = button.GetComponentInChildren<Text>(true);
            if (label == null)
            {
                continue;
            }

            string text = label.text.Trim();
            if (text == "\uB9E4\uC7A5" || text == "\uD3EC\uC7A5")
            {
                string orderType = text;
                button.onClick.AddListener(() => ShowDrinkMenu(orderType));
            }
        }
    }

    private void ShowDrinkMenu(string orderType)
    {
        adminRoot.SetActive(false);

        foreach (GameObject root in startScreenRoots)
        {
            if (root != null && root != menuRoot)
            {
                root.SetActive(false);
            }
        }

        ClearOrder();
        orderTypeText.text = orderType + " \uC8FC\uBB38";
        menuRoot.SetActive(true);
    }

    private void ShowStartScreen()
    {
        menuRoot.SetActive(false);
        adminLoginRoot.SetActive(false);
        adminRoot.SetActive(false);
        adminClickCount = 0;

        foreach (GameObject root in startScreenRoots)
        {
            if (root != null)
            {
                root.SetActive(true);
            }
        }
    }

    private void CreateMenuRoot()
    {
        menuRoot = CreateUiObject("DrinkMenu", canvas.transform);
        Stretch(menuRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image background = menuRoot.AddComponent<Image>();
        background.color = new Color32(247, 248, 244, 255);
        background.raycastTarget = true;
    }

    private void CreateHeader()
    {
        GameObject header = CreateUiObject("Header", menuRoot.transform);
        RectTransform rect = header.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 76);

        Image image = header.AddComponent<Image>();
        image.color = new Color32(24, 24, 24, 255);

        Text title = CreateText("Title", header.transform, "\uBA54\uB274 \uC120\uD0DD", 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        Stretch(title.GetComponent<RectTransform>(), new Vector2(30, 0), new Vector2(-240, 0));

        orderTypeText = CreateText("OrderType", header.transform, "\uB9E4\uC7A5 \uC8FC\uBB38", 22, FontStyle.Bold, TextAnchor.MiddleRight, new Color32(255, 226, 121, 255));
        Stretch(orderTypeText.GetComponent<RectTransform>(), new Vector2(460, 0), new Vector2(-30, 0));
    }

    private void CreateCategoryTabs()
    {
        GameObject tabs = CreateUiObject("CategoryTabs", menuRoot.transform);
        RectTransform rect = tabs.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -84);
        rect.sizeDelta = new Vector2(0, 58);

        HorizontalLayoutGroup layout = tabs.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 0, 0);
        layout.spacing = 8;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        foreach (string category in categories)
        {
            GameObject buttonObject = CreateUiObject(category + "Tab", tabs.transform);
            Image image = buttonObject.AddComponent<Image>();
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectCategory(category));

            Text text = CreateText("Text", buttonObject.transform, category, 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            categoryButtons[category] = button;
        }

        UpdateCategoryButtons();
    }

    private void CreateDrinkGrid()
    {
        gridRoot = CreateUiObject("MenuGrid", menuRoot.transform);
        Stretch(gridRoot.GetComponent<RectTransform>(), new Vector2(30, 132), new Vector2(-30, -138));

        GridLayoutGroup layout = gridRoot.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(226, 128);
        layout.spacing = new Vector2(18, 14);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.padding = new RectOffset(0, 0, 8, 8);

        RebuildGrid();
    }

    private void CreateFooter()
    {
        GameObject footer = CreateUiObject("Footer", menuRoot.transform);
        RectTransform rect = footer.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 126);

        Image footerImage = footer.AddComponent<Image>();
        footerImage.color = new Color32(32, 32, 32, 255);

        GameObject backButton = CreateFooterButton("BackButton", footer.transform, "\uCC98\uC74C\uC73C\uB85C", new Vector2(28, 26), new Vector2(128, 56));
        backButton.GetComponent<Button>().onClick.AddListener(ShowStartScreen);

        selectedItemsText = CreateText("SelectedItems", footer.transform, "\uC120\uD0DD\uB41C \uBA54\uB274\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.", 18, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        RectTransform selectedRect = selectedItemsText.GetComponent<RectTransform>();
        selectedRect.anchorMin = new Vector2(0, 0);
        selectedRect.anchorMax = new Vector2(1, 1);
        selectedRect.offsetMin = new Vector2(178, 12);
        selectedRect.offsetMax = new Vector2(-220, -12);
        selectedItemsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        selectedItemsText.verticalOverflow = VerticalWrapMode.Truncate;

        totalText = CreateText("Total", footer.transform, "\uCD1D\uD569\uACC4 0\uC6D0", 24, FontStyle.Bold, TextAnchor.MiddleRight, new Color32(255, 226, 121, 255));
        RectTransform totalRect = totalText.GetComponent<RectTransform>();
        totalRect.anchorMin = new Vector2(1, 0);
        totalRect.anchorMax = new Vector2(1, 1);
        totalRect.pivot = new Vector2(1, 0.5f);
        totalRect.anchoredPosition = new Vector2(-30, 18);
        totalRect.sizeDelta = new Vector2(190, 52);

        GameObject payButton = CreateFooterButton("PayButton", footer.transform, "\uACB0\uC81C", new Vector2(-176, 24), new Vector2(146, 48));
        RectTransform payRect = payButton.GetComponent<RectTransform>();
        payRect.anchorMin = new Vector2(1, 0);
        payRect.anchorMax = new Vector2(1, 0);
        payRect.pivot = new Vector2(1, 0);
        payButton.GetComponent<Button>().onClick.AddListener(OnPayClicked);
    }

    private void CreateAdminScreen()
    {
        adminRoot = CreateUiObject("AdminScreen", canvas.transform);
        Stretch(adminRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image background = adminRoot.AddComponent<Image>();
        background.color = new Color32(245, 245, 245, 255);
        background.raycastTarget = true;

        GameObject header = CreateUiObject("Header", adminRoot.transform);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = new Vector2(0, 82);

        Image headerImage = header.AddComponent<Image>();
        headerImage.color = new Color32(24, 24, 24, 255);

        Text title = CreateText("Title", header.transform, "\uAD00\uB9AC\uC790 \uD654\uBA74", 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        Stretch(title.GetComponent<RectTransform>(), new Vector2(30, 0), new Vector2(-200, 0));

        GameObject closeButton = CreateFooterButton("CloseButton", header.transform, "\uB098\uAC00\uAE30", new Vector2(-30, 14), new Vector2(120, 54));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 0);
        closeRect.anchorMax = new Vector2(1, 0);
        closeRect.pivot = new Vector2(1, 0);
        closeButton.GetComponent<Button>().onClick.AddListener(ShowStartScreen);

        GameObject salesTab = CreateFooterButton("SalesTab", adminRoot.transform, "\uB9E4\uCD9C \uC870\uD68C", new Vector2(40, -150), new Vector2(160, 50));
        RectTransform salesTabRect = salesTab.GetComponent<RectTransform>();
        salesTabRect.anchorMin = new Vector2(0, 1);
        salesTabRect.anchorMax = new Vector2(0, 1);
        salesTabRect.pivot = new Vector2(0, 1);
        salesTab.GetComponent<Button>().onClick.AddListener(ShowAdminSalesView);

        GameObject menuTab = CreateFooterButton("MenuTab", adminRoot.transform, "\uBA54\uB274 \uC0C1\uD0DC \uAD00\uB9AC", new Vector2(218, -150), new Vector2(190, 50));
        RectTransform menuTabRect = menuTab.GetComponent<RectTransform>();
        menuTabRect.anchorMin = new Vector2(0, 1);
        menuTabRect.anchorMax = new Vector2(0, 1);
        menuTabRect.pivot = new Vector2(0, 1);
        menuTab.GetComponent<Button>().onClick.AddListener(ShowAdminMenuView);

        CreateAdminSalesView();
        CreateAdminMenuView();
        ShowAdminSalesView();
    }

    private void CreateAdminSalesView()
    {
        adminSalesView = CreateUiObject("AdminSalesView", adminRoot.transform);
        Stretch(adminSalesView.GetComponent<RectTransform>(), new Vector2(40, 40), new Vector2(-40, -230));

        GameObject salesPanel = CreateUiObject("SalesPanel", adminSalesView.transform);
        RectTransform salesRect = salesPanel.GetComponent<RectTransform>();
        salesRect.anchorMin = new Vector2(0, 1);
        salesRect.anchorMax = new Vector2(1, 1);
        salesRect.pivot = new Vector2(0.5f, 1);
        salesRect.anchoredPosition = Vector2.zero;
        salesRect.sizeDelta = new Vector2(-60, 82);

        Image salesImage = salesPanel.AddComponent<Image>();
        salesImage.color = Color.white;
        salesImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        salesImage.type = Image.Type.Sliced;

        salesSummaryText = CreateText("SalesSummary", salesPanel.transform, "", 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        Stretch(salesSummaryText.GetComponent<RectTransform>(), new Vector2(22, 0), new Vector2(-22, 0));
        UpdateSalesSummary();

        Text graphTitle = CreateText("GraphTitle", adminSalesView.transform, "\uCD5C\uADFC \uC8FC\uBB38 \uB9E4\uCD9C \uADF8\uB798\uD504", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        RectTransform graphTitleRect = graphTitle.GetComponent<RectTransform>();
        graphTitleRect.anchorMin = new Vector2(0, 1);
        graphTitleRect.anchorMax = new Vector2(1, 1);
        graphTitleRect.pivot = new Vector2(0.5f, 1);
        graphTitleRect.anchoredPosition = new Vector2(0, -112);
        graphTitleRect.sizeDelta = new Vector2(-60, 34);

        salesGraphRoot = CreateUiObject("SalesGraph", adminSalesView.transform);
        RectTransform graphRect = salesGraphRoot.GetComponent<RectTransform>();
        graphRect.anchorMin = new Vector2(0, 0);
        graphRect.anchorMax = new Vector2(1, 1);
        graphRect.offsetMin = new Vector2(30, 30);
        graphRect.offsetMax = new Vector2(-30, -160);

        Image graphBackground = salesGraphRoot.AddComponent<Image>();
        graphBackground.color = Color.white;
        graphBackground.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        graphBackground.type = Image.Type.Sliced;
        UpdateSalesGraph();
    }

    private void CreateAdminMenuView()
    {
        adminMenuView = CreateUiObject("AdminMenuView", adminRoot.transform);
        Stretch(adminMenuView.GetComponent<RectTransform>(), new Vector2(40, 40), new Vector2(-40, -230));

        Text info = CreateText("Info", adminMenuView.transform, "\uBA54\uB274 \uD310\uB9E4 \uC0C1\uD0DC\uB97C \uAD00\uB9AC\uD569\uB2C8\uB2E4.", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        RectTransform infoRect = info.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 1);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.pivot = new Vector2(0.5f, 1);
        infoRect.anchoredPosition = Vector2.zero;
        infoRect.sizeDelta = new Vector2(-60, 36);

        GameObject list = CreateUiObject("SoldOutList", adminMenuView.transform);
        RectTransform listRect = list.GetComponent<RectTransform>();
        Stretch(listRect, new Vector2(0, 56), new Vector2(0, 0));

        GridLayoutGroup layout = list.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(250, 104);
        layout.spacing = new Vector2(18, 12);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.UpperCenter;

        for (int i = 0; i < items.Length; i++)
        {
            CreateAdminMenuStatusCard(list.transform, i);
        }
    }

    private void ShowAdminSalesView()
    {
        adminSalesView.SetActive(true);
        adminMenuView.SetActive(false);
        UpdateSalesSummary();
        UpdateSalesGraph();
    }

    private void ShowAdminMenuView()
    {
        adminSalesView.SetActive(false);
        adminMenuView.SetActive(true);
    }

    private void CreateAdminLoginScreen()
    {
        adminLoginRoot = CreateUiObject("AdminLogin", canvas.transform);
        Stretch(adminLoginRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image background = adminLoginRoot.AddComponent<Image>();
        background.color = new Color32(245, 245, 245, 255);
        background.raycastTarget = true;

        GameObject dialog = CreateUiObject("LoginPanel", adminLoginRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(420, 300);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        Text title = CreateText("Title", dialog.transform, "\uAD00\uB9AC\uC790 \uB85C\uADF8\uC778", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(24, 24, 24, 255));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -28);
        titleRect.sizeDelta = new Vector2(-40, 42);

        Text label = CreateText("PasswordLabel", dialog.transform, "\uBE44\uBC00\uBC88\uD638", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 1);
        labelRect.anchoredPosition = new Vector2(0, -94);
        labelRect.sizeDelta = new Vector2(-70, 28);

        adminPasswordInput = CreateInputField("PasswordInput", dialog.transform);
        RectTransform inputRect = adminPasswordInput.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 1);
        inputRect.anchorMax = new Vector2(1, 1);
        inputRect.pivot = new Vector2(0.5f, 1);
        inputRect.anchoredPosition = new Vector2(0, -128);
        inputRect.sizeDelta = new Vector2(-70, 46);

        adminLoginErrorText = CreateText("Error", dialog.transform, "", 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(190, 45, 45, 255));
        RectTransform errorRect = adminLoginErrorText.GetComponent<RectTransform>();
        errorRect.anchorMin = new Vector2(0, 1);
        errorRect.anchorMax = new Vector2(1, 1);
        errorRect.pivot = new Vector2(0.5f, 1);
        errorRect.anchoredPosition = new Vector2(0, -180);
        errorRect.sizeDelta = new Vector2(-40, 28);

        GameObject cancelButton = CreateDialogButton("CancelButton", dialog.transform, "\uCDE8\uC18C", new Vector2(-100, 26), new Color32(92, 92, 92, 255));
        cancelButton.GetComponent<Button>().onClick.AddListener(ShowStartScreen);

        GameObject loginButton = CreateDialogButton("LoginButton", dialog.transform, "\uB85C\uADF8\uC778", new Vector2(100, 26), new Color32(235, 143, 42, 255));
        loginButton.GetComponent<Button>().onClick.AddListener(SubmitAdminLogin);

        CreateAdminLoginAlert();
    }

    private InputField CreateInputField(string name, Transform parent)
    {
        GameObject inputObject = CreateUiObject(name, parent);

        Image image = inputObject.AddComponent<Image>();
        image.color = new Color32(244, 244, 244, 255);

        InputField input = inputObject.AddComponent<InputField>();
        input.targetGraphic = image;
        input.contentType = InputField.ContentType.Password;
        input.characterLimit = 12;

        Text text = CreateText("Text", inputObject.transform, "", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect, new Vector2(14, 0), new Vector2(-14, 0));
        text.supportRichText = false;

        Text placeholder = CreateText("Placeholder", inputObject.transform, "\uBE44\uBC00\uBC88\uD638 \uC785\uB825", 18, FontStyle.Normal, TextAnchor.MiddleLeft, new Color32(150, 150, 150, 255));
        Stretch(placeholder.GetComponent<RectTransform>(), new Vector2(14, 0), new Vector2(-14, 0));

        input.textComponent = text;
        input.placeholder = placeholder;
        input.onEndEdit.AddListener(value =>
        {
            if (IsSubmitKeyPressed())
            {
                SubmitAdminLogin();
            }
        });

        return input;
    }

    private bool IsSubmitKeyPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame);
    }

    private void CreateAdminLoginAlert()
    {
        adminLoginAlertRoot = CreateUiObject("AdminLoginAlert", adminLoginRoot.transform);
        Stretch(adminLoginAlertRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image dim = adminLoginAlertRoot.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 145);
        dim.raycastTarget = true;

        GameObject dialog = CreateUiObject("Dialog", adminLoginAlertRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(380, 210);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        Text title = CreateText("Title", dialog.transform, "\uB85C\uADF8\uC778 \uC2E4\uD328", 25, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(24, 24, 24, 255));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -24);
        titleRect.sizeDelta = new Vector2(-40, 40);

        Text message = CreateText("Message", dialog.transform, "\uBE44\uBC00\uBC88\uD638\uAC00 \uC77C\uCE58\uD558\uC9C0 \uC54A\uC2B5\uB2C8\uB2E4.", 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(1, 1);
        messageRect.pivot = new Vector2(0.5f, 1);
        messageRect.anchoredPosition = new Vector2(0, -80);
        messageRect.sizeDelta = new Vector2(-40, 54);

        GameObject okButton = CreateDialogButton("OkButton", dialog.transform, "\uD655\uC778", new Vector2(0, 24), new Color32(235, 143, 42, 255));
        okButton.GetComponent<Button>().onClick.AddListener(CloseAdminLoginAlert);

        adminLoginAlertRoot.SetActive(false);
    }

    private void CreateAdminMenuStatusCard(Transform parent, int itemIndex)
    {
        MenuItem item = items[itemIndex];
        GameObject card = CreateUiObject(item.Name + "Status", parent);
        Image cardImage = card.AddComponent<Image>();
        cardImage.color = Color.white;
        cardImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        cardImage.type = Image.Type.Sliced;
        card.AddComponent<RectMask2D>();

        Text name = CreateText("Name", card.transform, item.Name, 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -8);
        nameRect.sizeDelta = new Vector2(-50, 28);
        name.horizontalOverflow = HorizontalWrapMode.Wrap;
        name.verticalOverflow = VerticalWrapMode.Truncate;

        Text status = CreateText("Status", card.transform, "", 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(45, 45, 45, 255));
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.5f);
        statusRect.anchorMax = new Vector2(1, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0, -4);
        statusRect.sizeDelta = new Vector2(-24, 24);
        status.horizontalOverflow = HorizontalWrapMode.Wrap;
        adminStatusTexts[itemIndex] = status;

        GameObject toggleButton = CreateUiObject("ToggleSoldOut", card.transform);
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1, 0);
        toggleRect.anchorMax = new Vector2(1, 0);
        toggleRect.pivot = new Vector2(0.5f, 0);
        toggleRect.anchoredPosition = new Vector2(-125, 10);
        toggleRect.sizeDelta = new Vector2(90, 30);

        Image buttonImage = toggleButton.AddComponent<Image>();
        buttonImage.color = new Color32(235, 143, 42, 255);

        Button button = toggleButton.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => ToggleSoldOut(itemIndex));

        Text buttonText = CreateText("Text", toggleButton.transform, "\uBCC0\uACBD", 16, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(buttonText.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        UpdateAdminStatusText(itemIndex);
    }

    private void ConnectBackgroundAdminTrigger()
    {
        Transform background = FindTransformByName(canvas.transform, "backgraund");
        if (background == null)
        {
            return;
        }

        Image image = background.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }

        Button button = background.GetComponent<Button>();
        if (button == null)
        {
            button = background.gameObject.AddComponent<Button>();
        }

        button.onClick.AddListener(OnMainBackgroundClicked);
    }

    private void OnMainBackgroundClicked()
    {
        if (menuRoot.activeSelf || adminLoginRoot.activeSelf || adminRoot.activeSelf)
        {
            return;
        }

        adminClickCount++;
        if (adminClickCount < 4)
        {
            return;
        }

        adminClickCount = 0;
        ShowAdminLoginScreen();
    }

    private void ShowAdminLoginScreen()
    {
        menuRoot.SetActive(false);
        adminRoot.SetActive(false);
        adminPasswordInput.text = "";
        adminLoginErrorText.text = "";
        adminLoginAlertRoot.SetActive(false);

        foreach (GameObject root in startScreenRoots)
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        adminLoginRoot.SetActive(true);
        adminPasswordInput.ActivateInputField();
    }

    private void SubmitAdminLogin()
    {
        string password = adminPasswordInput.text;
        if (string.IsNullOrEmpty(password))
        {
            adminPasswordInput.ActivateInputField();
            return;
        }

        if (password == AdminPassword)
        {
            adminLoginRoot.SetActive(false);
            ShowAdminScreen();
            return;
        }

        adminLoginErrorText.text = "";
        adminPasswordInput.text = "";
        adminLoginAlertRoot.SetActive(true);
    }

    private void CloseAdminLoginAlert()
    {
        adminLoginAlertRoot.SetActive(false);
        adminPasswordInput.ActivateInputField();
    }

    private void ShowAdminScreen()
    {
        menuRoot.SetActive(false);
        adminLoginRoot.SetActive(false);

        foreach (GameObject root in startScreenRoots)
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        adminRoot.SetActive(true);
    }

    private void ToggleSoldOut(int itemIndex)
    {
        soldOutItems[itemIndex] = !soldOutItems[itemIndex];
        UpdateAdminStatusText(itemIndex);
        RebuildGrid();
    }

    private void UpdateAdminStatusText(int itemIndex)
    {
        if (!adminStatusTexts.TryGetValue(itemIndex, out Text statusText))
        {
            return;
        }

        statusText.text = soldOutItems[itemIndex] ? "\uC0C1\uD0DC: \uB9E4\uC9C4" : "\uC0C1\uD0DC: \uD310\uB9E4\uC911";
        statusText.color = soldOutItems[itemIndex] ? new Color32(190, 45, 45, 255) : new Color32(35, 128, 74, 255);
    }

    private bool IsSoldOut(int itemIndex)
    {
        return soldOutItems != null && itemIndex >= 0 && itemIndex < soldOutItems.Length && soldOutItems[itemIndex];
    }

    private void CreateAlert()
    {
        alertRoot = CreateUiObject("Alert", menuRoot.transform);
        Stretch(alertRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image dim = alertRoot.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 145);
        dim.raycastTarget = true;

        GameObject dialog = CreateUiObject("Dialog", alertRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(360, 190);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        Text message = CreateText("Message", dialog.transform, "\uC120\uD0DD\uD558\uC2E0 \uBA54\uB274\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(1, 1);
        messageRect.pivot = new Vector2(0.5f, 1);
        messageRect.anchoredPosition = new Vector2(0, -32);
        messageRect.sizeDelta = new Vector2(-32, 70);

        GameObject okButton = CreateUiObject("OkButton", dialog.transform);
        RectTransform okRect = okButton.GetComponent<RectTransform>();
        okRect.anchorMin = new Vector2(0.5f, 0);
        okRect.anchorMax = new Vector2(0.5f, 0);
        okRect.pivot = new Vector2(0.5f, 0);
        okRect.anchoredPosition = new Vector2(0, 24);
        okRect.sizeDelta = new Vector2(132, 46);

        Image okImage = okButton.AddComponent<Image>();
        okImage.color = new Color32(235, 143, 42, 255);

        Button ok = okButton.AddComponent<Button>();
        ok.targetGraphic = okImage;
        ok.onClick.AddListener(() => alertRoot.SetActive(false));

        Text okText = CreateText("Text", okButton.transform, "\uD655\uC778", 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(okText.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        alertRoot.SetActive(false);
    }

    private void OnPayClicked()
    {
        if (orderLines.Count == 0)
        {
            alertRoot.SetActive(true);
            return;
        }

        UpdateConfirmModal();
        confirmRoot.SetActive(true);
    }

    private void CreateConfirmModal()
    {
        confirmRoot = CreateUiObject("ConfirmOrder", menuRoot.transform);
        Stretch(confirmRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image dim = confirmRoot.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 150);
        dim.raycastTarget = true;

        GameObject dialog = CreateUiObject("Dialog", confirmRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(480, 420);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        Text title = CreateText("Title", dialog.transform, "\uC8FC\uBB38 \uCD5C\uC885\uD655\uC778", 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(24, 24, 24, 255));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -24);
        titleRect.sizeDelta = new Vector2(-40, 42);

        confirmItemsText = CreateText("Items", dialog.transform, "", 19, FontStyle.Normal, TextAnchor.UpperLeft, new Color32(45, 45, 45, 255));
        RectTransform itemsRect = confirmItemsText.GetComponent<RectTransform>();
        itemsRect.anchorMin = new Vector2(0, 0);
        itemsRect.anchorMax = new Vector2(1, 1);
        itemsRect.offsetMin = new Vector2(34, 126);
        itemsRect.offsetMax = new Vector2(-34, -86);
        confirmItemsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        confirmItemsText.verticalOverflow = VerticalWrapMode.Truncate;

        confirmTotalText = CreateText("Total", dialog.transform, "\uCD1D\uD569\uACC4 0\uC6D0", 24, FontStyle.Bold, TextAnchor.MiddleRight, new Color32(235, 143, 42, 255));
        RectTransform totalRect = confirmTotalText.GetComponent<RectTransform>();
        totalRect.anchorMin = new Vector2(0, 0);
        totalRect.anchorMax = new Vector2(1, 0);
        totalRect.pivot = new Vector2(0.5f, 0);
        totalRect.anchoredPosition = new Vector2(0, 78);
        totalRect.sizeDelta = new Vector2(-40, 38);

        GameObject cancelButton = CreateDialogButton("CancelButton", dialog.transform, "\uCDE8\uC18C", new Vector2(-110, 24), new Color32(92, 92, 92, 255));
        cancelButton.GetComponent<Button>().onClick.AddListener(() => confirmRoot.SetActive(false));

        GameObject confirmButton = CreateDialogButton("ConfirmButton", dialog.transform, "\uD655\uC778", new Vector2(110, 24), new Color32(235, 143, 42, 255));
        confirmButton.GetComponent<Button>().onClick.AddListener(ShowPaymentModal);

        confirmRoot.SetActive(false);
    }

    private void CreatePaymentModal()
    {
        paymentRoot = CreateUiObject("Payment", menuRoot.transform);
        Stretch(paymentRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image dim = paymentRoot.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 150);
        dim.raycastTarget = true;

        GameObject dialog = CreateUiObject("Dialog", paymentRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(430, 330);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        Text title = CreateText("Title", dialog.transform, "\uACB0\uC81C", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(24, 24, 24, 255));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -24);
        titleRect.sizeDelta = new Vector2(-40, 42);

        Text message = CreateText("Message", dialog.transform, "\uCE74\uB4DC\uB97C \uC0BD\uC785\uD558\uAC70\uB098\n\uD130\uCE58\uD574 \uACB0\uC81C\uD574\uC8FC\uC138\uC694", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(1, 1);
        messageRect.pivot = new Vector2(0.5f, 1);
        messageRect.anchoredPosition = new Vector2(0, -86);
        messageRect.sizeDelta = new Vector2(-40, 86);

        paymentTotalText = CreateText("Total", dialog.transform, "\uCD1D\uD569\uACC4 0\uC6D0", 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(235, 143, 42, 255));
        RectTransform totalRect = paymentTotalText.GetComponent<RectTransform>();
        totalRect.anchorMin = new Vector2(0, 0);
        totalRect.anchorMax = new Vector2(1, 0);
        totalRect.pivot = new Vector2(0.5f, 0);
        totalRect.anchoredPosition = new Vector2(0, 92);
        totalRect.sizeDelta = new Vector2(-40, 42);

        GameObject cancelButton = CreateDialogButton("CancelButton", dialog.transform, "\uCDE8\uC18C", new Vector2(-100, 24), new Color32(92, 92, 92, 255));
        cancelButton.GetComponent<Button>().onClick.AddListener(() => paymentRoot.SetActive(false));

        GameObject completeButton = CreateDialogButton("CompleteButton", dialog.transform, "\uACB0\uC81C\uC644\uB8CC", new Vector2(100, 24), new Color32(235, 143, 42, 255));
        completeButton.GetComponent<Button>().onClick.AddListener(CompletePayment);

        paymentRoot.SetActive(false);
    }

    private void ShowPaymentModal()
    {
        confirmRoot.SetActive(false);
        paymentTotalText.text = "\uCD1D\uD569\uACC4 " + FormatWon(GetOrderTotal());
        paymentRoot.SetActive(true);
    }

    private void CompletePayment()
    {
        RecordSale();
        paymentRoot.SetActive(false);
        receiptRoot.SetActive(true);
    }

    private void RecordSale()
    {
        lastOrderAmount = GetOrderTotal();
        totalSalesAmount += lastOrderAmount;
        totalOrderCount++;
        totalSoldQuantity += orderLines.Count;
        recentSaleAmounts.Add(lastOrderAmount);
        if (recentSaleAmounts.Count > 8)
        {
            recentSaleAmounts.RemoveAt(0);
        }

        UpdateSalesSummary();
        UpdateSalesGraph();
    }

    private void UpdateSalesSummary()
    {
        if (salesSummaryText == null)
        {
            return;
        }

        salesSummaryText.text = "\uB9E4\uCD9C \uC870\uD68C"
            + "\n\uCD1D \uB9E4\uCD9C: " + FormatWon(totalSalesAmount)
            + "    \uC8FC\uBB38 \uAC74\uC218: " + totalOrderCount
            + "    \uD310\uB9E4 \uC218\uB7C9: " + totalSoldQuantity
            + "    \uCD5C\uADFC \uC8FC\uBB38: " + FormatWon(lastOrderAmount);
    }

    private void UpdateSalesGraph()
    {
        if (salesGraphRoot == null)
        {
            return;
        }

        for (int i = salesGraphRoot.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(salesGraphRoot.transform.GetChild(i).gameObject);
        }

        int maxAmount = 1;
        foreach (int amount in recentSaleAmounts)
        {
            maxAmount = Mathf.Max(maxAmount, amount);
        }

        if (recentSaleAmounts.Count == 0)
        {
            Text empty = CreateText("Empty", salesGraphRoot.transform, "\uC544\uC9C1 \uB9E4\uCD9C \uB370\uC774\uD130\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(90, 90, 90, 255));
            Stretch(empty.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            return;
        }

        int count = recentSaleAmounts.Count;
        float barAreaWidth = 620f;
        float barWidth = Mathf.Min(58f, barAreaWidth / count - 12f);
        float startX = -((barWidth + 22f) * (count - 1)) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            int amount = recentSaleAmounts[i];
            float normalized = Mathf.Clamp01((float)amount / maxAmount);
            float barHeight = Mathf.Lerp(24f, 190f, normalized);

            GameObject bar = CreateUiObject("Bar", salesGraphRoot.transform);
            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0);
            barRect.anchorMax = new Vector2(0.5f, 0);
            barRect.pivot = new Vector2(0.5f, 0);
            barRect.anchoredPosition = new Vector2(startX + i * (barWidth + 22f), 48);
            barRect.sizeDelta = new Vector2(barWidth, barHeight);

            Image barImage = bar.AddComponent<Image>();
            barImage.color = new Color32(235, 143, 42, 255);

            Text label = CreateText("Label", salesGraphRoot.transform, FormatWon(amount), 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0);
            labelRect.anchorMax = new Vector2(0.5f, 0);
            labelRect.pivot = new Vector2(0.5f, 0);
            labelRect.anchoredPosition = new Vector2(barRect.anchoredPosition.x, 16);
            labelRect.sizeDelta = new Vector2(94, 24);
        }
    }

    private void CreateReceiptModal()
    {
        receiptRoot = CreateUiObject("Receipt", menuRoot.transform);
        Stretch(receiptRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image dim = receiptRoot.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 150);
        dim.raycastTarget = true;

        GameObject dialog = CreateUiObject("Dialog", receiptRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(430, 260);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        Text title = CreateText("Title", dialog.transform, "\uC601\uC218\uC99D \uBC1C\uAE09", 27, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(24, 24, 24, 255));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(-40, 42);

        Text message = CreateText("Message", dialog.transform, "\uC601\uC218\uC99D\uC744 \uBC1C\uAE09\uBC1B\uC73C\uC2DC\uACA0\uC2B5\uB2C8\uAE4C?", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(1, 1);
        messageRect.pivot = new Vector2(0.5f, 1);
        messageRect.anchoredPosition = new Vector2(0, -92);
        messageRect.sizeDelta = new Vector2(-40, 54);

        GameObject noButton = CreateDialogButton("NoReceiptButton", dialog.transform, "\uC548 \uBC1B\uAE30", new Vector2(-100, 28), new Color32(92, 92, 92, 255));
        noButton.GetComponent<Button>().onClick.AddListener(FinishOrder);

        GameObject yesButton = CreateDialogButton("ReceiptButton", dialog.transform, "\uBC1B\uAE30", new Vector2(100, 28), new Color32(235, 143, 42, 255));
        yesButton.GetComponent<Button>().onClick.AddListener(FinishOrder);

        receiptRoot.SetActive(false);
    }

    private void FinishOrder()
    {
        receiptRoot.SetActive(false);
        ClearOrder();
        ShowStartScreen();
    }

    private GameObject CreateDialogButton(string name, Transform parent, string label, Vector2 anchoredPosition, Color32 color)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(150, 48);

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText("Text", buttonObject.transform, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        return buttonObject;
    }

    private void UpdateConfirmModal()
    {
        List<string> selectedLines = new List<string>();

        foreach (OrderLine line in orderLines)
        {
            selectedLines.Add(line.DisplayName + "  " + FormatWon(line.Price));
        }

        confirmItemsText.text = string.Join("\n", selectedLines.ToArray());
        confirmTotalText.text = "\uCD1D\uD569\uACC4 " + FormatWon(GetOrderTotal());
    }

    private void CreateDetailModal()
    {
        detailRoot = CreateUiObject("MenuDetail", menuRoot.transform);
        Stretch(detailRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        Image dim = detailRoot.AddComponent<Image>();
        dim.color = new Color32(0, 0, 0, 145);
        dim.raycastTarget = true;

        GameObject dialog = CreateUiObject("Dialog", detailRoot.transform);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(500, 500);

        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = Color.white;
        dialogImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        dialogImage.type = Image.Type.Sliced;

        detailTitleText = CreateText("Title", dialog.transform, "", 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(24, 24, 24, 255));
        RectTransform titleRect = detailTitleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(-40, 36);

        detailPriceText = CreateText("Price", dialog.transform, "", 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(235, 143, 42, 255));
        RectTransform priceRect = detailPriceText.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0, 1);
        priceRect.anchorMax = new Vector2(1, 1);
        priceRect.pivot = new Vector2(0.5f, 1);
        priceRect.anchoredPosition = new Vector2(0, -58);
        priceRect.sizeDelta = new Vector2(-40, 30);

        temperatureGroup = CreateOptionGroup(dialog.transform, "\uC628\uB3C4", 110);
        temperatureButtons.Add(CreateOptionButton(temperatureGroup.transform, "\uC544\uC774\uC2A4", 0, () => SetTemperature("\uC544\uC774\uC2A4")));
        temperatureButtons.Add(CreateOptionButton(temperatureGroup.transform, "\uD56B", 1, () => SetTemperature("\uD56B")));

        GameObject sizeGroup = CreateOptionGroup(dialog.transform, "\uC0AC\uC774\uC988", 198);
        sizeButtons.Add(CreateOptionButton(sizeGroup.transform, "\uB808\uADE4\uB7EC", 0, () => SetLarge(false)));
        sizeButtons.Add(CreateOptionButton(sizeGroup.transform, "\uB77C\uC9C0 +500\uC6D0", 1, () => SetLarge(true)));

        customGroup = CreateOptionGroup(dialog.transform, "\uCEE4\uC2A4\uD140 \uC635\uC158", 286);
        customButtons.Add(CreateOptionButton(customGroup.transform, "\uC0F7", 0, () => ToggleShot()));
        customButtons.Add(CreateOptionButton(customGroup.transform, "\uC0F7 \uCD94\uAC00 +500\uC6D0", 1, () => ToggleExtraShot()));
        customButtons.Add(CreateOptionButton(customGroup.transform, "\uC2DC\uB7FD\uCD94\uAC00 +300\uC6D0", 2, () => ToggleSyrup()));

        GameObject cancelButton = CreateDialogButton("CancelButton", dialog.transform, "\uCDE8\uC18C", new Vector2(-110, 24), new Color32(92, 92, 92, 255));
        cancelButton.GetComponent<Button>().onClick.AddListener(() => detailRoot.SetActive(false));

        GameObject addButton = CreateDialogButton("AddButton", dialog.transform, "\uB2F4\uAE30", new Vector2(110, 24), new Color32(235, 143, 42, 255));
        addButton.GetComponent<Button>().onClick.AddListener(AddDetailSelection);

        detailRoot.SetActive(false);
    }

    private GameObject CreateOptionGroup(Transform parent, string title, float y)
    {
        GameObject group = CreateUiObject(title, parent);
        RectTransform rect = group.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -y);
        rect.sizeDelta = new Vector2(-50, 72);

        Text label = CreateText("Label", group.transform, title, 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 1);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(0, 26);

        return group;
    }

    private Button CreateOptionButton(Transform parent, string label, int column, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUiObject(label, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(column * 146, 0);
        rect.sizeDelta = new Vector2(136, 38);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color32(230, 230, 230, 255);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        Text text = CreateText("Text", buttonObject.transform, label, 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        return button;
    }

    private void ShowDetail(int itemIndex)
    {
        detailItemIndex = itemIndex;
        detailTemperature = "\uC544\uC774\uC2A4";
        detailLarge = false;
        detailShot = false;
        detailExtraShot = false;
        detailSyrup = false;

        MenuItem item = items[itemIndex];
        detailTitleText.text = item.Name;
        detailPriceText.text = FormatWon(item.Price);
        temperatureGroup.SetActive(!item.IsSide);
        customGroup.SetActive(!item.IsSide);
        UpdateDetailOptions();
        detailRoot.SetActive(true);
    }

    private void SetTemperature(string temperature)
    {
        detailTemperature = temperature;
        UpdateDetailOptions();
    }

    private void SetLarge(bool isLarge)
    {
        detailLarge = isLarge;
        UpdateDetailOptions();
    }

    private void ToggleShot()
    {
        detailShot = !detailShot;
        UpdateDetailOptions();
    }

    private void ToggleExtraShot()
    {
        detailExtraShot = !detailExtraShot;
        UpdateDetailOptions();
    }

    private void ToggleSyrup()
    {
        detailSyrup = !detailSyrup;
        UpdateDetailOptions();
    }

    private void UpdateDetailOptions()
    {
        SetButtonSelected(temperatureButtons[0], detailTemperature == "\uC544\uC774\uC2A4");
        SetButtonSelected(temperatureButtons[1], detailTemperature == "\uD56B");
        SetButtonSelected(sizeButtons[0], !detailLarge);
        SetButtonSelected(sizeButtons[1], detailLarge);
        SetButtonSelected(customButtons[0], detailShot);
        SetButtonSelected(customButtons[1], detailExtraShot);
        SetButtonSelected(customButtons[2], detailSyrup);
        detailPriceText.text = FormatWon(items[detailItemIndex].Price + GetDetailOptionPrice());
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        Image image = button.GetComponent<Image>();
        image.color = selected ? new Color32(235, 143, 42, 255) : new Color32(230, 230, 230, 255);
        Text text = button.GetComponentInChildren<Text>(true);
        text.color = selected ? Color.white : new Color32(35, 35, 35, 255);
    }

    private int GetDetailOptionPrice()
    {
        int price = 0;
        if (detailLarge)
        {
            price += 500;
        }

        if (detailExtraShot)
        {
            price += 500;
        }

        if (detailSyrup)
        {
            price += 300;
        }

        return price;
    }

    private void AddDetailSelection()
    {
        MenuItem item = items[detailItemIndex];
        List<string> options = new List<string>();

        if (!item.IsSide)
        {
            options.Add(detailTemperature);
        }

        options.Add(detailLarge ? "\uB77C\uC9C0" : "\uB808\uADE4\uB7EC");

        if (detailShot)
        {
            options.Add("\uC0F7");
        }

        if (detailExtraShot)
        {
            options.Add("\uC0F7\uCD94\uAC00");
        }

        if (detailSyrup)
        {
            options.Add("\uC2DC\uB7FD\uCD94\uAC00");
        }

        AddOrderLine(detailItemIndex, string.Join(", ", options.ToArray()), GetDetailOptionPrice());
        detailRoot.SetActive(false);
    }

    private GameObject CreateFooterButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color32(235, 143, 42, 255);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText("Text", buttonObject.transform, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        return buttonObject;
    }

    private void SelectCategory(string category)
    {
        selectedCategory = category;
        UpdateCategoryButtons();
        RebuildGrid();
    }

    private void UpdateCategoryButtons()
    {
        foreach (KeyValuePair<string, Button> pair in categoryButtons)
        {
            Image image = pair.Value.GetComponent<Image>();
            image.color = pair.Key == selectedCategory ? new Color32(235, 143, 42, 255) : new Color32(67, 67, 67, 255);
        }
    }

    private void RebuildGrid()
    {
        quantityTexts.Clear();

        for (int i = gridRoot.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridRoot.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (ShouldShowItem(items[i]))
            {
                CreateMenuCard(gridRoot.transform, i);
            }
        }
    }

    private bool ShouldShowItem(MenuItem item)
    {
        if (selectedCategory == AllCategory)
        {
            return true;
        }

        if (selectedCategory == RecommendedCategory)
        {
            return item.IsRecommended;
        }

        return item.Category == selectedCategory;
    }

    private void CreateMenuCard(Transform parent, int itemIndex)
    {
        MenuItem item = items[itemIndex];
        bool isSoldOut = IsSoldOut(itemIndex);
        GameObject card = CreateUiObject(item.Name, parent);
        Image cardImage = card.AddComponent<Image>();
        cardImage.color = isSoldOut ? new Color32(215, 215, 215, 255) : Color.white;
        cardImage.sprite = CreateRoundedSprite(new Color32(255, 255, 255, 255));
        cardImage.type = Image.Type.Sliced;

        Button cardButton = card.AddComponent<Button>();
        cardButton.targetGraphic = cardImage;
        cardButton.interactable = !isSoldOut;
        cardButton.onClick.AddListener(() => OnMenuCardClicked(itemIndex));

        Image itemImage = CreateImage("ItemImage", card.transform, CreateMenuSprite(item));
        itemImage.color = isSoldOut ? new Color32(150, 150, 150, 150) : Color.white;
        RectTransform itemRect = itemImage.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(0, 0.5f);
        itemRect.pivot = new Vector2(0, 0.5f);
        itemRect.anchoredPosition = new Vector2(12, 13);
        itemRect.sizeDelta = new Vector2(80, 80);

        Text name = CreateText("Name", card.transform, item.Name, 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(35, 35, 35, 255));
        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(44, -12);
        nameRect.sizeDelta = new Vector2(-104, 28);

        Text price = CreateText("Price", card.transform, FormatWon(item.Price), 16, FontStyle.Normal, TextAnchor.MiddleLeft, new Color32(102, 74, 42, 255));
        RectTransform priceRect = price.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0, 1);
        priceRect.anchorMax = new Vector2(1, 1);
        priceRect.pivot = new Vector2(0.5f, 1);
        priceRect.anchoredPosition = new Vector2(44, -40);
        priceRect.sizeDelta = new Vector2(-104, 24);

        GameObject minus = CreateSmallButton("Minus", card.transform, "-", new Vector2(102, 14), itemIndex, -1);
        GameObject plus = CreateSmallButton("Plus", card.transform, "+", new Vector2(176, 14), itemIndex, 1);
        minus.GetComponent<Image>().color = new Color32(92, 92, 92, 255);
        plus.GetComponent<Image>().color = new Color32(235, 143, 42, 255);
        plus.GetComponent<Button>().interactable = !isSoldOut;

        Text quantity = CreateText("Quantity", card.transform, GetQuantity(itemIndex).ToString(), 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(35, 35, 35, 255));
        RectTransform quantityRect = quantity.GetComponent<RectTransform>();
        quantityRect.anchorMin = new Vector2(0, 0);
        quantityRect.anchorMax = new Vector2(0, 0);
        quantityRect.pivot = new Vector2(0, 0);
        quantityRect.anchoredPosition = new Vector2(140, 14);
        quantityRect.sizeDelta = new Vector2(34, 34);
        quantityTexts[itemIndex] = quantity;

        if (isSoldOut)
        {
            Text soldOut = CreateText("SoldOut", card.transform, "\uB9E4\uC9C4", 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(190, 45, 45, 255));
            RectTransform soldOutRect = soldOut.GetComponent<RectTransform>();
            soldOutRect.anchorMin = new Vector2(0, 0);
            soldOutRect.anchorMax = new Vector2(1, 1);
            soldOutRect.offsetMin = Vector2.zero;
            soldOutRect.offsetMax = Vector2.zero;
        }
    }

    private GameObject CreateSmallButton(string name, Transform parent, string label, Vector2 anchoredPosition, int itemIndex, int delta)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(34, 34);

        Image image = buttonObject.AddComponent<Image>();
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => OnQuantityButtonClicked(itemIndex, delta));

        Text text = CreateText("Text", buttonObject.transform, label, 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        return buttonObject;
    }

    private void OnMenuCardClicked(int itemIndex)
    {
        if (IsSoldOut(itemIndex))
        {
            return;
        }

        if (items[itemIndex].Category != CoffeeCategory)
        {
            ChangeQuantity(itemIndex, 1);
            return;
        }

        ShowDetail(itemIndex);
    }

    private void OnQuantityButtonClicked(int itemIndex, int delta)
    {
        if (delta > 0 && IsSoldOut(itemIndex))
        {
            return;
        }

        if (delta > 0 && items[itemIndex].Category == CoffeeCategory)
        {
            ShowDetail(itemIndex);
            return;
        }

        ChangeQuantity(itemIndex, delta);
    }

    private void ChangeQuantity(int itemIndex, int delta)
    {
        if (delta > 0)
        {
            AddOrderLine(itemIndex, "", 0);
        }
        else
        {
            RemoveOrderLine(itemIndex);
        }
    }

    private void AddOrderLine(int itemIndex, string detail, int optionPrice)
    {
        if (IsSoldOut(itemIndex))
        {
            return;
        }

        MenuItem item = items[itemIndex];
        orderLines.Add(new OrderLine(itemIndex, item.Name, detail, item.Price + optionPrice));
        int nextQuantity = GetQuantity(itemIndex) + 1;
        quantities[itemIndex] = nextQuantity;
        if (quantityTexts.TryGetValue(itemIndex, out Text quantityText))
        {
            quantityText.text = nextQuantity.ToString();
        }

        UpdateSummary();
    }

    private void RemoveOrderLine(int itemIndex)
    {
        for (int i = orderLines.Count - 1; i >= 0; i--)
        {
            if (orderLines[i].ItemIndex == itemIndex)
            {
                orderLines.RemoveAt(i);
                break;
            }
        }

        int nextQuantity = Mathf.Max(0, GetQuantity(itemIndex) - 1);
        if (nextQuantity == 0)
        {
            quantities.Remove(itemIndex);
        }
        else
        {
            quantities[itemIndex] = nextQuantity;
        }

        if (quantityTexts.TryGetValue(itemIndex, out Text quantityText))
        {
            quantityText.text = nextQuantity.ToString();
        }

        UpdateSummary();
    }

    private int GetQuantity(int itemIndex)
    {
        return quantities.TryGetValue(itemIndex, out int quantity) ? quantity : 0;
    }

    private void ClearOrder()
    {
        orderLines.Clear();
        quantities.Clear();
        selectedCategory = AllCategory;
        UpdateCategoryButtons();
        if (gridRoot != null)
        {
            RebuildGrid();
        }

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        List<string> selectedLines = new List<string>();

        foreach (OrderLine line in orderLines)
        {
            selectedLines.Add(line.DisplayName + "  " + FormatWon(line.Price));
        }

        selectedItemsText.text = selectedLines.Count == 0 ? "\uC120\uD0DD\uB41C \uBA54\uB274\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4." : string.Join("\n", selectedLines.ToArray());
        totalText.text = "\uCD1D\uD569\uACC4 " + FormatWon(GetOrderTotal());
    }

    private int GetOrderTotal()
    {
        int total = 0;

        foreach (OrderLine line in orderLines)
        {
            total += line.Price;
        }

        return total;
    }

    private GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private Transform FindTransformByName(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindTransformByName(root.GetChild(i), targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private Text CreateText(string name, Transform parent, string value, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        GameObject textObject = CreateUiObject(name, parent);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = uiFont;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        GameObject imageObject = CreateUiObject(name, parent);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Sprite CreateMenuSprite(MenuItem item)
    {
        const int width = 128;
        const int height = 128;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color32 clear = new Color32(0, 0, 0, 0);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        if (item.IsSide)
        {
            DrawSide(texture, item);
        }
        else
        {
            DrawDrink(texture, item);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
    }

    private static void DrawDrink(Texture2D texture, MenuItem item)
    {
        Color32 outline = new Color32(32, 32, 32, 255);
        Color32 ice = new Color32(255, 255, 255, 170);

        FillRoundedRect(texture, 34, 18, 60, 84, 14, outline);
        FillRoundedRect(texture, 38, 22, 52, 76, 12, item.CupColor);
        FillRect(texture, 41, 60, 46, 34, item.ItemColor);
        FillCircle(texture, 52, 82, 5, ice);
        FillCircle(texture, 67, 74, 5, ice);
        FillCircle(texture, 78, 86, 4, ice);
        FillRect(texture, 47, 100, 34, 5, outline);
        FillRect(texture, 54, 106, 20, 8, outline);
        FillCircle(texture, 90, 68, 17, outline);
        FillCircle(texture, 90, 68, 10, new Color32(0, 0, 0, 0));
    }

    private static void DrawSide(Texture2D texture, MenuItem item)
    {
        Color32 outline = new Color32(32, 32, 32, 255);
        FillRoundedRect(texture, 22, 26, 84, 22, 10, outline);
        FillRoundedRect(texture, 28, 30, 72, 14, 7, item.CupColor);
        FillRoundedRect(texture, 36, 44, 56, 42, 8, outline);
        FillRoundedRect(texture, 40, 48, 48, 34, 6, item.ItemColor);
        FillCircle(texture, 52, 62, 5, outline);
        FillCircle(texture, 69, 56, 4, outline);
        FillCircle(texture, 77, 70, 4, outline);
    }

    private Sprite CreateRoundedSprite(Color32 color)
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color32 clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        FillRoundedRect(texture, 0, 0, size, size, 6, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static string FormatWon(int amount)
    {
        return amount.ToString("N0") + "\uC6D0";
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int row = y; row < y + height; row++)
        {
            for (int column = x; column < x + width; column++)
            {
                if (column >= 0 && column < texture.width && row >= 0 && row < texture.height)
                {
                    texture.SetPixel(column, row, color);
                }
            }
        }
    }

    private static void FillRoundedRect(Texture2D texture, int x, int y, int width, int height, int radius, Color color)
    {
        for (int row = y; row < y + height; row++)
        {
            for (int column = x; column < x + width; column++)
            {
                int left = column - x;
                int right = x + width - 1 - column;
                int bottom = row - y;
                int top = y + height - 1 - row;
                int dx = Mathf.Max(radius - Mathf.Min(left, right), 0);
                int dy = Mathf.Max(radius - Mathf.Min(bottom, top), 0);

                if (dx * dx + dy * dy <= radius * radius)
                {
                    texture.SetPixel(column, row, color);
                }
            }
        }
    }

    private static void FillCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        for (int row = centerY - radius; row <= centerY + radius; row++)
        {
            for (int column = centerX - radius; column <= centerX + radius; column++)
            {
                int dx = column - centerX;
                int dy = row - centerY;
                if (dx * dx + dy * dy <= radius * radius && column >= 0 && column < texture.width && row >= 0 && row < texture.height)
                {
                    texture.SetPixel(column, row, color);
                }
            }
        }
    }

    private readonly struct MenuItem
    {
        public readonly string Name;
        public readonly string Category;
        public readonly int Price;
        public readonly Color32 ItemColor;
        public readonly Color32 CupColor;
        public readonly bool IsSide;
        public readonly bool IsRecommended;

        public MenuItem(string name, string category, int price, Color32 itemColor, Color32 cupColor, bool isSide, bool isRecommended)
        {
            Name = name;
            Category = category;
            Price = price;
            ItemColor = itemColor;
            CupColor = cupColor;
            IsSide = isSide;
            IsRecommended = isRecommended;
        }
    }

    private readonly struct OrderLine
    {
        public readonly int ItemIndex;
        public readonly string Name;
        public readonly string Detail;
        public readonly int Price;

        public string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(Detail) ? Name : Name + " (" + Detail + ")";
            }
        }

        public OrderLine(int itemIndex, string name, string detail, int price)
        {
            ItemIndex = itemIndex;
            Name = name;
            Detail = detail;
            Price = price;
        }
    }
}
