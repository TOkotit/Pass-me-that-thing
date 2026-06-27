using System;
using Game.Scripts.GameFiles.Items;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    public ParticleSystem particleSystem;
    
    public Particles Id {get; set;}
    
    public event Action<Particles, ParticleHandler> OnParticleEnd;
    private void OnParticleSystemStopped()
    {
        OnParticleEnd?.Invoke(Id, this);
    }
}
