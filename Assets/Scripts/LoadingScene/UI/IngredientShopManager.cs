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

    private GameManager gameManager;
    private int selectedIndex = -1;

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
        if (nameText != null) nameText.text = name;
        if (priceText != null) priceText.text = $"가격: {price}원";

        // 선택을 위해 Row 또는 내부 Button에 클릭 이벤트 연결
        Button rowButton = rowObj.GetComponent<Button>();
        if (rowButton == null)
        {
            rowButton = rowObj.AddComponent<Button>();
        }
        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(() => OnSelectIndex(rowObj, index));
    }

    private void OnSelectIndex(GameObject rowObj, int index)
    {
        selectedIndex = index;
        // 선택 시 간단한 강조(배경 색)
        var image = rowObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.9f, 0.95f, 1f, 1f);
        }
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

        int price = IngredientDatabase.Instance.GetIngredientPrice(selectedIndex);
        if (gameManager.playerStats.Money < price)
        {
            Debug.Log("잔액이 부족합니다.");
            return;
        }

        // 차감 & 인벤토리 추가
        var stats = gameManager.playerStats;
        stats.Money -= price;

        List<int> inv = stats.PlayerInventory;
        // 방어적: 길이 보장
        while (inv.Count <= selectedIndex)
        {
            inv.Add(0);
        }
        inv[selectedIndex] += 1;
        stats.PlayerInventory = inv;

        // 저장
        if (gameManager.dbManager != null)
        {
            gameManager.dbManager.SavePlayerStats(stats);
        }

        RefreshMoneyUI();
        Debug.Log($"구매 완료: {IngredientDatabase.Instance.GetIngredientName(selectedIndex)}");
    }

    private T FindChild<T>(Transform root, string childName) where T : Component
    {
        Transform child = root.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }
}
