// 2025-10-04 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 2025-10-04 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 2025-10-04 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.


public class JuicerManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager; // Inspector 연결
    public GameObject juicerPanel;  // 주서 UI 패널
    public Transform inventoryContent; // 왼쪽 인벤토리 리스트 Content
    public GameObject inventoryItemPrefab; // 왼쪽 아이템 프리팹(NameText, CountText, IncreaseButton, DecreaseButton)
    public Button mixButton; // 섞기 버튼


    [Header("Child Names In Prefab")] 
    public string nameTextChild = "NameText";
    public string countTextChild = "CountText";
    public string increaseButtonChild = "IncreaseButton";
    public string decreaseButtonChild = "DecreaseButton";

    // Ingredient names are now provided by IngredientDatabase singleton

    [Header("Recipes")]

    public List<Recipe> recipes = new List<Recipe>();
    private List<int> tempInventory; // 임시 인벤토리

    void Awake()
    {
        if (mixButton != null)
        {
            mixButton.onClick.RemoveAllListeners();
            mixButton.onClick.AddListener(OnClickMix);
        }
    }

    void Start()
    {
        if (RecipeDatabase.Instance != null)
        {
            recipes = RecipeDatabase.Instance.GetRecipesByTool(CookingTool.Juicer);
            // 또는 주스만 가져오려면:
            // recipes = RecipeDatabase.Instance.GetRecipesByCategory(RecipeCategory.Juice);
            
            Debug.Log($"레시피 로드 완료: {recipes.Count}개");
        }
        else
        {
            Debug.LogError("RecipeDatabase를 찾을 수 없습니다!");
        }
        
    }

    public void OpenAndPopulate()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (juicerPanel != null)
        {
            juicerPanel.SetActive(true);
        }

        // 임시 인벤토리 초기화
        tempInventory = new List<int>(gameManager.playerStats.PlayerInventory);
        PopulateFromPlayerInventory();
    }

    public void Close()
    {
        if (juicerPanel != null)
        {
            juicerPanel.SetActive(false);
        }
    }

    private void PopulateFromPlayerInventory()
    {
        if (gameManager == null || tempInventory == null) return;
        if (inventoryContent == null || inventoryItemPrefab == null) return;

        for (int i = inventoryContent.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryContent.GetChild(i).gameObject);
        }

        for (int index = 0; index < tempInventory.Count; index++)
        {
            int count = tempInventory[index];
            if (count <= 0) continue;
            string name = IngredientDatabase.Instance.GetIngredientName(index);

            GameObject rowObj = Instantiate(inventoryItemPrefab, inventoryContent);

            var row = BindRow(rowObj);
            row.index = index;
            row.nameText.text = name;
            row.countText.text = count.ToString();
            row.increaseButton.onClick.RemoveAllListeners();
            row.increaseButton.onClick.AddListener(() => AdjustIngredientCount(row.index, 1));
            row.decreaseButton.onClick.RemoveAllListeners();
            row.decreaseButton.onClick.AddListener(() => AdjustIngredientCount(row.index, -1));
        }
    }

    private void AdjustIngredientCount(int index, int delta)
    {
        if (tempInventory == null || index < 0 || index >= tempInventory.Count) return;

        int currentCount = tempInventory[index];

        if (delta > 0 && currentCount + delta > gameManager.playerStats.PlayerInventory[index])
        {
            Debug.Log("선택한 수량이 실제 인벤토리 수량을 초과할 수 없습니다.");
            return;
        }

        currentCount += delta;
        if (currentCount < 0) currentCount = 0;

        tempInventory[index] = currentCount;
    }

    private void OnClickMix()
    {
        if (gameManager == null || gameManager.playerStats == null) return;

        Recipe matchedRecipe = FindMatchingRecipe();
        if (matchedRecipe == null)
        {
            Debug.Log("일치하는 레시피가 없습니다.");
            return;
        }

        // 플레이어가 선택한 tempInventory가 레시피와 정확히 같은지 검사
        foreach (var ingredient in matchedRecipe.requiredIngredients)
        {
            int index = ingredient.Key;
            int requiredCount = ingredient.Value;

            if (tempInventory[index] != requiredCount)
            {
                Debug.LogError("선택한 재료 수량이 레시피와 정확히 일치하지 않습니다.");
                return;
            }
        }

        // 레시피와 정확히 일치 → 인벤토리에서 차감
        foreach (var ingredient in matchedRecipe.requiredIngredients)
        {
            int index = ingredient.Key;
            int requiredCount = ingredient.Value;

            if (gameManager.playerStats.PlayerInventory[index] < requiredCount)
            {
                Debug.LogError($"{IngredientDatabase.Instance.GetIngredientName(index)}가 충분하지 않습니다.");
                return;
            }

            var list = gameManager.playerStats.PlayerInventory;
            list[index] -= requiredCount;
            gameManager.playerStats.PlayerInventory = list;
            Debug.Log($"차감 후: {IngredientDatabase.Instance.GetIngredientName(index)} = {gameManager.playerStats.PlayerInventory[index]}");
        }

        // Save
        if (gameManager.dbManager != null)
        {
            gameManager.dbManager.SavePlayerStats(gameManager.playerStats);
        }

        // ✅ 이 부분 추가: tempInventory를 새로 초기화
        tempInventory = new List<int>(gameManager.playerStats.PlayerInventory);

        // Refresh UI
        PopulateFromPlayerInventory();

        Debug.Log($"성공적으로 믹싱 완료: {matchedRecipe.recipeName}!");

        if (gameManager.playerStats != null && gameManager.playerStats.Tutorial == false)
        {
            var cookingTutorial = FindObjectOfType<CookingTutorial>();
            if (cookingTutorial != null)
            {
                cookingTutorial.AdvanceStep();
            }
        }
    }


    private Recipe FindMatchingRecipe()
    {
        foreach (var recipe in recipes)
        {
            bool matches = true;
            foreach (var ingredient in recipe.requiredIngredients)
            {
                int index = ingredient.Key;
                int requiredCount = ingredient.Value;

                if (tempInventory[index] < requiredCount)
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return recipe;
            }
        }
        return null;
    }

    // Ingredient names are retrieved from IngredientDatabase; local method removed

    private InventoryRow BindRow(GameObject rowObj)
    {
        Text nameText = rowObj.transform.Find(nameTextChild).GetComponent<Text>();
        Text countText = rowObj.transform.Find(countTextChild).GetComponent<Text>();
        Button increaseButton = rowObj.transform.Find(increaseButtonChild).GetComponent<Button>();
        Button decreaseButton = rowObj.transform.Find(decreaseButtonChild).GetComponent<Button>();
        return new InventoryRow
        {
            index = -1,
            nameText = nameText,
            countText = countText,
            increaseButton = increaseButton,
            decreaseButton = decreaseButton
        };
    }

    private class InventoryRow
    {
        public int index;
        public Text nameText;
        public Text countText;
        public Button increaseButton;
        public Button decreaseButton;
    }
}