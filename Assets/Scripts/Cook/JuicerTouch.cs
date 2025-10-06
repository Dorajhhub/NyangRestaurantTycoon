using UnityEngine;
using Unity.AppUI.UI;
using System.Collections.Generic;

public class JuicerTouch : MonoBehaviour
{
  public GameObject juicerPanelObject; // Juicer 패널 오브젝트
  public JuicerManager juicerManager;  // JuicerManager 참조
  public GameManager gameManager;

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
          if (hit.transform.name == "Juicer")
          {
            OpenJuicer();
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
        if (hit.transform.name == "Juicer")
        {
          OpenJuicer();
        }
      }
    }
  }

  private void OpenJuicer()
  {
    if (juicerPanelObject != null)
    {
      juicerPanelObject.SetActive(true);
    }
    if (juicerManager == null)
    {
      juicerManager = FindObjectOfType<JuicerManager>(true);
    }
    if (juicerManager != null)
    {
      juicerManager.OpenAndPopulate();
    }

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

    List<int> playerinventory = gameManager.playerStats.PlayerInventory;
    Debug.Log($"플레이어 인벤토리 로드됨: {string.Join(", ", playerinventory)}");
  }

  public void CloseJuicer()
  {
    if (juicerManager != null)
    {
      juicerManager.Close();
    }
    if (juicerPanelObject != null)
    {
      juicerPanelObject.SetActive(false);
    }
  }
}


