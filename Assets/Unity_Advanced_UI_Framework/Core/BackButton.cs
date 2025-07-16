using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    private Button _button;

    [Inject] private UIManager _uiManager;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnBackPressed);
    }

    private void OnBackPressed()
    {
        _uiManager.GoBackAsync().Forget();
    }
}
