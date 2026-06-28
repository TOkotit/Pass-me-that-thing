using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    [CreateAssetMenu(fileName = "ParticleDatabase", menuName = "Scriptable Objects/ParticleDatabase")]
    public class ParticleDatabase : ScriptableObject
    {
        [Header("Префабы партиклов")]
        [SerializedDictionary] public SerializedDictionary<Particles, GameObject> particles;
        
        public GameObject GetParticlePrefab(Particles type)
        {
            return particles[type];
        }
    }
}