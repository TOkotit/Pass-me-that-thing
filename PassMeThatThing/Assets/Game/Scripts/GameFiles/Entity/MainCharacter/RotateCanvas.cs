using UnityEngine;
using VContainer;

namespace MainCharacter
{
    public class RotateCanvas : MonoBehaviour
    {
        [SerializeField] public Transform _object;
        [SerializeField] public Vector3 offset;

        [Inject] private MainCharacterCamera mainCamera;
        [Inject] [Key("WorldCanvas")] private GameObject _canvas;
        
        
        void Start()
        {
            Debug.Log($"{mainCamera is null} {_canvas is null}");
            _object = transform.parent;
            transform.SetParent(_canvas.transform);
        }
        void Update()
        {
            if (!_object)
            {
                Destroy(this.gameObject);
                return;
            }
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            transform.position = _object.position + offset;
        }
    }
}