using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 조리 도구 종류
/// </summary>
public enum CookingTool
{
    Juicer,     // 주서기 (믹서기)
    Stove,      // 가스레인지
    Oven,       // 오븐
    Grill,      // 그릴
    Pan,        // 프라이팬
    Pot         // 냄비
}

/// <summary>
/// 레시피 카테고리
/// </summary>
public enum RecipeCategory
{
    Juice,      // 주스
    Sandwich,   // 샌드위치
    Soup,       // 스프
    MainDish,   // 메인 요리
    Dessert,    // 디저트
    Salad       // 샐러드
}

/// <summary>
/// 레시피 클래스 (확장 버전)
/// </summary>
[System.Serializable]
public class Recipe
{
    public string recipeName;                           // 레시피 이름
    public RecipeCategory category;                     // 카테고리
    public CookingTool requiredTool;                    // 필요한 조리도구
    public Dictionary<int, int> requiredIngredients;    // 필요한 재료 (인덱스, 수량)
    public int cookingTime = 10;                        // 조리 시간 (초)
    public int sellingPrice = 100;                      // 판매 가격
    public string description = "";                     // 설명

    /// <summary>
    /// 레시피에 필요한 재료 목록을 문자열로 반환
    /// </summary>
    public string GetIngredientsString(List<string> ingredientNames)
    {
        List<string> items = new List<string>();
        foreach (var ingredient in requiredIngredients)
        {
            string name = ingredient.Key >= 0 && ingredient.Key < ingredientNames.Count 
                ? ingredientNames[ingredient.Key] 
                : $"Ingredient {ingredient.Key}";
            items.Add($"{name} x{ingredient.Value}");
        }
        return string.Join(", ", items);
    }
}

/// <summary>
/// 게임 내 모든 레시피를 중앙에서 관리하는 데이터베이스
/// </summary>
public class RecipeDatabase : MonoBehaviour
{
    private static RecipeDatabase _instance;
    public static RecipeDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RecipeDatabase>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RecipeDatabase");
                    _instance = obj.AddComponent<RecipeDatabase>();
                }
            }
            return _instance;
        }
    }

    [Header("재료 이름 목록")]
    public List<string> ingredientNames = new List<string>
    {
        "토마토",   // 0
        "양상추",   // 1
        "치즈",     // 2
        "빵",       // 3
        "고기",     // 4
        "양파",     // 5
        "버섯",     // 6
        "감자",     // 7
        "당근",     // 8
        "계란"      // 9
    };

    [Header("모든 레시피")]
    public List<Recipe> allRecipes = new List<Recipe>();

    private Dictionary<string, Recipe> recipeByName = new Dictionary<string, Recipe>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        InitializeRecipes();
    }

    /// <summary>
    /// 모든 레시피를 초기화
    /// </summary>
    private void InitializeRecipes()
    {
        allRecipes.Clear();
        recipeByName.Clear();

        // === 주스 카테고리 ===
        AddRecipe("토마토 주스", RecipeCategory.Juice, CookingTool.Juicer, new Dictionary<int, int>
        {
            { 0, 1 } // 토마토 1개
        }, 5, 150, "신선한 토마토로 만든 건강한 주스");

        AddRecipe("당근 주스", RecipeCategory.Juice, CookingTool.Juicer, new Dictionary<int, int>
        {
            { 8, 2 } // 당근 2개
        }, 5, 200, "영양 만점 당근 주스");

        AddRecipe("믹스 주스", RecipeCategory.Juice, CookingTool.Juicer, new Dictionary<int, int>
        {
            { 0, 1 }, // 토마토 1개
            { 8, 1 }  // 당근 1개
        }, 7, 250, "토마토와 당근의 환상적인 조합");

        // === 샌드위치 카테고리 ===
        AddRecipe("치즈 샌드위치", RecipeCategory.Sandwich, CookingTool.Oven, new Dictionary<int, int>
        {
            { 3, 2 }, // 빵 2개
            { 2, 1 }  // 치즈 1개
        }, 10, 300, "고소한 치즈가 듬뿍 들어간 샌드위치");

        AddRecipe("야채 샌드위치", RecipeCategory.Sandwich, CookingTool.Oven, new Dictionary<int, int>
        {
            { 3, 2 }, // 빵 2개
            { 1, 1 }, // 양상추 1개
            { 0, 1 }  // 토마토 1개
        }, 12, 350, "신선한 야채로 가득한 건강 샌드위치");

        AddRecipe("BLT 샌드위치", RecipeCategory.Sandwich, CookingTool.Grill, new Dictionary<int, int>
        {
            { 3, 2 }, // 빵 2개
            { 4, 1 }, // 고기 1개
            { 1, 1 }, // 양상추 1개
            { 0, 1 }  // 토마토 1개
        }, 15, 500, "베이컨, 양상추, 토마토의 클래식한 조합");

        // === 스프 카테고리 ===
        AddRecipe("토마토 스프", RecipeCategory.Soup, CookingTool.Pot, new Dictionary<int, int>
        {
            { 0, 3 }, // 토마토 3개
            { 5, 1 }  // 양파 1개
        }, 20, 400, "진한 토마토 풍미의 따뜻한 스프");

        AddRecipe("버섯 스프", RecipeCategory.Soup, CookingTool.Pot, new Dictionary<int, int>
        {
            { 6, 2 }, // 버섯 2개
            { 5, 1 }  // 양파 1개
        }, 18, 450, "크리미한 버섯 스프");

        // === 요리 카테고리 ===
        AddRecipe("스테이크", RecipeCategory.MainDish, CookingTool.Grill, new Dictionary<int, int>
        {
            { 4, 2 }, // 고기 2개
            { 7, 1 }  // 감자 1개
        }, 25, 800, "육즙 가득한 프리미엄 스테이크");

        AddRecipe("오믈렛", RecipeCategory.MainDish, CookingTool.Pan, new Dictionary<int, int>
        {
            { 9, 3 }, // 계란 3개
            { 2, 1 }  // 치즈 1개
        }, 15, 350, "부드러운 치즈 오믈렛");

        AddRecipe("야채 볶음", RecipeCategory.MainDish, CookingTool.Pan, new Dictionary<int, int>
        {
            { 1, 1 }, // 양상추 1개
            { 8, 1 }, // 당근 1개
            { 6, 1 }, // 버섯 1개
            { 5, 1 }  // 양파 1개
        }, 20, 450, "알록달록 건강한 야채 볶음");

        Debug.Log($"✅ RecipeDatabase 초기화 완료: 총 {allRecipes.Count}개 레시피");
    }

    /// <summary>
    /// 레시피 추가 헬퍼 메서드
    /// </summary>
    private void AddRecipe(string name, RecipeCategory category, CookingTool tool, 
        Dictionary<int, int> ingredients, int cookingTime, int sellingPrice, string description)
    {
        Recipe recipe = new Recipe
        {
            recipeName = name,
            category = category,
            requiredTool = tool,
            requiredIngredients = ingredients,
            cookingTime = cookingTime,
            sellingPrice = sellingPrice,
            description = description
        };
        allRecipes.Add(recipe);
        recipeByName[name] = recipe;
    }

    /// <summary>
    /// 레시피 이름으로 검색
    /// </summary>
    public Recipe GetRecipeByName(string recipeName)
    {
        if (recipeByName.ContainsKey(recipeName))
        {
            return recipeByName[recipeName];
        }
        return null;
    }

    /// <summary>
    /// 카테고리별 레시피 가져오기
    /// </summary>
    public List<Recipe> GetRecipesByCategory(RecipeCategory category)
    {
        List<Recipe> result = new List<Recipe>();
        foreach (var recipe in allRecipes)
        {
            if (recipe.category == category)
            {
                result.Add(recipe);
            }
        }
        return result;
    }

    /// <summary>
    /// 특정 조리도구로 만들 수 있는 레시피 가져오기
    /// </summary>
    public List<Recipe> GetRecipesByTool(CookingTool tool)
    {
        List<Recipe> result = new List<Recipe>();
        foreach (var recipe in allRecipes)
        {
            if (recipe.requiredTool == tool)
            {
                result.Add(recipe);
            }
        }
        return result;
    }
    public string GetIngredientName(int index)
    {
        if (index >= 0 && index < ingredientNames.Count)
        {
            return ingredientNames[index];
        }
        return $"Ingredient {index}";
    }

    /// <summary>
    /// 모든 레시피 반환
    /// </summary>
    public List<Recipe> GetAllRecipes()
    {
        return new List<Recipe>(allRecipes);
    }

    /// <summary>
    /// 재료로 만들 수 있는 레시피 찾기
    /// </summary>
    public List<Recipe> FindRecipesByIngredients(List<int> availableIngredients)
    {
        List<Recipe> canMake = new List<Recipe>();
        
        foreach (var recipe in allRecipes)
        {
            bool canMakeThis = true;
            foreach (var ingredient in recipe.requiredIngredients)
            {
                int index = ingredient.Key;
                int required = ingredient.Value;
                
                if (index >= availableIngredients.Count || availableIngredients[index] < required)
                {
                    canMakeThis = false;
                    break;
                }
            }
            
            if (canMakeThis)
            {
                canMake.Add(recipe);
            }
        }
        
        return canMake;
    }
}