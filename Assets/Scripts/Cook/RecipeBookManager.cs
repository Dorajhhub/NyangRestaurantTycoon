using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBookManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject recipeBookPanel;      // 레시피북 전체 패널
    public Transform recipeListContent;     // ScrollView의 Content (레시피 목록)
    public GameObject recipeItemPrefab;     // 레시피 아이템 프리팹

    [Header("Filter Buttons")]
    public Button allButton;                // 전체 보기
    public Button juiceButton;              // 주스
    public Button sandwichButton;           // 샌드위치
    public Button soupButton;               // 스프
    public Button mainDishButton;           // 메인 요리

    [Header("Child Names in Prefab")]
    public string recipeNameTextChild = "RecipeNameText";
    public string toolTextChild = "ToolText";
    public string ingredientsTextChild = "IngredientsText";
    public string cookingTimeTextChild = "CookingTimeText";
    public string priceTextChild = "PriceText";
    public string descriptionTextChild = "DescriptionText";

    [Header("Tool Display Names")]
    private Dictionary<CookingTool, string> toolNames = new Dictionary<CookingTool, string>
    {
        { CookingTool.Juicer, "🥤 믹서기" },
        { CookingTool.Stove, "🔥 가스레인지" },
        { CookingTool.Oven, "🔥 오븐" },
        { CookingTool.Grill, "🍖 그릴" },
        { CookingTool.Pan, "🍳 프라이팬" },
        { CookingTool.Pot, "🍲 냄비" }
    };

    private RecipeCategory? currentFilter = null; // null이면 전체 보기

    void Start()
    {
        // 버튼 이벤트 연결 (카테고리 기준)
        if (allButton != null)
            allButton.onClick.AddListener(() => FilterRecipes(null));
        if (juiceButton != null)
            juiceButton.onClick.AddListener(() => FilterRecipes(RecipeCategory.Juice));
        if (sandwichButton != null)
            sandwichButton.onClick.AddListener(() => FilterRecipes(RecipeCategory.Sandwich));
        if (soupButton != null)
            soupButton.onClick.AddListener(() => FilterRecipes(RecipeCategory.Soup));
        if (mainDishButton != null)
            mainDishButton.onClick.AddListener(() => FilterRecipes(RecipeCategory.MainDish));

        // 초기에는 패널 닫기
        if (recipeBookPanel != null)
            recipeBookPanel.SetActive(false);
    }

    /// <summary>
    /// 레시피북 열기
    /// </summary>
    public void OpenRecipeBook()
    {
        if (recipeBookPanel != null)
        {
            recipeBookPanel.SetActive(true);
        }

        // 전체 레시피 표시
        FilterRecipes(null);
    }

    /// <summary>
    /// 레시피북 닫기
    /// </summary>
    public void CloseRecipeBook()
    {
        if (recipeBookPanel != null)
        {
            recipeBookPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 레시피 필터링 및 표시
    /// </summary>
    private void FilterRecipes(RecipeCategory? category)
    {
        currentFilter = category;

        // 기존 항목 삭제
        foreach (Transform child in recipeListContent)
        {
            Destroy(child.gameObject);
        }

        // 레시피 가져오기
        List<Recipe> recipes;
        if (category.HasValue)
        {
            recipes = RecipeDatabase.Instance.GetRecipesByCategory(category.Value);
        }
        else
        {
            recipes = RecipeDatabase.Instance.GetAllRecipes();
        }

        Debug.Log($"총 {recipes.Count}개 레시피 로드됨"); // 디버그 추가

        // 레시피 UI 생성
        foreach (var recipe in recipes)
        {
            Debug.Log($"레시피 생성 중: {recipe.recipeName}"); // 디버그 추가
            CreateRecipeItem(recipe);
        }

        Debug.Log($"레시피 필터링 완료: {recipes.Count}개 표시");
    }

    /// <summary>
    /// 레시피 아이템 UI 생성
    /// </summary>
    private void CreateRecipeItem(Recipe recipe)
    {
        if (recipeItemPrefab == null || recipeListContent == null) return;

        GameObject item = Instantiate(recipeItemPrefab, recipeListContent);
        
        // ✅ 높이 강제 설정
        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 220); // 높이 220으로 설정
        }
        
        // ✅ Layout Element 설정
        LayoutElement layout = item.GetComponent<LayoutElement>();
        if (layout == null)
            layout = item.AddComponent<LayoutElement>();
        layout.minHeight = 220;
        layout.preferredHeight = 220;

        // 레시피 이름
        Text nameText = FindChildComponent<Text>(item.transform, recipeNameTextChild);
        if (nameText != null)
            nameText.text = recipe.recipeName;

        // 조리 도구
        Text toolText = FindChildComponent<Text>(item.transform, toolTextChild);
        if (toolText != null)
        {
            string toolName = toolNames.ContainsKey(recipe.requiredTool) 
                ? toolNames[recipe.requiredTool] 
                : recipe.requiredTool.ToString();
            toolText.text = $"도구: {toolName}";
        }

        // 필요한 재료
        Text ingredientsText = FindChildComponent<Text>(item.transform, ingredientsTextChild);
        if (ingredientsText != null)
        {
            ingredientsText.text = "재료: " + GetIngredientsString(recipe);
        }

        // 조리 시간
        Text cookingTimeText = FindChildComponent<Text>(item.transform, cookingTimeTextChild);
        if (cookingTimeText != null)
            cookingTimeText.text = $"⏱️ {recipe.cookingTime}초";

        // 판매 가격
        Text priceText = FindChildComponent<Text>(item.transform, priceTextChild);
        if (priceText != null)
            priceText.text = $"💰 {recipe.sellingPrice}원";

        // 설명
        Text descriptionText = FindChildComponent<Text>(item.transform, descriptionTextChild);
        if (descriptionText != null)
            descriptionText.text = recipe.description;
    }

    /// <summary>
    /// 재료 목록을 문자열로 변환
    /// </summary>
    private string GetIngredientsString(Recipe recipe)
    {
        List<string> items = new List<string>();
        foreach (var ingredient in recipe.requiredIngredients)
        {
            string name = IngredientDatabase.Instance.GetIngredientName(ingredient.Key);
            items.Add($"{name} x{ingredient.Value}");
        }
        return string.Join(", ", items);
    }

    /// <summary>
    /// 자식 컴포넌트 찾기 헬퍼 메서드
    /// </summary>
    private T FindChildComponent<T>(Transform parent, string childName) where T : Component
    {
        Transform child = parent.Find(childName);
        if (child != null)
            return child.GetComponent<T>();
        return null;
    }
}