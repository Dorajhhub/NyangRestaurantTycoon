using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientShopManager : MonoBehaviour
{
    [Header("References")]
    public GameObject ingredientShopPanel;   // IngredientShopPanel
    public Text titleText;                   // TitleText
    public Button closeButton;               // CloseButton
    public ScrollRect ingredientListScroll;  // IngredientListScroll
    public Transform ingredientListContent;  // Content under ScrollRect
    public GameObject ingredientItemPrefab;  // Prefab with Text_IngredientName, Text_Price (and optional Button)
    public Button buyButton;                 // BuyButton
    public Text playerMoneyText;             // PlayerMoneyText

    [Header("Child Names In Prefab")] 
    public string nameTextChild = "Text_IngredientName";
    public string priceTextChild = "Text_Price";
    public string increaseButtonChild = "IncreaseButton";
    public string decreaseButtonChild = "DecreaseButton";
    public string quantityTextChild = "Text_Quantity";

    private GameManager gameManager;
    private int selectedIndex = -1;
    private readonly Dictionary<int, int> indexToQuantity = new Dictionary<int, int>();
    private readonly Dictionary<int, RowUI> indexToRow = new Dictionary<int, RowUI>();

    void Awake()
    {
        // Auto-bind common references by name to reduce setup errors
        if (ingredientShopPanel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var panelTf = canvas.transform.Find("IngredientShopPanel");
                if (panelTf != null) ingredientShopPanel = panelTf.gameObject;
            }
        }
        if (ingredientShopPanel != null)
        {
            if (titleText == null)
            {
                var tf = ingredientShopPanel.transform.Find("TitleText");
                if (tf != null) titleText = tf.GetComponent<Text>();
            }
            if (closeButton == null)
            {
                var tf = ingredientShopPanel.transform.Find("CloseButton");
                if (tf != null) closeButton = tf.GetComponent<Button>();
            }
            if (ingredientListScroll == null)
            {
                var tf = ingredientShopPanel.transform.Find("IngredientListScroll");
                if (tf != null) ingredientListScroll = tf.GetComponent<ScrollRect>();
            }
            if (ingredientListContent == null && ingredientListScroll != null)
            {
                var contentTf = ingredientListScroll.transform.Find("Viewport/Content");
                if (contentTf != null) ingredientListContent = contentTf;
            }
            if (buyButton == null)
            {
                var tf = ingredientShopPanel.transform.Find("BuyButton");
                if (tf != null) buyButton = tf.GetComponent<Button>();
            }
            if (playerMoneyText == null)
            {
                var tf = ingredientShopPanel.transform.Find("PlayerMoneyText");
                if (tf != null) playerMoneyText = tf.GetComponent<Text>();
            }
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShop);
        }
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClickBuy);
        }
    }

    void Start()
    {
        if (ingredientShopPanel != null)
        {
            ingredientShopPanel.SetActive(false);
        }
    }

    public void OpenShop()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (ingredientShopPanel != null)
        {
            ingredientShopPanel.SetActive(true);
        }
        UIInputBlocker.IsBlocking = true;
        selectedIndex = -1;
        RefreshMoneyUI();
        PopulateIngredientList();
    }

    public void CloseShop()
    {
        if (ingredientShopPanel != null)
        {
            ingredientShopPanel.SetActive(false);
        }
        UIInputBlocker.IsBlocking = false;
    }

    private void RefreshMoneyUI()
    {
        if (playerMoneyText == null || gameManager == null || gameManager.playerStats == null) return;
        playerMoneyText.text = $"보유 금액: {gameManager.playerStats.Money}원";
    }

    private void PopulateIngredientList()
    {
        if (ingredientListContent == null || ingredientItemPrefab == null) return;

        for (int i = ingredientListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(ingredientListContent.GetChild(i).gameObject);
        }

        indexToRow.Clear();
        var db = IngredientDatabase.Instance;
        List<string> names = db.GetAllIngredientNames();
        for (int index = 0; index < names.Count; index++)
        {
            string name = names[index];
            int price = db.GetIngredientPrice(index);

            GameObject rowObj = Instantiate(ingredientItemPrefab, ingredientListContent);
            BindRow(rowObj, index, name, price);
        }
    }

    private void BindRow(GameObject rowObj, int index, string name, int price)
    {
        Text nameText = FindChild<Text>(rowObj.transform, nameTextChild);
        Text priceText = FindChild<Text>(rowObj.transform, priceTextChild);
        Text quantityText = FindChild<Text>(rowObj.transform, quantityTextChild);
        Button incButton = FindChild<Button>(rowObj.transform, increaseButtonChild);
        Button decButton = FindChild<Button>(rowObj.transform, decreaseButtonChild);

        if (nameText != null) nameText.text = name;
        if (priceText != null) priceText.text = $"가격: {price}원";

        if (!indexToQuantity.ContainsKey(index)) indexToQuantity[index] = 0; // 기본 0개
        if (quantityText != null) quantityText.text = indexToQuantity[index].ToString();

        // 선택을 위해 Row 자체에 클릭 연결
        Button rowButton = rowObj.GetComponent<Button>();
        if (rowButton == null) rowButton = rowObj.AddComponent<Button>();
        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(() => OnSelectIndex(rowObj, index));

        // + / - 버튼 동작
        if (incButton != null)
        {
            incButton.onClick.RemoveAllListeners();
            incButton.onClick.AddListener(() => { OnSelectIndex(rowObj, index); AdjustQuantity(index, +1); });
        }
        if (decButton != null)
        {
            decButton.onClick.RemoveAllListeners();
            decButton.onClick.AddListener(() => { OnSelectIndex(rowObj, index); AdjustQuantity(index, -1); });
        }

        indexToRow[index] = new RowUI
        {
            root = rowObj,
            nameText = nameText,
            priceText = priceText,
            quantityText = quantityText,
            incButton = incButton,
            decButton = decButton,
            rowButton = rowButton
        };
    }

    private void OnSelectIndex(GameObject rowObj, int index)
    {
        selectedIndex = index;
        // 선택 시 간단한 강조(배경 색) 및 다른 항목 원복
        foreach (var kv in indexToRow)
        {
            var img = kv.Value.root.GetComponent<Image>();
            if (img != null) img.color = Color.white;
        }
        var image = rowObj.GetComponent<Image>();
        if (image != null) image.color = new Color(0.9f, 0.95f, 1f, 1f);
    }

    private void OnClickBuy()
    {
        if (gameManager == null || gameManager.playerStats == null)
        {
            Debug.LogWarning("IngredientShop: GameManager 또는 PlayerStats가 없습니다.");
            return;
        }
        if (selectedIndex < 0)
        {
            Debug.Log("구매할 재료를 선택하세요.");
            return;
        }

        // 튜토리얼에서는 구매 불가
        if (gameManager.playerStats.Tutorial == false)
        {
            Debug.Log("튜토리얼 진행 중에는 구매할 수 없습니다.");
            return;
        }

        int qty = indexToQuantity.ContainsKey(selectedIndex) ? indexToQuantity[selectedIndex] : 0;
        if (qty <= 0)
        {
            Debug.Log("수량을 선택하세요 (+ 버튼으로 수량 증가).");
            return;
        }

        int price = IngredientDatabase.Instance.GetIngredientPrice(selectedIndex);
        int totalCost = price * qty;
        if (gameManager.playerStats.Money < totalCost)
        {
            Debug.Log("잔액이 부족합니다.");
            return;
        }

        // 차감 & 인벤토리 추가
        var stats = gameManager.playerStats;
        stats.Money -= totalCost;

        // 냉장고 인벤토리에 저장
        List<int> inv = stats.RefrigeratorInventory;
        // 방어적: 길이 보장
        while (inv.Count <= selectedIndex)
        {
            inv.Add(0);
        }
        inv[selectedIndex] += qty;
        stats.RefrigeratorInventory = inv;

        // 저장
        if (gameManager.dbManager != null)
        {
            gameManager.dbManager.SavePlayerStats(stats);
        }

        RefreshMoneyUI();
        Debug.Log($"구매 완료: {IngredientDatabase.Instance.GetIngredientName(selectedIndex)} x{qty} (총 {totalCost}원)");

        // 선택 수량 초기화 및 UI 갱신
        indexToQuantity[selectedIndex] = 0;
        if (indexToRow.TryGetValue(selectedIndex, out var row))
        {
            if (row.quantityText != null) row.quantityText.text = "0";
        }
    }

    private T FindChild<T>(Transform root, string childName) where T : Component
    {
        Transform child = root.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private void AdjustQuantity(int index, int delta)
    {
        if (!indexToQuantity.ContainsKey(index)) indexToQuantity[index] = 0;
        int q = indexToQuantity[index] + delta;
        if (q < 0) q = 0;
        indexToQuantity[index] = q;
        if (indexToRow.TryGetValue(index, out var row))
        {
            if (row.quantityText != null) row.quantityText.text = q.ToString();
        }
    }

    private class RowUI
    {
        public GameObject root;
        public Text nameText;
        public Text priceText;
        public Text quantityText;
        public Button incButton;
        public Button decButton;
        public Button rowButton;
    }
}
