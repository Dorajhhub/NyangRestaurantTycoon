using UnityEngine;

/// <summary>
/// 손님(캐릭터)의 주문 정보를 보관하는 컴포넌트
/// </summary>
public class CustomerOrder : MonoBehaviour
{
    public string requestedRecipeName;
    public Recipe requestedRecipe;

    public void SetOrderByName(string recipeName)
    {
        requestedRecipeName = recipeName;
        if (RecipeDatabase.Instance != null)
        {
            requestedRecipe = RecipeDatabase.Instance.GetRecipeByName(recipeName);
        }
        else
        {
            requestedRecipe = null;
        }
    }
}
