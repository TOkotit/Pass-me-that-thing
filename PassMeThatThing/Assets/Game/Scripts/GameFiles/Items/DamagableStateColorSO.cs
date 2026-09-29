using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    [CreateAssetMenu(fileName = "DamageableStateColorData", menuName = "Scriptable Objects/DamageableStateColorData")]
    public class DamageableStateColorData : ScriptableObject
    {
       [SerializedDictionary] public SerializedDictionary<StateSource, Color> colors; 
    }
}