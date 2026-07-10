using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionWheel : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image selectionElementPrefab;
    [SerializeField] private int segmentsCount = 3;
    
    private List<Image> _images = new();
    
    private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;
    private Vector2 _lastMousePos;

    private Quaternion _rotation;
    private Vector3 _direction;
    private Vector3 _pos;

    public float CenterDistance => _rectTransform.rect.height / 3;
    
    public event Action<int, int> OnValueChanged;
        
    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = transform.parent as RectTransform;

        UpdateImages();
    }

    private void UpdateImages()
    {
        _images.Clear();
        
        for (var i = 0; i < segmentsCount; i++)
        {
            _rotation = Quaternion.Euler(0f, 0f, (i+0.5f) * (360f / segmentsCount));
            _direction = _rotation * _rectTransform.right;
            _pos = _direction * CenterDistance + _rectTransform.position;
            var imageInstance = Instantiate(selectionElementPrefab, _pos, _rotation, _parentRectTransform);
            
            _images.Add(imageInstance);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastMousePos = GetMousePosRelativeToCenter(eventData);
        var angleDelta = Vector2.SignedAngle(_rectTransform.right, _lastMousePos);
        
        var positiveAngleDelta = (360 + angleDelta) % 360;
        var wheelPartIndex = (int)positiveAngleDelta / (360 / segmentsCount);
        
        //Debug.Log($"{angleDelta} {wheelPartIndex}/{segmentsCount-1}");
        OnValueChanged?.Invoke(wheelPartIndex, segmentsCount-1);
    }
        
    private Vector2 GetMousePosRelativeToCenter(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentRectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out var localPoint
        );

        return localPoint - (Vector2)_rectTransform.localPosition;
    }

    public void SetImageSprites(List<Sprite> sprites)
    {
        for (int i = 0; i < _images.Count && i < sprites.Count; i++)
        {
            _images[i].sprite = sprites[i];
        }
    }
}
