using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription;
using Game.Gameplay.View.UI;
using Game.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy.View
{
    public class EnemyView : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected NetworkAnimator netAnimator;

        [Header("Particles")]
        [SerializeField] protected ParticleSystem particles;

        [Header("AttackPreview")]
        [SerializeField] protected GameObject attackPreviewSphere;

        [Header("Mesh")]
        [SerializeField] private bool isSkinnedMeshRenderer;
        [SerializeField] private List<MeshRenderer> meshRenderers;
        [SerializeField] private List<SkinnedMeshRenderer> skinnedMeshRenderers;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private Color healColor = Color.greenYellow;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private int flashCount = 2;

        [Inject] private GameplayUIManager _gameplayUIManager;

        private WindowEnemyViewViewModel _windowViewModel;

        private MaterialPropertyBlock propBlock;
        private Coroutine flashRoutine;


        private void Awake()
        {
            propBlock = new();
        }

        public void InitUI(Enemy enemy)
        {
            _windowViewModel = _gameplayUIManager.OpenWindowEnemyView(enemy);
        }

        private void OnDestroy()
        {
            _gameplayUIManager.CloseWindowEnemyView(_windowViewModel);
        }

        public void EnableAnimator() => animator.enabled = true;
        public void DisableAnimator() => animator.enabled = false;
        
        public void PlayParticles() => particles.Play();

        public void EnableAttackpreview(bool enabled)
        {
            attackPreviewSphere.SetActive(enabled);
        }

        public void SetAttackpreview(Vector3 pos, Vector3 size)
        {
            attackPreviewSphere.transform.position = pos;
            attackPreviewSphere.transform.localScale = size;
        }

        public void TakeDamage()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashRoutine(damageColor));
        }

        public void Heal()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashRoutine(healColor));
        }

        private IEnumerator FlashRoutine(Color flashColor)
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
            if (isSkinnedMeshRenderer)
            {
                foreach (var renderer in skinnedMeshRenderers)
                {
                    propBlock.SetColor("_EmissionColor", color * intensity);

                    renderer.SetPropertyBlock(propBlock);
                }
            }
            else
            {
                foreach (var renderer in meshRenderers)
                {
                    propBlock.SetColor("_EmissionColor", color * intensity);

                    renderer.SetPropertyBlock(propBlock);
                }
            }
            
        }
    }
}