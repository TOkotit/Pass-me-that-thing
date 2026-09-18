using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    [CreateAssetMenu(fileName = "RootNetworkConfig", menuName = "Scriptable Objects/RootNetworkConfig")]
    public class RootNetworkConfig : ScriptableObject
    {
        [SerializeField] private bool isSteamUsing;

        public bool IsSteamUsing => isSteamUsing;
    }
}