using UnityEngine;

// 전역 UI 입력 차단 플래그
public static class UIInputBlocker
{
  // UI 상호작용 중이면 true (카메라 이동/줌 등 입력 차단 용도)
  public static bool IsBlocking = false;
}


