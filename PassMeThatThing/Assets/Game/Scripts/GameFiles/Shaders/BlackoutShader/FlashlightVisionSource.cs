using Mirror;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    [RequireComponent(typeof(Light))]
    public class FlashlightVisionSource : NetworkBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private int _shadowResolution = 512;
        [SerializeField] private LayerMask _shadowLayerMask = ~0;

        [SyncVar]
        private bool _isOn = true;

        private Camera _shadowCam;
        private RenderTexture _shadowMap;

        public Light LightSource => _light;
        public RenderTexture ShadowMap => _shadowMap;
        public Matrix4x4 WorldToLightMatrix { get; private set; }

        public bool IsActive => isActiveAndEnabled && _isOn && _light != null && _light.enabled;

        private void Awake()
        {
            if (!_light) _light = GetComponent<Light>();

            _shadowMap = new RenderTexture(_shadowResolution, _shadowResolution, 16, RenderTextureFormat.Depth)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var camGo = new GameObject("FlashlightShadowCam");
            camGo.transform.SetParent(transform, false);
            camGo.transform.localPosition = Vector3.zero;
            camGo.transform.localRotation = Quaternion.identity;

            _shadowCam = camGo.AddComponent<Camera>();
            _shadowCam.clearFlags = CameraClearFlags.Depth;
            _shadowCam.backgroundColor = Color.black;
            _shadowCam.cullingMask = _shadowLayerMask;
            _shadowCam.orthographic = false;
            _shadowCam.depthTextureMode = DepthTextureMode.Depth;
            _shadowCam.targetTexture = _shadowMap;
            _shadowCam.enabled = false; 

            var urpData = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderShadows = false;
            urpData.requiresColorOption = CameraOverrideOption.Off;
            urpData.requiresDepthOption = CameraOverrideOption.On;
        }

        private void OnEnable()
        {
            if (GlobalVisionShaderManager.Instance)
                GlobalVisionShaderManager.Instance.RegisterSource(this);

            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        private void OnDisable()
        {
            if (GlobalVisionShaderManager.Instance)
                GlobalVisionShaderManager.Instance.UnregisterSource(this);

            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        public void UpdateCameraAndMatrix()
        {
            _shadowCam.fieldOfView = _light.spotAngle;
            _shadowCam.nearClipPlane = 0.05f;
            _shadowCam.farClipPlane = _light.range;
            _shadowCam.aspect = 1f;

            _shadowCam.ResetProjectionMatrix();

            var V = _shadowCam.worldToCameraMatrix;
            var P = GL.GetGPUProjectionMatrix(_shadowCam.projectionMatrix, true);
            WorldToLightMatrix = P * V;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera.cameraType != CameraType.Game && renderingCamera.cameraType != CameraType.SceneView)
                return;

            if (renderingCamera == _shadowCam)
                return;

            if (!IsActive)
                return;

            UniversalRenderPipeline.RenderSingleCamera(context, _shadowCam);
        }

        [Command]
        public void CmdSetFlashlightOn(bool state) => _isOn = state;

        private void OnDestroy()
        {
            if (_shadowMap != null)
            {
                _shadowMap.Release();
                Destroy(_shadowMap);
            }
        }
    }
}