using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class TowerView : MonoBehaviour, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform zoneToPlaceCubes;
    [SerializeField] private RectTransform holeZone;
    [SerializeField] private CubeView prefabToSpawnForTower;

    [Header("Animation Settings")]
    [SerializeField] private float cubeSpawnHeight = 50f;
    [SerializeField] private float cubeFallDuration = 1f;

    private TowerService _towerService;
    private Vector2 cubeViewSize = Vector2.zero;
    private GameEventsHandler _gameEvents;
    private List<CubeView> _cubeViews = new List<CubeView>();

    [Inject] private DiContainer _container;

    [Inject]
    public void Construct(GameEventsHandler events, TowerService towerService)
    {
        _gameEvents = events;
        _towerService = towerService;
    }

    private void Start()
    {
        var towerData = _towerService.LoadTower(out cubeViewSize);
        if (towerData == null) return;

        // Создаём визуал
        foreach (var cube in towerData)
        {
            SpawnCubeView(cube);
        }
    }

    // Основной обработчик сброса куба в зону башни
    public void OnDrop(PointerEventData eventData)
    {
        var draggedCube = eventData.pointerDrag?.GetComponent<DraggableCube>();

        if (draggedCube == null) return;

        if (cubeViewSize == Vector2.zero || _towerService.GetTopCube() == null)
        {
            cubeViewSize = draggedCube.GetComponent<RectTransform>().rect.size;
        }
        // Создаём визуальный куб
        var cubeView = SpawnTowerCube(draggedCube);

        // Получаем позицию дропа мыши
        Vector2 mousePos = GetMouseDropPosition(eventData, cubeView.Rect, zoneToPlaceCubes);
        cubeView.Rect.anchoredPosition = mousePos;

        // Пытаемся разместить куб через TowerService
        var cubeModel = new CubeModel { CubeSprite = draggedCube.CubeSprite, CubeId = draggedCube.Id };
        var result = _towerService.TryPlaceCube(cubeModel, mousePos, cubeViewSize, out Vector2 dropPos);

        switch (result)
        {
            case PlaceResult.Success:
                // Анимация падения куба
                TowerAnimations.AnimateDrop(cubeView, dropPos, cubeSpawnHeight, cubeFallDuration)
                    .OnComplete(() => _towerService.SaveTower(cubeViewSize));

                cubeView.DraggableCube.AssignCubeToTower(this, cubeView);
                _towerService.FinalizePlacement(cubeView.DraggableCube, dropPos, cubeViewSize);
                _cubeViews.Add(cubeView);
                _gameEvents.textEvent.OnNext(GameEventType.CubePlaced);
                break;

            case PlaceResult.Missed:
                RejectCube(cubeView, GameEventType.CubeMissed);
                break;

            case PlaceResult.HeightLimit:
                RejectCube(cubeView, GameEventType.HeightLimitReached);
                break;
        }
    }

    private CubeView SpawnTowerCube(DraggableCube dragged)
    {
        var cube = _container.InstantiatePrefabForComponent<CubeView>(prefabToSpawnForTower, zoneToPlaceCubes);
        cube.Initialize(dragged.CubeSprite, zoneToPlaceCubes, null);
        AdaptCubeSize(cube.Rect);
        return cube;
    }

    private void SpawnCubeView(TowerCube cubeData)
    {
        var cube = _container.InstantiatePrefabForComponent<CubeView>(prefabToSpawnForTower, zoneToPlaceCubes);
        cube.Initialize(cubeData.CubeSprite, zoneToPlaceCubes, null);
        AdaptCubeSize(cube.Rect);
        cube.DraggableCube.AssignCubeToTower(this, cube, cubeData.Id);
        _cubeViews.Add(cube);

        cube.Rect.anchoredPosition = cubeData.Position;
        cube.Rect.sizeDelta = cubeData.Size;
    }

    private void RejectCube(CubeView cube, GameEventType eventType)
    {
        _gameEvents.textEvent.OnNext(eventType);
        TowerAnimations.FadeAndDestroy(cube);
    }

    public void RemoveCube(DraggableCube draggableCube, PointerEventData eventData)
    {
        if (!_towerService.CanRemoveCube(draggableCube, holeZone, eventData.position))
        {
            _gameEvents.textEvent.OnNext(GameEventType.CubeMissed);
            return;
        }


        var cubeView = draggableCube.AssignedCubeView;
        int index = _cubeViews.IndexOf(cubeView);

        if (index < 0) return;

        Vector2 firstPos = cubeView.Rect.anchoredPosition;

        _towerService.RemoveCube(index); // удаляем из TowerState
        _cubeViews.RemoveAt(index);

        Vector2 mousePos = GetMouseDropPosition(eventData, cubeView.Rect, holeZone);
        SpawnFallingCube(cubeView, mousePos);
        TowerAnimations.FadeAndDestroy(cubeView);
        _gameEvents.textEvent.OnNext(GameEventType.CubeThrown);

        // Перестраиваем башню визуально
        var rebuildData = _towerService.GetRebuildPositions(firstPos, index);
        Sequence seq = DOTween.Sequence();
        foreach (var item in rebuildData)
        {
            var cv = _cubeViews[item.index];
            seq.Join(cv.Rect.DOAnchorPos(item.targetPos, 0.3f).SetEase(Ease.OutQuad));
        }
        seq.OnComplete(() =>
        {
            _towerService.ApplyRebuildPositions(rebuildData);
            _towerService.SaveTower(cubeViewSize);

        });
    }

    // создаем копию куба, который просто будет падать, чтобы создать эффект выброса из башни и дать возможность создать анимцию исчезновения в самой башне
    private void SpawnFallingCube(CubeView sourceCube, Vector2 screenMousePos)
    {
        var fallingCube = _container.InstantiatePrefabForComponent<CubeView>(
            prefabToSpawnForTower,
            holeZone
        );

        fallingCube.Initialize(sourceCube.CubeSprite, holeZone, null);

        fallingCube.Rect.anchoredPosition = screenMousePos;
        fallingCube.Rect.sizeDelta = cubeViewSize;

        TowerAnimations.AnimateFallIntoHole(fallingCube, 1f);
    }

    // Получаем позицию дропа куба (координаты мыши) относительно зоны башни
    private Vector2 GetMouseDropPosition(PointerEventData eventData, RectTransform cubeRect, RectTransform zoneToDrop)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            zoneToDrop,
            eventData.position,
            null,
            out localPoint
        );

        return new Vector2(
            localPoint.x + zoneToDrop.rect.width / 2,
            localPoint.y + zoneToDrop.rect.height * zoneToDrop.pivot.y - cubeRect.rect.height / 2f
        );
    }

    // Адаптируем размер куба в башне к сохраненному размеру
    private void AdaptCubeSize(RectTransform cubeRect)
    {
        cubeRect.pivot = new Vector2(0.5f, 0f);
        cubeRect.anchorMin = cubeRect.anchorMax = new Vector2(0.5f, 0f);

        cubeRect.sizeDelta = cubeViewSize;
    }
}

