using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Основной срипт для перетаскивания куба и возможностью заблокировать перетаскивания
public class DraggableCube : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image image;

    private Transform _initialParent;
    private ScrollRect _scroll;
    private Vector2 _startPos;
    private RectTransform _dragParent;
    private GameEventsHandler _gameEventsHandler;
    private TowerView _assignedTower;
    private int _interactionLocks;
    private string _id;
    private CubeView assignedCubeView;

    public Sprite CubeSprite => image.sprite;
    public bool CanInteract => _interactionLocks == 0;
    public string Id => _id;
    public CubeView AssignedCubeView => assignedCubeView;

    public void LockInteraction()
    {
        _interactionLocks++;
    }

    public void UnlockInteraction()
    {
        _interactionLocks = Mathf.Max(0, _interactionLocks - 1);
    }

    public void Construct(GameEventsHandler events)
    {
        _gameEventsHandler = events;
    }
    public void Initialize(Sprite sprite, Transform parent, RectTransform dragParent, ScrollRect scroll = null)
    {
        image.sprite = sprite;
        _initialParent = parent;

        if (scroll)
            _scroll = scroll;
        _dragParent = dragParent;
    }

    public void DroppedInHole(PointerEventData eventData)
    {
        if (_assignedTower == null) return;

        _assignedTower.RemoveCube(this, eventData);
    }

    public void AssignCubeToTower(TowerView tower, CubeView assignedView, string id = null)
    {
        _assignedTower = tower;
        assignedCubeView = assignedView;

        if (id == null)
            _id = Guid.NewGuid().ToString();
        else
            _id = id;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanInteract)
        {
            eventData.pointerDrag = null;
            return;
        }

        _startPos = rect.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        if (_scroll) _scroll.enabled = false;
        transform.SetParent(_dragParent);
    }

    // тянем относительно родителя
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragParent.transform as RectTransform, eventData.position, null, out localPoint);

        rect.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (_scroll) _scroll.enabled = true;
        transform.SetParent(_initialParent);
        rect.anchoredPosition = _startPos;
    }
}
