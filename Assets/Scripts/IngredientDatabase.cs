using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 재료 데이터 중앙 관리 싱글턴. 다른 어디서나 재료 이름을 참조할 수 있게 제공.
/// </summary>
public class IngredientDatabase : MonoBehaviour
{
    private static IngredientDatabase _instance;
    public static IngredientDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<IngredientDatabase>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("IngredientDatabase");
                    _instance = obj.AddComponent<IngredientDatabase>();
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

    [Header("재료 가격 목록 (index와 1:1 매핑)")]
    public List<int> ingredientPrices = new List<int>
    {
        100, // 토마토
        80,  // 양상추
        150, // 치즈
        120, // 빵
        300, // 고기
        90,  // 양파
        130, // 버섯
        110, // 감자
        140, // 당근
        160  // 계란
    };

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string GetIngredientName(int index)
    {
        if (index >= 0 && index < ingredientNames.Count)
        {
            return ingredientNames[index];
        }
        return $"Ingredient {index}";
    }

    public List<string> GetAllIngredientNames()
    {
        return new List<string>(ingredientNames);
    }

    public int GetIngredientPrice(int index)
    {
        if (index >= 0 && index < ingredientPrices.Count)
        {
            return ingredientPrices[index];
        }
        // 가격 리스트가 짧을 경우 기본값
        return 100;
    }
}
