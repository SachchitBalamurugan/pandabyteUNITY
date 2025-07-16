using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingView : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.value = value;

        if (progressText != null)
            progressText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
