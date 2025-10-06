using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public Slider slider;

    public void SetProgress(float progress)
    {
        slider.value = Mathf.Clamp01(progress);
    }
}
