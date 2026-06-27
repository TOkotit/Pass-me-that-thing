using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public class ParticlePoolManager : NetworkBehaviour
    {
        [Header("Префабы партиклов")]
        [SerializedDictionary] private SerializedDictionary<Particles, GameObject> particles;
        
        [SerializeField] private int maxParticleCount = 10;
        private Dictionary<Particles, int> _totalParticlesCount = new ();
        private Dictionary<Particles, Stack<ParticleHandler>> _activeParticles = new ();

        public void Initialize()
        {
            foreach (var item in particles)
            {
                _activeParticles[item.Key] = new Stack<ParticleHandler>();
            }
        }
        
        public void CreateParticle(Particles id)
        {
            var particlePrefab = particles[id];
            var particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(particleInstance);
            var particle = particleInstance.GetComponent<ParticleHandler>();
            particle.OnParticleEnd += ReturnParticleInPool;
            
            _totalParticlesCount[id]++;
            _activeParticles[id].Push(particle);
        }
        
        public bool GetFromPool(Particles id, out ParticleHandler particle)
        {
            if (_activeParticles.ContainsKey(id))
            {
                if (_activeParticles[id].Count > 0)
                {
                    particle = _activeParticles[id].Pop();
                    return true;
                }
                if (_activeParticles[id].Count < maxParticleCount)
                {
                    CreateParticle(id);
                    particle = _activeParticles[id].Pop();
                    return true;
                }
            }

            particle = null;
            return false;
        }
        
        public void GetAndPlayParticle(Particles id, Vector3 position)
        {
            if (!GetFromPool(id, out var particle)) return;
            particle.Id = id;
            particle.transform.position = position;
            particle.particleSystem.Play();
            
        }

        public void ReturnParticleInPool(Particles id, ParticleHandler particle)
        {
            _activeParticles[id].Push(particle);
        }
        
    }
}