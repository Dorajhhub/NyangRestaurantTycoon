using SQLite4Unity3d;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json; // Newtonsoft.Json 패키지 필요

public class PlayerStats
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int Level { get; set; }
    public int XP { get; set; }
    public int Affection { get; set; }

    public bool Tutorial { get; set; }
    public string RefrigeratorInventoryJson { get; set; }
    public string PlayerInventoryJson { get; set; }

    [Ignore]
    public List<int> RefrigeratorInventory
    {
        get
        {
            if (string.IsNullOrEmpty(RefrigeratorInventoryJson))
                return new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            return JsonConvert.DeserializeObject<List<int>>(RefrigeratorInventoryJson);
        }
        set
        {
            RefrigeratorInventoryJson = JsonConvert.SerializeObject(value);
        }
    }

    [Ignore]
    public List<int> PlayerInventory
    {
        get
        {
            if (string.IsNullOrEmpty(PlayerInventoryJson))
                return new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 기본값: 전부 0
            return JsonConvert.DeserializeObject<List<int>>(PlayerInventoryJson);
        }
        set
        {
            PlayerInventoryJson = JsonConvert.SerializeObject(value);
        }
    }
}