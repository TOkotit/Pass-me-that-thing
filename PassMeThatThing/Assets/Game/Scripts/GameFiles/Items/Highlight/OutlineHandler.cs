using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
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
                if (OutlineRegistry.Instance != null)
                    OutlineRegistry.Instance.Register(_outline);
                else
                    StartCoroutine(RegisterWhenReady());
            }
        }

        private IEnumerator RegisterWhenReady()
        {
            while (OutlineRegistry.Instance == null)
                yield return null;
            OutlineRegistry.Instance.Register(_outline);
        }
    }
}