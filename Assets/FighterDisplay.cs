using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FighterDisplay : MonoBehaviour 
{
    [SerializeField] GameObject[] hovers;

    [SerializeField] TextMeshProUGUI nameDisplay;
    [SerializeField] TextMeshProUGUI healthDisplay;
    [SerializeField] Slider healthbar;


    public FighterData Fighter { get; private set; }

    public void UpdateDisplay()
    {
        nameDisplay.text = Fighter.name;
        healthDisplay.text = $"{Fighter.CurrentHp}/{Fighter.Hp}";
    }

    public void Setup(FighterData fighter)
    {
        Fighter = fighter;
    }

    public void Hover(bool active)
    {
        foreach (var item in hovers)
        {
            item.SetActive(active);
        }
    }
}