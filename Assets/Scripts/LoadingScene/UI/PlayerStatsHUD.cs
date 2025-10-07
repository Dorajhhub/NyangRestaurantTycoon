using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsHUD : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Text moneyText;
    public Text affectionText;

    [Header("Format Strings")]
    public string moneyFormat = "돈: {0}원";
    public string affectionFormat = "호감도: {0}";

    private int lastMoney = int.MinValue;
    private int lastAffection = int.MinValue;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    void Update()
    {
        if (gameManager == null || gameManager.playerStats == null) return;

        int money = gameManager.playerStats.Money;
        int affection = gameManager.playerStats.Affection;

        if (money != lastMoney)
        {
            if (moneyText != null)
            {
                moneyText.text = string.Format(moneyFormat, money);
            }
            lastMoney = money;
        }

        if (affection != lastAffection)
        {
            if (affectionText != null)
            {
                affectionText.text = string.Format(affectionFormat, affection);
            }
            lastAffection = affection;
        }
    }
}
