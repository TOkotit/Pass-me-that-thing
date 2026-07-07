using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu
{
    public class RebindObject : MonoBehaviour
    {
        [SerializeField] public Button button;
        [SerializeField] public TextMeshProUGUI text;
        public int BindingIndex {get;  set;}
    }
}