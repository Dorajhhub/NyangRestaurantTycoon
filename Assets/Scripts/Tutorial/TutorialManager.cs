using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public GameManager gameManager;

    public void CompleteTutorial()
    {
        // 튜토리얼 완료 처리
        gameManager.playerStats.Tutorial = true;

        // 저장
        gameManager.dbManager.SavePlayerStats(gameManager.playerStats);
        Debug.Log("✅ 튜토리얼 완료 → 저장됨");

        // 다음 씬으로 이동
        SceneManager.LoadScene("CafeScene");
    }
}
