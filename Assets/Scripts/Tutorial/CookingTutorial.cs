// 2025-09-13 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CookingTutorial : MonoBehaviour
{
    public int step = 0;
    public Text tutorialText;
    private bool isCatSpawned = false;

    public Transform entrance; // 손님이 등장하는 위치
    public Transform orderLocation; // 손님이 주문하는 위치
    public Transform[] chairs; // 의자 배열

    public void AdvanceStep()
    {
        step++;
        ShowStep(step);
    }

    void Start()
    {
        SpawnCatOnce(); // 씬 시작 시 "cat" 소환
        ShowStep(step);
    }

    void ShowStep(int s)
    {
        switch (s)
        {
            case 0:
                ShowMessage("손님이 왔어요! 요리를 시작해볼까요?");
                HighlightObject("Cat");
                break;
            case 1:
                ShowMessage("손님은 토마토 주스를 원하네요.");
                HighlightObject("orderPanel");
                break;
            case 2:
                ShowMessage("냉장고에서 토마토를 꺼내세요.");
                HighlightObject("refrigerator_1");
                break;
            case 3:
                ShowMessage("주스기로 토마토 주스를 만들어주세요.");
                HighlightObject("Juicer");
                break;
            case 4:
                ShowMessage("레시피가 궁금하다면 요리책을 확인해보세요.");
                HighlightObject("RecipeBook");
                break;
            case 5:
                ShowMessage("재료는 무한하지 않아요. 직접 구매해야 해요.");
                HighlightObject("IngredientShop");
                break;
            case 6:
                ShowMessage("호감도가 낮으면 손님이 줄어요.");
                HighlightObject("ReputationMeter");
                break;
            case 7:
                ShowMessage("음식이 늦게 나가면 호감도가 떨어져요.");
                HighlightObject("CookingTimer");
                break;
            case 8:
                ShowMessage("맛있는 요리를 만들면 호감도가 올라가요!");
                HighlightObject("TasteMeter");
                break;
            case 9:
                ShowMessage("돈이 없으면 파산할 수 있어요. 주의하세요!");
                HighlightObject("Wallet");
                break;
            case 10:
                ShowMessage("튜토리얼이 끝났어요! 이제 카페를 운영해보세요.");
                CompleteTutorial();
                break;
        }
    }

    void ShowMessage(string msg)
    {
        if (tutorialText != null)
            tutorialText.text = msg;
        Debug.Log("📢 튜토리얼 메시지: " + msg);
    }

    void HighlightObject(string objName)
    {
        GameObject obj = FindInSceneIncludingInactive(objName);
        if (obj != null)
        {
            obj.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"⚠️ {objName} 오브젝트를 찾을 수 없음!");
        }
    }

    // 비활성 오브젝트까지 검색
    GameObject FindInSceneIncludingInactive(string targetName)
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == targetName)
                    return t.gameObject;
            }
        }
        return null;
    }


    void CompleteTutorial()
    {
        FindObjectOfType<TutorialManager>().CompleteTutorial();
    }

    void SpawnCatOnce()
    {
        if (!isCatSpawned)
        {
            GameObject cat = Instantiate(Resources.Load<GameObject>("Cat"), entrance.position, Quaternion.identity); // "Cat" 프리팹 로드 및 생성
            cat.name = "Cat"; // 생성된 오브젝트의 이름 설정
            Debug.Log("🐱 'Cat'이 소환되었습니다.");
            isCatSpawned = true; // 소환 상태를 true로 설정
            StartCoroutine(HandleCustomer(cat)); // 손님 행동 처리
        }
    }

    IEnumerator HandleCustomer(GameObject cat)
    {
        NavMeshAgent agent = cat.GetComponent<NavMeshAgent>();
        Animator animator = cat.GetComponent<Animator>();

        // 1. Entrance에서 OrderLocation으로 이동
        agent.SetDestination(orderLocation.position);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetBool("isWalking", true);
            yield return null;
        }

        // 도착 완료
        animator.SetFloat("Speed", 0);
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(2f);

        // 3. 빈 의자 찾기
        Transform targetChair = FindEmptyChair();
        AdvanceStep();
        // 빈 의자가 있으면 이동
        agent.SetDestination(targetChair.position);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            animator.SetBool("isWalking", true);
            animator.SetFloat("Speed", agent.velocity.magnitude); // 애니메이션 업데이트  
            yield return null;
        }

        animator.SetFloat("Speed", 0);
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(0.5f);
        AdvanceStep();
    }

    Transform FindEmptyChair()
    {
        foreach (Transform chair in chairs)
        {
            Collider[] colliders = Physics.OverlapSphere(chair.position, 0.5f);
            bool isOccupied = false;
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.name == "Cat")
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                return chair; // 빈 의자 반환
            }
        }

        return null; // 빈 의자가 없음
    }
}