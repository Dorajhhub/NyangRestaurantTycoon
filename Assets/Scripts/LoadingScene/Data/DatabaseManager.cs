using System.IO;
using UnityEngine;
using SQLite4Unity3d;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting.Dependencies.Sqlite;


public class DatabaseManager : MonoBehaviour
{
    private SQLiteConnection _connection;
    private string dbPath;
    public bool IsReady { get; private set; }

    // PRAGMA table_info(...) 결과 매핑용
    private class PragmaTableInfo
    {
        public int cid { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int notnull { get; set; }
        public string dflt_value { get; set; }
        public int pk { get; set; }
    }

    void Awake()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "game.db");
        bool isNew = !File.Exists(dbPath);

        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        InitializeDatabase();

        if (isNew)
        {
            StartCoroutine(HandleNewDatabase());
        }
        else
        {
            Debug.Log("✅ 기존 데이터베이스 로드됨");
        }

        IsReady = true;
    }

    IEnumerator HandleNewDatabase()
    {
        yield return new WaitForSeconds(0.25f);
        Debug.Log("📦 새 데이터베이스 생성됨");

        // ✅ Id = 1 설정을 제거해야 합니다. (SQLite가 자동으로 할당하도록 둠)
        PlayerStats defaultStats = new PlayerStats 
        { 
            Level = 1, 
            XP = 0, 
            Affection = 0, 
            Money = 1000,
            Tutorial = false,
            RefrigeratorInventory = new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            PlayerInventory = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        SavePlayerStats(defaultStats);
    }



    public void InitializeDatabase()
    {
        _connection.CreateTable<PlayerStats>();

        // 마이그레이션: PlayerInventoryJson 컬럼이 없으면 추가
        try
        {
            bool hasColumn = false;
            var pragma = _connection.DeferredQuery<PragmaTableInfo>("PRAGMA table_info(PlayerStats)");
            foreach (var col in pragma)
            {
                if (col.name == "PlayerInventoryJson")
                {
                    hasColumn = true;
                    break;
                }
            }
            if (!hasColumn)
            {
                _connection.Execute("ALTER TABLE PlayerStats ADD COLUMN PlayerInventoryJson TEXT");
                Debug.Log("🗂 PlayerInventoryJson 컬럼 추가됨");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"마이그레이션 검사 중 오류: {e.Message}");
        }

        // 마이그레이션: Money 컬럼이 없으면 추가 (기본값 1000)
        try
        {
            bool hasMoney = false;
            var pragma2 = _connection.DeferredQuery<PragmaTableInfo>("PRAGMA table_info(PlayerStats)");
            foreach (var col in pragma2)
            {
                if (col.name == "Money")
                {
                    hasMoney = true;
                    break;
                }
            }
            if (!hasMoney)
            {
                _connection.Execute("ALTER TABLE PlayerStats ADD COLUMN Money INTEGER DEFAULT 1000");
                Debug.Log("💰 Money 컬럼 추가됨 (기본 1000)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Money 컬럼 마이그레이션 오류: {e.Message}");
        }
    }


    // 저장
    public void SavePlayerStats(PlayerStats stats)
    {
        _connection.InsertOrReplace(stats);
        Debug.Log($"💾 저장 완료: Level {stats.Level}, XP {stats.XP}, 호감도 {stats.Affection}");
    }

    // 불러오기
    public PlayerStats LoadPlayerStats(int id = 1)
    {
        var stats = _connection.Find<PlayerStats>(id);
        if (stats != null)
        {
            Debug.Log($"📤 불러오기 완료: Level {stats.Level}, XP {stats.XP}, 호감도 {stats.Affection}");
        }
        else
        {
            Debug.Log("⚠️ 저장된 데이터 없음"); 
        }
        return stats;
    }

    // 첫 번째 레코드 불러오기 (Id가 1이 아닐 수 있는 경우 대비)
    public PlayerStats LoadFirstPlayerStats()
    {
        var list = _connection.Query<PlayerStats>("SELECT * FROM PlayerStats ORDER BY Id ASC LIMIT 1");
        if (list != null && list.Count > 0)
        {
            var stats = list[0];
            Debug.Log($"📤 첫 레코드 불러오기: Id {stats.Id}, Level {stats.Level}");
            return stats;
        }
        Debug.Log("⚠️ PlayerStats 테이블에 레코드가 없습니다.");
        return null;
    }
}
