using System;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.Highlight
{
    public class OutlineHandler : NetworkBehaviour
    {
        [SerializeField] private Outline _outline;
        private OutlineRegistry _outlineRegistry;

        [Inject]
        private void Construct(OutlineRegistry  outlineRegistry)
        {
            _outlineRegistry = outlineRegistry;
        }
        
        private void Start()
        {
            if (_outline)
            {
                _outlineRegistry.Register(_outline);
            }
        }
    }
}