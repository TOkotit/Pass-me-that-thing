using Mirror;
using UnityEngine;

public class NetworkOutlineTarget : NetworkBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineWidth = 0.05f;

    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int EnabledId = Shader.PropertyToID("_Enabled");

    private Material[][] _originalMaterials;
    private Material[][] _outlinedMaterials;

    [SyncVar(hook = nameof(OnStateChanged))]
    private bool _enabled;

    private void Awake()
    {
        Prepare();
    }

    private void Prepare()
    {
        _originalMaterials = new Material[renderers.Length][];
        _outlinedMaterials = new Material[renderers.Length][];

        for (int r = 0; r < renderers.Length; r++)
        {
            var mats = renderers[r].sharedMaterials;

            _originalMaterials[r] = mats;

            var outlined = new Material[mats.Length + 1];

            for (int i = 0; i < mats.Length; i++)
                outlined[i] = mats[i];

            var outMat = new Material(outlineMaterial);
            outMat.SetFloat(OutlineWidthId, outlineWidth);
            outMat.SetFloat(EnabledId, 1f);

            outlined[outlined.Length - 1] = outMat;

            _outlinedMaterials[r] = outlined;
        }
    }

    [Server]
    public void SetOutlined(bool state)
    {
        _enabled = state;
    }

    private void OnStateChanged(bool oldValue, bool newValue)
    {
        Apply(newValue);
    }

    private void Apply(bool enabled)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials =
                enabled
                    ? _outlinedMaterials[i]
                    : _originalMaterials[i];
        }
    }
}