using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public class Tracer : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] private string visibilityProperty = "_Dissolve";
        [SerializeField] private float visibilityTime;
        private float _visibility;
        
        private void Awake()
        {
            _visibility = 1;
        }

        private void Update()
        {
            if (_visibility > 0)
            {
                _visibility -= (Time.deltaTime / visibilityTime);
            }
            _visibility = Mathf.Clamp(_visibility, 0, 1);
            if (lineRenderer)
                lineRenderer.material.SetFloat(visibilityProperty,1 - _visibility);
        }
    }
}