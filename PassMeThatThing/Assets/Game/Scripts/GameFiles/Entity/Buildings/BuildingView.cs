using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> meshRenderers;

        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.6f;
        [SerializeField] private int flashCount = 3;

        private MaterialPropertyBlock propBlock;
        private Coroutine flashRoutine;

        private void Awake()
        {
            propBlock = new();
        }

        public void TakeDamage()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < flashCount; i++)
            {
                SetColor(flashColor, 0.5f);
                yield return new WaitForSeconds(flashDuration);

                SetColor(Color.black, 1f);
                yield return new WaitForSeconds(flashDuration);
            }
        }

        private void SetColor(Color color, float intensity)
        {
            //Debug.Log($"[BView] SetColor {color}");
            foreach (var renderer in meshRenderers)
            {
                propBlock.SetColor("_EmissionColor", color * intensity);

                renderer.SetPropertyBlock(propBlock);
            }
        }

    }
}