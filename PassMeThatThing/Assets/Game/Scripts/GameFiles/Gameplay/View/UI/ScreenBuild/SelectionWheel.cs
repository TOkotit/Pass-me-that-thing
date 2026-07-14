using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionWheel : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler
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
    
    private int _hoveringElemIndex;
    private float _normalScale=1f;
    private float _selectScale=1.4f;

    public float CenterDistance => RectTransform.rect.height / 3;

    public RectTransform RectTransform => _rectTransform;

    public RectTransform ParentRectTransform => _parentRectTransform;

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
            _direction = _rotation * RectTransform.right;
            _pos = _direction * CenterDistance + RectTransform.position;
            var imageInstance = Instantiate(selectionElementPrefab, _pos, Quaternion.identity, RectTransform);
            
            _images.Add(imageInstance);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastMousePos = GetMousePosRelativeToCenter(eventData);
        var angleDelta = Vector2.SignedAngle(RectTransform.right, _lastMousePos);
        
        var positiveAngleDelta = (360 + angleDelta) % 360;
        var wheelPartIndex = (int)positiveAngleDelta / (360 / segmentsCount);
        
        //Debug.Log($"{angleDelta} {wheelPartIndex}/{segmentsCount-1}");
        OnValueChanged?.Invoke(wheelPartIndex, segmentsCount-1);
    }
        
    private Vector2 GetMousePosRelativeToCenter(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ParentRectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out var localPoint
        );

        return localPoint - (Vector2)RectTransform.localPosition;
    }

    public void SetImageSprites(List<Sprite> sprites)
    {
        for (int i = 0; i < _images.Count && i < sprites.Count; i++)
        {
            _images[i].sprite = sprites[i];
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        var hoveringMousePos = GetMousePosRelativeToCenter(eventData);
        var angleDelta = Vector2.SignedAngle(RectTransform.right, hoveringMousePos);
        
        var positiveAngleDelta = (360 + angleDelta) % 360;
        var wheelPartIndex = (int)positiveAngleDelta / (360 / segmentsCount);

        if (_hoveringElemIndex != wheelPartIndex)
        {
            _images[_hoveringElemIndex].transform.DOScale(_normalScale, 0.2f)
                .From(_selectScale).SetEase(Ease.OutQuad);
            _hoveringElemIndex = wheelPartIndex;
            _images[_hoveringElemIndex].transform.DOScale(_selectScale, 0.2f)
                .From(_normalScale).SetEase(Ease.OutQuad);
        }
    }
}
