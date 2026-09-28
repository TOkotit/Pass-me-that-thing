using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    [CreateAssetMenu(fileName = "DamageableStateColorSO", menuName = "Scriptable Objects/DamageableStateColorSO")]
    public class DamageableStateColorSO : ScriptableObject
    {
       [SerializedDictionary] public SerializedDictionary<StateSource, Color> colors; 
    }
}