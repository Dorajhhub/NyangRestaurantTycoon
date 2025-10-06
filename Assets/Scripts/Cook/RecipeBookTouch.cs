using UnityEngine;

public class RecipeBookTouch : MonoBehaviour
{
    public RecipeBookManager recipeBookManager; // Inspector에서 연결

    void Update()
    {
        // 모바일 터치
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                CheckRecipeBookTouch(touch.position);
            }
        }

        // PC 마우스 클릭 (테스트용)
        if (Input.GetMouseButtonDown(0))
        {
            CheckRecipeBookTouch(Input.mousePosition);
        }
    }

    private void CheckRecipeBookTouch(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == "RecipeBook")
            {
                OpenRecipeBook();
            }
        }
    }

    private void OpenRecipeBook()
    {
        if (recipeBookManager == null)
        {
            recipeBookManager = FindObjectOfType<RecipeBookManager>();
        }

        if (recipeBookManager != null)
        {
            recipeBookManager.OpenRecipeBook();
            UIInputBlocker.IsBlocking = true; // UI 열릴 때 입력 차단
        }
        else
        {
            Debug.LogError("RecipeBookManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// Close 버튼에 연결할 메서드
    /// </summary>
    public void CloseRecipeBook()
    {
        if (recipeBookManager != null)
        {
            recipeBookManager.CloseRecipeBook();
            UIInputBlocker.IsBlocking = false; // UI 닫힐 때 차단 해제
        }
    }
}