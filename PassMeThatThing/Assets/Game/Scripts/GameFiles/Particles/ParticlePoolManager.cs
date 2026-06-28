using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public class ParticlePoolManager : MonoBehaviour
    {
        [SerializeField] private ParticleDatabase particleDatabase;
        
        [SerializeField] private int maxParticleCount = 10;
        private Dictionary<Particles, int> _totalParticlesCount = new ();
        private Dictionary<Particles, Stack<ParticleHandler>> _activeParticles = new ();

        public void Awake()
        {
            foreach (var item in particleDatabase.particles)
            {
                _totalParticlesCount[item.Key] = 0;
                _activeParticles[item.Key] = new Stack<ParticleHandler>();
            }
        }
        
        public void CreateParticle(Particles type)
        {
            var particlePrefab = particleDatabase.GetParticlePrefab(type);
            var particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            //NetworkServer.Spawn(particleInstance);
            var particle = particleInstance.GetComponent<ParticleHandler>();
            particle.OnParticleEnd += ReturnParticleInPool;
            
            _totalParticlesCount[type]++;
            _activeParticles[type].Push(particle);
        }
        
        public bool GetFromPool(Particles type, out ParticleHandler particle)
        {
            if (_activeParticles.ContainsKey(type))
            {
                if (_activeParticles[type].Count > 0)
                {
                    particle = _activeParticles[type].Pop();
                    return true;
                }
                if (_activeParticles[type].Count < maxParticleCount)
                {
                    CreateParticle(type);
                    particle = _activeParticles[type].Pop();
                    return true;
                }
            }

            particle = null;
            return false;
        }
        
        public void GetAndPlayParticle(Particles type, Vector3 position)
        {
            if (!GetFromPool(type, out var particle))
            {
                Debug.Log("<color=red>NO Particle");
                return;
            }
            particle.Id = type;
            particle.transform.position = position;
            particle.particleSystem.Play();
            
        }

        public void ReturnParticleInPool(Particles type, ParticleHandler particle)
        {
            _activeParticles[type].Push(particle);
        }
        
    }
}