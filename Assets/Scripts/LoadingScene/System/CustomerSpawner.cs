using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 메인 화면에서 손님을 주기적으로 스폰하고, 주문 위치까지 이동 후 주문 생성, 인근 빈 의자로 안내.
/// 스폰 주기는 호감도에 비례하여 짧아집니다.
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform spawnPoint;      // 입구 위치
    public Transform orderLocation;   // 주문 장소
    public Transform[] chairs;        // 의자들
    public GameObject customerPrefab; // 손님 프리팹 (NavMeshAgent, Animator 포함)

    [Header("Spawn Timing")]
    public float minInterval = 3f;    // 최대 호감도 시 최소 간격
    public float maxInterval = 12f;   // 호감도 낮을 때 간격
    public int affectionMax = 100;    // 호감도 상한 (보정용)
    public int maxConcurrentCustomers = 5;

    private readonly List<GameObject> liveCustomers = new List<GameObject>();
    private bool running;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (running) return;
        running = true;
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        running = false;
    }

    private IEnumerator SpawnLoop()
    {
        while (running)
        {
            // 동시 손님 제한
            CleanupDeadRefs();
            if (liveCustomers.Count < maxConcurrentCustomers)
            {
                SpawnOne();
            }

            float t = 1f;
            int affection = (gameManager != null && gameManager.playerStats != null) ? gameManager.playerStats.Affection : 0;
            affection = Mathf.Clamp(affection, 0, affectionMax);
            // 호감도 비례 보간 (호감도 높을수록 간격 짧아짐)
            float interval = Mathf.Lerp(maxInterval, minInterval, affection / (float)affectionMax);
            yield return new WaitForSeconds(interval);
        }
    }

    private void CleanupDeadRefs()
    {
        for (int i = liveCustomers.Count - 1; i >= 0; i--)
        {
            if (liveCustomers[i] == null) liveCustomers.RemoveAt(i);
        }
    }

    private void SpawnOne()
    {
        if (customerPrefab == null || spawnPoint == null || orderLocation == null) return;
        GameObject go = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        go.name = "Customer";

        var agent = go.GetComponent<NavMeshAgent>();
        var anim = go.GetComponent<Animator>();
        var order = go.GetComponent<CustomerOrder>();
        if (order == null) order = go.AddComponent<CustomerOrder>();
        order.exitTarget = spawnPoint; // 퇴장 시 입구로

        liveCustomers.Add(go);
        StartCoroutine(CustomerRoutine(go, agent, anim, order));
    }

    private IEnumerator CustomerRoutine(GameObject go, NavMeshAgent agent, Animator anim, CustomerOrder order)
    {
        // 주문 장소로 이동
        if (agent != null)
        {
            agent.SetDestination(orderLocation.position);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                if (anim != null)
                {
                    anim.SetBool("isWalking", true);
                    anim.SetFloat("Speed", agent.velocity.magnitude);
                }
                yield return null;
            }
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetFloat("Speed", 0f);
            }
        }

        // 주문 생성: 플레이어가 보유한 조리 도구로 만들 수 있는 레시피 중 무작위
        var db = RecipeDatabase.Instance;
        Recipe recipe = null;
        if (db != null)
        {
            var all = db.GetAllRecipes();
            List<Recipe> candidates = new List<Recipe>();
            List<int> ownedTools = (gameManager != null && gameManager.playerStats != null) ? gameManager.playerStats.OwnedToolIndices : new List<int> { (int)CookingTool.Juicer };
            HashSet<CookingTool> ownedToolSet = new HashSet<CookingTool>();
            foreach (var idx in ownedTools)
            {
                if (idx >= 0 && idx <= (int)CookingTool.Pot)
                {
                    ownedToolSet.Add((CookingTool)idx);
                }
            }

            foreach (var r in all)
            {
                if (ownedToolSet.Contains(r.requiredTool))
                {
                    candidates.Add(r);
                }
            }

            if (candidates.Count > 0)
            {
                recipe = candidates[Random.Range(0, candidates.Count)];
            }
        }
        string recipeName = recipe != null ? recipe.recipeName : "토마토 주스"; // fallback
        int wantCount = Random.Range(1, 3); // 1~2개 랜덤 수량
        order.SetOrder(recipeName, wantCount);
        Debug.Log($"🧾 손님 주문: {recipeName}");

        // 인근 빈 의자 탐색 후 이동 (충돌 회피 및 없으면 퇴장)
        Transform targetChair = FindSeatAvoidingCats();
        if (targetChair != null && agent != null)
        {
            agent.SetDestination(targetChair.position);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                if (anim != null)
                {
                    anim.SetBool("isWalking", true);
                    anim.SetFloat("Speed", agent.velocity.magnitude);
                }
                yield return null;
            }
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetFloat("Speed", 0f);
            }
        }
        else
        {
            // 자리가 없거나 주변에 고양이(Cat)가 있으면 주문 취소 후 퇴장
            order.CancelOrder();
            yield return StartCoroutine(ExitScene(agent, anim, order));
            yield break;
        }
    }

    private Transform FindEmptyChair()
    {
        foreach (Transform chair in chairs)
        {
            if (chair == null) continue;
            Collider[] colliders = Physics.OverlapSphere(chair.position, 0.5f);
            bool occupied = false;
            foreach (var col in colliders)
            {
                if (col != null && col.gameObject != null && col.gameObject.name == "Customer")
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied) return chair;
        }
        return null;
    }

    private Transform FindSeatAvoidingCats()
    {
        // 1차: 빈 의자 중 주변 3칸(임의로 반경 3m) 내 "Cat" 없으면 선택
        List<Transform> candidates = new List<Transform>();
        foreach (var chair in chairs)
        {
            if (chair == null) continue;
            if (IsChairOccupied(chair)) continue;
            if (!IsCatNearby(chair, 3f))
            {
                candidates.Add(chair);
            }
        }
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        // 2차: 고양이가 있어도 빈 의자 중 하나라도 있으면 그쪽으로 (회피 실패 시에도 착석 허용할지 정책 결정; 여기서는 돌아감)
        // 요구사항에 따라 고양이가 있으면 다른 쪽으로 가고, 없으면 돌아감 -> 빈 의자가 하나도 없거나 모두 주변에 Cat이 있으면 null 반환
        foreach (var chair in chairs)
        {
            if (chair == null) continue;
            if (!IsChairOccupied(chair))
            {
                // 모든 빈 의자 주변에 Cat이 있음 -> null
            }
        }
        return null;
    }

    private bool IsChairOccupied(Transform chair)
    {
        Collider[] colliders = Physics.OverlapSphere(chair.position, 0.5f);
        foreach (var col in colliders)
        {
            if (col != null && col.gameObject != null && col.gameObject.name == "Customer")
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCatNearby(Transform chair, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(chair.position, radius);
        foreach (var col in colliders)
        {
            if (col != null && col.gameObject != null && col.gameObject.name == "Cat")
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ExitScene(NavMeshAgent agent, Animator anim, CustomerOrder order)
    {
        Transform exit = (order != null && order.exitTarget != null) ? order.exitTarget : spawnPoint;
        if (agent != null && exit != null)
        {
            agent.SetDestination(exit.position);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                if (anim != null)
                {
                    anim.SetBool("isWalking", true);
                    anim.SetFloat("Speed", agent.velocity.magnitude);
                }
                yield return null;
            }
        }
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetFloat("Speed", 0f);
        }
        yield return new WaitForSeconds(0.2f);
        if (order != null) Destroy(order.gameObject);
    }
}
