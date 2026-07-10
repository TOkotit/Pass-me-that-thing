using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    [CreateAssetMenu(fileName = "ParticleDatabase", menuName = "Scriptable Objects/ParticleDatabase")]
    public class ParticleDatabase : ScriptableObject
    {
        [Header("Префабы партиклов")]
        [SerializedDictionary] public SerializedDictionary<Particles, ParticleHandler> particles;
        
        public ParticleHandler GetParticlePrefab(Particles type)
        {
            return particles[type];
        }
    }
}