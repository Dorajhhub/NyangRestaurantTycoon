using UnityEngine;
using UnityEngine.EventSystems;

// ScrollView 영역에서 드래그/스크롤 시 카메라 입력을 차단
public class ScrollViewInputGuard : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
  public void OnBeginDrag(PointerEventData eventData)
  {
    UIInputBlocker.IsBlocking = true;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    UIInputBlocker.IsBlocking = false;
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    UIInputBlocker.IsBlocking = true;
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    UIInputBlocker.IsBlocking = false;
  }
}


