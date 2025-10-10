using Unity.AppUI.UI;
using UnityEngine;
using System.Collections.Generic;

public class RefrigeratorTouch : MonoBehaviour
{
  public GameObject refrigeratorPanelObject; // 냉장고 Panel이 붙어있는 GameObject를 Inspector에서 연결
  public GameObject orderPanelObject; // 주문 Panel이 붙어있는 GameObject를 Inspector에서 연결
  public OrderPanelManager orderPanelManager; // 주문 패널 매니저
  public GameManager gameManager; // GameManager를 Inspector에서 연결
  public RefrigeratorManager refrigeratorManager; // UI 채우기용 매니저

  void Start()
  {
    // Inspector에서 GameManager가 연결되지 않은 경우 자동으로 찾기
    if (gameManager == null)
    {
      gameManager = FindObjectOfType<GameManager>();
      if (gameManager == null)
      {
        Debug.LogError("GameManager를 찾을 수 없습니다! Inspector에서 GameManager를 연결해주세요.");
      }
    }
    
    // GameManager가 있지만 playerStats가 초기화되지 않은 경우 초기화
    if (gameManager != null && gameManager.isInitialized == false)
    {
      Debug.Log("GameManager의 playerStats가 초기화되지 않았습니다. 초기화를 시도합니다...");
      gameManager.InitializeGameData();
    }
  }

  void Update()
  {
    // 모바일 터치
    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);

      if (touch.phase == TouchPhase.Began)
      {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
          // 냉장고 터치 처리
          if (hit.transform.name == "refrigerator_1")
          {
            if (refrigeratorPanelObject != null)
            {
              refrigeratorPanelObject.SetActive(true);
              UIInputBlocker.IsBlocking = true; // 패널 열릴 때 입력 차단
              LoadRefrigeratorInventory();
              TryPopulateUI();
            }
          }
          // 주문 패널 터치 처리
          else if (hit.transform.name == "OrderPanel")
          {
            if (orderPanelManager == null)
            {
              orderPanelManager = FindObjectOfType<OrderPanelManager>(true);
            }
            if (orderPanelManager != null)
            {
              var customer = FindObjectOfType<CustomerOrder>();
              orderPanelManager.OpenWithCustomer(customer);
            }
            else if (orderPanelObject != null)
            {
              orderPanelObject.SetActive(true);
            }
          }
        }
      }
    }

    // PC 마우스 클릭 (테스트용)
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit))
      {
        // 냉장고 클릭 처리
        if (hit.transform.name == "refrigerator_1")
        {
          if (refrigeratorPanelObject != null)
          {
            refrigeratorPanelObject.SetActive(true);
            UIInputBlocker.IsBlocking = true; // 패널 열릴 때 입력 차단
            LoadRefrigeratorInventory();
            TryPopulateUI();
          }
        }
        // 주문 패널 클릭 처리
        else if (hit.transform.name == "orderPanel")
        {
          if (orderPanelManager == null)
          {
            orderPanelManager = FindObjectOfType<OrderPanelManager>(true);
          }
          if (orderPanelManager != null)
          {
            var customer = FindObjectOfType<CustomerOrder>();
            orderPanelManager.OpenWithCustomer(customer);
          }
          else if (orderPanelObject != null)
          {
            orderPanelObject.SetActive(true);
          }
          UIInputBlocker.IsBlocking = true;
        }
      }
    }
  }
    
    public void CloseRefrigeratorPanel()
    {
        if (refrigeratorPanelObject != null)
            refrigeratorPanelObject.SetActive(false);
        UIInputBlocker.IsBlocking = false; // 패널 닫힐 때 해제
    }
    
    public void CloseOrderPanel()
    {
        if (orderPanelObject != null)
            orderPanelObject.SetActive(false);
        UIInputBlocker.IsBlocking = false; // 패널 닫힐 때 해제
    }
    
    private void LoadRefrigeratorInventory()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager가 연결되지 않았습니다!");
            return;
        }
        
        if (gameManager.playerStats == null)
        {
            Debug.Log("PlayerStats가 초기화되지 않았습니다. 초기화를 시도합니다...");
            gameManager.InitializeGameData();
            
            if (gameManager.playerStats == null)
            {
                Debug.LogError("PlayerStats 초기화에 실패했습니다!");
                return;
            }
        }
        
        List<int> inventory = gameManager.playerStats.RefrigeratorInventory;
        Debug.Log($"냉장고 인벤토리 로드됨: {string.Join(", ", inventory)}");
        
        // 여기서 인벤토리 데이터를 UI에 표시하는 로직을 추가할 수 있습니다
        // 예: UI 매니저에 인벤토리 데이터 전달
        // UIManager.Instance.UpdateRefrigeratorInventory(inventory);
    }

    private void TryPopulateUI()
    {
        if (refrigeratorManager == null)
        {
            refrigeratorManager = FindObjectOfType<RefrigeratorManager>(true);
        }
        
        if (refrigeratorManager == null)
        {
            Debug.LogWarning("RefrigeratorManager가 씬에 없습니다. UI 갱신을 건너뜁니다.");
            return;
        }

        if (gameManager != null && gameManager.playerStats != null)
        {
            refrigeratorManager.gameManager = gameManager;
            refrigeratorManager.PopulateFromInventory(gameManager.playerStats.RefrigeratorInventory);
        }
    }
}