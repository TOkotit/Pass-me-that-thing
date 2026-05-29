using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEventUIElement : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    public Image Icon
    {
        get => icon;
        set => icon = value;
    }

    public TextMeshProUGUI Text
    {
        get => text;
        set => text = value;
    }
}
