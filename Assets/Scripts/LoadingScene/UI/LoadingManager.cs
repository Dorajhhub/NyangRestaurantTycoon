using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public LoadingBar loadingBar;
    public GameObject networkAlertPanel;
    public Text loadingText;
    public Button alertConfirmButton;
    public DatabaseManager dbManager;

    private string dbPath;
    public string nextSceneName = "CafeScene";
    private bool isInternetAvailable = false;

    void Start()
    {
        alertConfirmButton.onClick.AddListener(RestartApp);
        networkAlertPanel.SetActive(false);
        StartCoroutine(LoadSequence());
    }

    private bool isLoadSequenceActive = true;

    void Update()
    {
        if (isLoadSequenceActive) // 플래그가 true일 때만 로드 텍스트 업데이트
        {
            LoadText();
        }
    }


    float progress = 0f;

    void LoadText()
    {
        int percent = (int)progress;
    
        // 2. 문자열 형태로 변환하여 Text 컴포넌트에 할당
        // 예: progress가 50.5f 일 때, "50%"를 표시
        loadingText.text = percent.ToString() + "%";

    }

    IEnumerator LoadSequence()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "game.db");

        // 1. 로딩 게이지 절반까지 진행
        
        while (progress < 50f)
        {
            progress += Time.deltaTime * 30f;
            loadingBar.SetProgress(progress / 100f);
            yield return null;
        }
        Debug.Log("LOG A: 게이지 절반 완료."); // 🛑 LOG A

        // 2. 인터넷 연결 확인
        yield return StartCoroutine(CheckInternetConnection());
        Debug.Log($"LOG B: 인터넷 연결 확인 완료. 상태: {isInternetAvailable}"); // 🛑 LOG B

        if (!isInternetAvailable)
        {
            networkAlertPanel.SetActive(true);
            isLoadSequenceActive = false;
            yield break;
        }

        // 3. DB 확인 및 생성
        Debug.Log("LOG C: DB 확인 및 생성 완료."); // 🛑 LOG C

        // 4. 게이지 완료
        loadingBar.SetProgress(1f);
        Debug.Log("LOG D: 로딩 바 완료."); // 🛑 LOG D

        Debug.Log("LOG E: DB 초기화 완료."); // 🛑 LOG E

        // GameManager 안전하게 찾기
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.InitializeGameData();
            Debug.Log("LOG F: GameManager 초기화 완료."); // 🛑 LOG F
        }
        else
        {
            Debug.LogError("LOG G: GameManager를 찾을 수 없어 로딩 중단.");
            yield break;
        }

        // 튜토리얼 여부에 따라 씬 전환 (비동기)
        if (gameManager.playerStats.Tutorial == false)
        {
            yield return SceneManager.LoadSceneAsync("Tutorial");
        }
        else
        {
            yield return SceneManager.LoadSceneAsync("CafeScene");
        }
    }
    

    IEnumerator CheckInternetConnection()
    {
        // 물리적 네트워크 연결 확인
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("🚫 네트워크 연결 안 됨");
            isInternetAvailable = false;
            yield break;
        }

        // 실제 인터넷 연결 확인 (서버 핑)
        using (UnityWebRequest request = UnityWebRequest.Get("https://dorajhhub.github.io/Apk-download-and-news/ping"))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("❌ 서버 연결 실패: " + request.error);
                isInternetAvailable = false;
            }
            else
            {
                Debug.Log("✅ 인터넷 연결 확인됨");
                isInternetAvailable = true;
            }
        }
    }

    void CreateDatabase()
    {
        File.Create(dbPath).Dispose();
        Debug.Log("🛠️ SQLite DB 생성 완료");
    }

    void RestartApp()
    {
        networkAlertPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
