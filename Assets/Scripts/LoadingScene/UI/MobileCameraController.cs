// 2025-09-16 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCameraController : MonoBehaviour
{
    public float dragSpeed = 5f; // �巡�� �ӵ�
    public float minZoom = 3f; // �ּ� ��
    public float maxZoom = 50f; // �ִ� ��
    public float zoomSpeed = 1f; // �� �ӵ�

    public Vector2 minBounds; // ī�޶� �̵� �ּ� ����
    public Vector2 maxBounds; // ī�޶� �̵� �ִ� ����

    private Camera cam;
    private Vector3 lastWorldPos;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera�� �������� �ʾҽ��ϴ�. ī�޶� 'MainCamera' �±׸� �߰��ϼ���.");
        }
    }

    void Update()
    {
        // UI �г�/��ũ�� ���̸� ī�޶� �Է� ����
        if (UIInputBlocker.IsBlocking || IsTouchOverUI())
        {
            return;
        }

        HandleDrag();
        HandleZoom();
        ClampCameraPosition();
    }

    bool IsTouchOverUI()
    {
        if (EventSystem.current == null) return false;

        // ����� ��ġ �˻�
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                {
                    return true;
                }
            }
        }
        else
        {
            // ������/PC ���콺 �˻�
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
        }

        return false;
    }

    void HandleDrag()
    {
        if (UIInputBlocker.IsBlocking || IsTouchOverUI()) return;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // ��ġ�� ȭ�� ���� ���� �ִ��� Ȯ��
            if (touch.position.x < 0 || touch.position.x > Screen.width ||
                touch.position.y < 0 || touch.position.y > Screen.height)
            {
                return; // ȭ�� ������ ����� ó������ ����
            }

            Ray ray = cam.ScreenPointToRay(touch.position);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // y=0 ���
            float distance;

            if (groundPlane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);

                if (touch.phase == TouchPhase.Began)
                {
                    lastWorldPos = worldPos;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector3 delta = lastWorldPos - worldPos;
                    cam.transform.position += new Vector3(delta.x, 0, delta.z) * dragSpeed; // Y�� ����

                    // ī�޶� ������ �� ���ο� worldPos ���
                    Ray newRay = cam.ScreenPointToRay(touch.position);
                    if (groundPlane.Raycast(newRay, out distance))
                    {
                        lastWorldPos = newRay.GetPoint(distance);
                    }
                }
            }
        }
    }

    void HandleZoom()
    {
        if (UIInputBlocker.IsBlocking || IsTouchOverUI()) return;
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevT0 = t0.position - t0.deltaPosition;
            Vector2 prevT1 = t1.position - t1.deltaPosition;

            float prevDistance = Vector2.Distance(prevT0, prevT1);
            float currentDistance = Vector2.Distance(t0.position, t1.position);

            float delta = currentDistance - prevDistance;

            // Y�� �̵����� ����
            Vector3 pos = cam.transform.position;
            pos.y -= delta * zoomSpeed; // ���(-)�ϸ� Y�� ����, Ȯ��(+)�ϸ� Y�� ����
            pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom); // Y�� ���� ����
            cam.transform.position = pos;
        }
    }

    void ClampCameraPosition()
    {
        Vector3 pos = cam.transform.position;

        // X��� Z���� ���� ���� (3D������ X, Z�� ���� �̵�)
        float clampedX = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        float clampedZ = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);

        cam.transform.position = new Vector3(clampedX, pos.y, clampedZ);
    }
}