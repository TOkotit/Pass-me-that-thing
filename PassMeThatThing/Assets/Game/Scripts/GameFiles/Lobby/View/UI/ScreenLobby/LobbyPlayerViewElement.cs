using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class LobbyPlayerViewElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nickname;
        [SerializeField] private TextMeshProUGUI readyStatus;
        [SerializeField] private Image playerProfilePic;
    }
}