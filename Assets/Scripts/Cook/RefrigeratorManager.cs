using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RefrigeratorManager : MonoBehaviour
{
  [Header("References")]
  public GameManager gameManager; // Inspector 연결
  public Transform contentRoot;   // ScrollView Content
  public GameObject ingredientItemPrefab; // 자식에 Text(TextMeshPro가 아닌 기본 Text), Button, Text 구성
  public ScrollRect scrollRect; // 선택 (없으면 자동 탐색)
  
  [Header("Options")]
  public bool hideZeroCount = true; // 수량 0은 숨기기
  public int contentTopPadding = 40; // 제목에 가리지 않도록 위쪽 패딩
  public float contentItemSpacing = 8f; // 아이템 간격
  public float itemMinHeight = 64f; // 각 아이템 최소 높이

  [Header("Child Names In Prefab")] 
  public string nameTextChild = "NameText";
  public string countTextChild = "CountText";
  public string takeOutButtonChild = "TakeOutButton";

  [Header("Ingredient Names By Index")] 
  public List<string> ingredientNames = new List<string>
  {
    "토마토",   // 0
    "양상추",   // 1
    "치즈",     // 2
    "빵",       // 3
    "고기",     // 4
    "양파",     // 5
    "버섯",     // 6
    "감자",     // 7
    "당근",     // 8
    "계란"      // 9
  };

  private readonly Dictionary<int, IngredientRow> indexToRow = new Dictionary<int, IngredientRow>();
  private Coroutine rebuildCoroutine;
  private bool isRebuilding;

  public void PopulateFromInventory(List<int> inventory)
  {
    if (gameManager == null)
    {
      gameManager = FindObjectOfType<GameManager>();
    }

    if (contentRoot == null || ingredientItemPrefab == null || inventory == null)
    {
      Debug.LogWarning("RefrigeratorManager: 설정이 누락되었거나 인벤토리가 없습니다.");
      return;
    }

    if (scrollRect == null)
    {
      scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    // 중복 실행 방지: 기존 코루틴 정리 후 시작
    if (rebuildCoroutine != null)
    {
      StopCoroutine(rebuildCoroutine);
      rebuildCoroutine = null;
      isRebuilding = false;
    }
    rebuildCoroutine = StartCoroutine(RebuildUIRoutine(inventory));
  }

  private IEnumerator RebuildUIRoutine(List<int> inventory)
  {
    if (isRebuilding) yield break;
    isRebuilding = true;
    SetScrollingEnabled(false);

    // 기존 항목 정리 (한 프레임 뒤 생성하여 스크롤 중 파괴 이슈 회피)
    indexToRow.Clear();
    for (int i = contentRoot.childCount - 1; i >= 0; i--)
    {
      Transform child = contentRoot.GetChild(i);
      if (child != null)
      {
        Destroy(child.gameObject);
      }
    }

    // 프레임 넘겨서 UGUI 내부 코루틴(스크롤바 드래그 등) 안정화
    yield return null;

    // 레이아웃 패딩/간격 적용: Content에 VerticalLayoutGroup/ContentSizeFitter 보장
    var contentVlg = contentRoot != null ? contentRoot.GetComponent<UnityEngine.UI.VerticalLayoutGroup>() : null;
    if (contentVlg == null && contentRoot != null)
    {
      contentVlg = contentRoot.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
    }
    if (contentVlg != null)
    {
      var p = contentVlg.padding;
      p.top = contentTopPadding;
      contentVlg.padding = p;
      contentVlg.spacing = contentItemSpacing;
      contentVlg.childAlignment = TextAnchor.UpperLeft;
      contentVlg.childForceExpandHeight = false;
      contentVlg.childControlHeight = true;
      contentVlg.childForceExpandWidth = true;
      contentVlg.childControlWidth = true;
    }
    var csf = contentRoot != null ? contentRoot.GetComponent<UnityEngine.UI.ContentSizeFitter>() : null;
    if (csf == null && contentRoot != null)
    {
      csf = contentRoot.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
    }
    if (csf != null)
    {
      csf.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
      csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
    }

    for (int index = 0; index < inventory.Count; index++)
    {
      int count = inventory[index];
      if (hideZeroCount && count <= 0) continue; // 0개는 표시하지 않음
      string ingredientName = GetIngredientName(index);

      GameObject rowObj = Instantiate(ingredientItemPrefab, contentRoot);
      // 타이틀을 가리지 않도록 항상 Content의 맨 마지막에 배치
      rowObj.transform.SetAsLastSibling();
      // 레이아웃 사용 시 안전하게 기본 스케일 보장
      rowObj.transform.localScale = Vector3.one;
      
      // Row RectTransform 표준 설정
      var rowRect = rowObj.transform as RectTransform;
      if (rowRect != null)
      {
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
      }

      // LayoutElement 보장 및 최소 높이 적용
      var le = rowObj.GetComponent<UnityEngine.UI.LayoutElement>();
      if (le == null)
      {
        le = rowObj.AddComponent<UnityEngine.UI.LayoutElement>();
      }
      le.minHeight = itemMinHeight;
      le.flexibleHeight = 0f;
      IngredientRow row = BindRow(rowObj);
      row.index = index;
      row.nameText.text = ingredientName;
      row.countText.text = count.ToString();
      row.takeOutButton.onClick.RemoveAllListeners();
      row.takeOutButton.onClick.AddListener(() => OnClickTakeOut(row.index));

      indexToRow[index] = row;
    }

    // 레이아웃 강제 갱신 후 스크롤 재활성화
    Canvas.ForceUpdateCanvases();
    var contentRect = contentRoot as RectTransform;
    if (contentRect != null)
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }
    Canvas.ForceUpdateCanvases();
    SetScrollingEnabled(true);
    isRebuilding = false;
    rebuildCoroutine = null;
  }

  private void SetScrollingEnabled(bool enabled)
  {
    if (scrollRect != null)
    {
      scrollRect.enabled = enabled;
    }

    // 연결된 스크롤바도 함께 비활성화/활성화
    if (scrollRect != null)
    {
      if (scrollRect.horizontalScrollbar != null)
        scrollRect.horizontalScrollbar.enabled = enabled;
      if (scrollRect.verticalScrollbar != null)
        scrollRect.verticalScrollbar.enabled = enabled;
    }
  }

  private string GetIngredientName(int index)
  {
    if (index >= 0 && index < ingredientNames.Count)
    {
      return ingredientNames[index];
    }
    return $"Ingredient {index}";
  }

  private IngredientRow BindRow(GameObject rowObj)
  {
    Text nameText = FindChild<Text>(rowObj.transform, nameTextChild);
    Text countText = FindChild<Text>(rowObj.transform, countTextChild);
    Button takeOutButton = FindChild<Button>(rowObj.transform, takeOutButtonChild);

    if (nameText == null || countText == null || takeOutButton == null)
    {
      Debug.LogError("RefrigeratorManager: 프리팹의 자식 이름이 올바르지 않습니다. name/count/button 자식명을 확인하세요.");
    }

    return new IngredientRow
    {
      root = rowObj,
      nameText = nameText,
      countText = countText,
      takeOutButton = takeOutButton
    };
  }

  private T FindChild<T>(Transform root, string childName) where T : Component
  {
    Transform child = root.Find(childName);
    return child != null ? child.GetComponent<T>() : null;
  }

  private void OnClickTakeOut(int index)
  {
    if (gameManager == null || gameManager.playerStats == null)
    {
      Debug.LogWarning("RefrigeratorManager: GameManager 또는 PlayerStats가 없습니다.");
      return;
    }

    List<int> inventory = gameManager.playerStats.RefrigeratorInventory;
    if (index < 0 || index >= inventory.Count) return;

    if (inventory[index] <= 0)
    {
      Debug.Log("해당 재료가 없습니다.");
      return;
    }

    inventory[index] -= 1;
    // 감소된 수량 UI에 반영
    if (indexToRow.TryGetValue(index, out var row))
    {
      row.countText.text = inventory[index].ToString();
    }

    // 저장
    if (gameManager.dbManager != null)
    {
      gameManager.playerStats.RefrigeratorInventory = inventory; // setter가 Json 업데이트
      var playerInv = gameManager.playerStats.PlayerInventory; // 복사본 가져오기
      playerInv[index] += 1;                                   // 수정
      gameManager.playerStats.PlayerInventory = playerInv;
      gameManager.dbManager.SavePlayerStats(gameManager.playerStats);
    }

    // 튜토리얼 진행: 플레이어 스탯의 Tutorial이 false일 때 단계 1 증가
    if (gameManager.playerStats != null && gameManager.playerStats.Tutorial == false)
    {
      var cookingTutorial = FindObjectOfType<CookingTutorial>();
      if (cookingTutorial != null)
      {
        cookingTutorial.AdvanceStep();
      }
    }
  }

  private class IngredientRow
  {
    public int index;
    public GameObject root;
    public Text nameText;
    public Text countText;
    public Button takeOutButton;
  }
}


