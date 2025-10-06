using UnityEngine;

public class IngredientShopTouch : MonoBehaviour
{
    public IngredientShopManager shopManager; // Inspector에서 연결 가능

    void Update()
    {
        // 모바일 터치
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                CheckTouch(touch.position);
            }
        }

        // PC 마우스 클릭 (테스트용)
        if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }
    }

    private void CheckTouch(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == "IngredientShop")
            {
                OpenShop();
            }
        }
    }

    private void OpenShop()
    {
        if (shopManager == null)
        {
            shopManager = FindObjectOfType<IngredientShopManager>();
        }
        if (shopManager != null)
        {
            shopManager.OpenShop();
            UIInputBlocker.IsBlocking = true;
        }
        else
        {
            Debug.LogError("IngredientShopManager를 찾을 수 없습니다!");
        }
    }

    public void CloseShop()
    {
        if (shopManager != null)
        {
            shopManager.CloseShop();
            UIInputBlocker.IsBlocking = false;
        }
    }
}
