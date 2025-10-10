using UnityEngine;
using UnityEngine.UI;

public class OrderPanelManager : MonoBehaviour
{
    [Header("References")]
    public GameObject orderPanel; // 패널 루트
    public Transform listRoot;    // Need 프리팹이 붙을 Content 같은 곳
    public GameObject needItemPrefab; // 이름: Need, 내부에 FoodName(Text), Want(Text)

    [Header("Child Names")]
    public string foodNameChild = "FoodName";
    public string wantChild = "Want";

    public void OpenWithCustomer(CustomerOrder order)
    {
        if (orderPanel != null) orderPanel.SetActive(true);
        Populate(order);
    }

    public void Close()
    {
        if (orderPanel != null) orderPanel.SetActive(false);
    }

    public void Populate(CustomerOrder order)
    {
        if (listRoot == null || needItemPrefab == null || order == null) return;

        for (int i = listRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(listRoot.GetChild(i).gameObject);
        }

        GameObject go = Instantiate(needItemPrefab, listRoot);
        Text foodName = FindChild<Text>(go.transform, foodNameChild);
        Text want = FindChild<Text>(go.transform, wantChild);

        if (foodName != null) foodName.text = order.requestedRecipeName;
        if (want != null) want.text = order.requestedCount.ToString();
    }

    private T FindChild<T>(Transform root, string name) where T : Component
    {
        Transform t = root.Find(name);
        return t != null ? t.GetComponent<T>() : null;
    }
}
