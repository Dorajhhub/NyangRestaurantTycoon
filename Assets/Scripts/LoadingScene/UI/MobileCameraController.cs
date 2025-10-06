// 2025-09-16 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCameraController : MonoBehaviour
{
    public float dragSpeed = 5f; // 드래그 속도
    public float minZoom = 3f; // 최소 줌
    public float maxZoom = 50f; // 최대 줌
    public float zoomSpeed = 1f; // 줌 속도

    public Vector2 minBounds; // 카메라 이동 최소 범위
    public Vector2 maxBounds; // 카메라 이동 최대 범위

    private Camera cam;
    private Vector3 lastWorldPos;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera가 설정되지 않았습니다. 카메라에 'MainCamera' 태그를 추가하세요.");
        }
    }

    void Update()
    {
        // UI 패널/스크롤 중이면 카메라 입력 차단
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

        // 모바일 터치 검사
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
            // 에디터/PC 마우스 검사
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

            // 터치가 화면 범위 내에 있는지 확인
            if (touch.position.x < 0 || touch.position.x > Screen.width ||
                touch.position.y < 0 || touch.position.y > Screen.height)
            {
                return; // 화면 범위를 벗어나면 처리하지 않음
            }

            Ray ray = cam.ScreenPointToRay(touch.position);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // y=0 평면
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
                    cam.transform.position += new Vector3(delta.x, 0, delta.z) * dragSpeed; // Y축 고정

                    // 카메라가 움직인 후 새로운 worldPos 계산
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

            // Y축 이동으로 변경
            Vector3 pos = cam.transform.position;
            pos.y -= delta * zoomSpeed; // 축소(-)하면 Y가 증가, 확대(+)하면 Y가 감소
            pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom); // Y축 범위 제한
            cam.transform.position = pos;
        }
    }

    void ClampCameraPosition()
    {
        Vector3 pos = cam.transform.position;

        // X축과 Z축을 각각 제한 (3D에서는 X, Z가 수평 이동)
        float clampedX = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        float clampedZ = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);

        cam.transform.position = new Vector3(clampedX, pos.y, clampedZ);
    }
}