using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public DatabaseManager dbManager; // 인스펙터 연결 필요!

    public PlayerStats playerStats;
    public bool isInitialized;

    // GameManager.cs

    void Awake()
    {
        if (dbManager == null)
        {
            dbManager = FindObjectOfType<DatabaseManager>();
        }
        // DB가 준비되었는지 확인 후 초기화
        if (dbManager != null && dbManager.IsReady)
        {
            InitializeGameData();
        }
        else
        {
            // 혹시 Awake 순서 문제일 경우 다음 프레임에 초기화 시도
            StartCoroutine(InitializeNextFrame());
        }
    }

    System.Collections.IEnumerator InitializeNextFrame()
    {
        yield return null;
        if (dbManager == null)
        {
            dbManager = FindObjectOfType<DatabaseManager>();
        }
        if (dbManager != null)
        {
            while (!dbManager.IsReady) yield return null;
            InitializeGameData();
        }
    }

    public void InitializeGameData()
    {
        if (isInitialized) return;
        
        // 1. DB에서 데이터 로드 시도
        playerStats = dbManager.LoadPlayerStats(1);
        if (playerStats == null)
        {
            // Id=1이 없으면 첫 레코드 시도
            playerStats = dbManager.LoadFirstPlayerStats();
        }

        // 2-A. 로드 성공했지만 Tutorial == false면 기본 데이터로 강제 초기화
        if (playerStats != null && playerStats.Tutorial == false)
        {
            Debug.LogWarning("튜토리얼 중단 상태 감지: 기본 데이터로 초기화합니다.");
            playerStats = new PlayerStats
            {
                Level = 1,
                XP = 0,
                Affection = 0,
                Tutorial = false,
                RefrigeratorInventory = new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                PlayerInventory = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            dbManager.SavePlayerStats(playerStats);
        }

        // 2-B. 로드 실패 시 NullReferenceException 방지 및 기본값 생성
        if (playerStats == null)
        {
            Debug.LogWarning("⚠️ 저장된 데이터가 없습니다. 기본값으로 초기화해야 합니다.");
            
            // 새 PlayerStats 객체 생성 및 초기값 할당
            playerStats = new PlayerStats
            {
                Level = 1,
                XP = 0,
                Affection = 0,
                Tutorial = false,
                RefrigeratorInventory = new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                PlayerInventory = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            
            // 새로 만든 기본 데이터를 DB에 저장 (다음 로딩 때 재사용)
            dbManager.SavePlayerStats(playerStats);
        }

        Debug.Log($"🎮 불러온 데이터: Level {playerStats.Level}, XP {playerStats.XP}, 호감도 {playerStats.Affection}, Tutorial {playerStats.Tutorial}");
        isInitialized = true;
    }
}