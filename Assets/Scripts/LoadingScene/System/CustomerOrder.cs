using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 손님(캐릭터)의 주문 정보를 보관하는 컴포넌트
/// </summary>
public class CustomerOrder : MonoBehaviour
{
    public string requestedRecipeName;
    public Recipe requestedRecipe;
    public bool isServed;
    public bool isCanceled;
    public Transform exitTarget;
    public float exitDelaySeconds = 5f;

    private bool postServeFlowStarted;
    private NavMeshAgent navMeshAgent;
    private Animator characterAnimator;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterAnimator = GetComponent<Animator>();
    }

    public void SetOrderByName(string recipeName)
    {
        requestedRecipeName = recipeName;
        if (RecipeDatabase.Instance != null)
        {
            requestedRecipe = RecipeDatabase.Instance.GetRecipeByName(recipeName);
        }
        else
        {
            requestedRecipe = null;
        }
    }

    public void MarkServed()
    {
        if (isServed) return;
        isServed = true;
        Debug.Log($"🍹 주문 서빙 완료: {requestedRecipeName}");
        StartPostServeFlow();
    }

    public void CancelOrder()
    {
        if (isCanceled) return;
        isCanceled = true;
        Debug.Log($"🛑 주문 취소: {requestedRecipeName}");
    }

    public void StartPostServeFlow()
    {
        if (postServeFlowStarted) return;
        postServeFlowStarted = true;
        StartCoroutine(PostServeRoutine());
    }

    private System.Collections.IEnumerator PostServeRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, exitDelaySeconds));

        int payment = requestedRecipe != null ? requestedRecipe.sellingPrice : 0;
        var gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.playerStats != null)
        {
            gm.playerStats.Money += payment;
            if (gm.dbManager != null)
            {
                gm.dbManager.SavePlayerStats(gm.playerStats);
            }
            Debug.Log($"💵 손님이 결제했습니다: +{payment}원");
        }
        else
        {
            Debug.LogWarning("GameManager 또는 PlayerStats를 찾을 수 없어 결제 처리를 건너뜁니다.");
        }

        Transform target = exitTarget;
        if (target == null)
        {
            var tutorial = FindObjectOfType<CookingTutorial>();
            if (tutorial != null)
            {
                target = tutorial.entrance;
            }
        }

        if (navMeshAgent != null && target != null)
        {
            navMeshAgent.SetDestination(target.position);
            while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                if (characterAnimator != null)
                {
                    characterAnimator.SetBool("isWalking", true);
                    characterAnimator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
                }
                yield return null;
            }
        }

        if (characterAnimator != null)
        {
            characterAnimator.SetBool("isWalking", false);
            characterAnimator.SetFloat("Speed", 0f);
        }

        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}
