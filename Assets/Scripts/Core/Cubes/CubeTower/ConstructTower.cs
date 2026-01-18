using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

// Управляет размещением кубов в башне:
// обработка дропа Кубов
// Проверка правил размещения Кубов
// запуск анимаций
// сохранение состояния после установки или выброса Кубов
public class ConstructTower : MonoBehaviour, IDropHandler
{
    [SerializeField] private RectTransform zoneToPlaceCubes;
    [SerializeField] private RectTransform holeZone;
    [SerializeField] private float cubeSpawnHeight = 50f;
    [SerializeField] private float cubeFallDuration = 1f;
    [SerializeField] private CubeView prefabToSpawnForTower;

    private GameEventsHandler _gameEvents;
    private TowerCubeData _towerData;
    private ITowerPlacement _towerPlacement;
    private Vector2 cubeViewSize = Vector2.zero;
    private TowerSaveData _saveData;
    private IGameConfiguration _config;
    private static string SaveKey => "TOWER_SAVE_JSON";
    private bool IsHeightLimit(Vector2 point, CubeView cube) => point.y + cube.Rect.rect.height > zoneToPlaceCubes.rect.height;

    [Inject] private DiContainer _container;

    [Inject]
    public void Construct(GameEventsHandler events, ICubePlacement placementRules, ISaveService saveService, IGameConfiguration gameConfig)
    {
        _gameEvents = events;
        _towerPlacement = new TowerCubePlacementDefault(zoneToPlaceCubes, placementRules);
        _saveData = new TowerSaveData();
        _saveData.Construct(saveService);
        _config = gameConfig;
    }

    private void Awake()
    {
        _towerData = new TowerCubeData();
    }

    private void Start()
    {
        var data = _saveData.LoadCubes(SaveKey, ref cubeViewSize);
        if (data == null || data.Count == 0) return;

        ReloadData(data);
    }
    //Обновляет состояние башни из сохраненных данных и восстанавливает расположение кубов
    private void ReloadData(List<CubeSaveData> data)
    {
        foreach (var cubeData in data)
        {
            var sprite = _config.CubeColorSprites.Find(s => s.name == cubeData.CubeId);

            if (sprite == null) continue;


            var cubeView = _container.InstantiatePrefabForComponent<CubeView>(prefabToSpawnForTower, zoneToPlaceCubes);
            cubeView.Initialize(sprite, zoneToPlaceCubes, null);
            AdaptCubeSize(cubeView.Rect);

            Vector2 dropPos = new Vector2(cubeData.OffsetX, cubeData.OffsetY);

            cubeView.Rect.anchoredPosition = new Vector2(cubeData.OffsetX, cubeData.OffsetY);
            cubeView.Rect.sizeDelta = cubeViewSize;
            TowerAnimations.AnimateDrop(cubeView, dropPos, cubeSpawnHeight, cubeFallDuration);

            _towerData.AddCube(cubeView);
            cubeView.CubeToDrag.AssignCubeToTower(this);
        }
    }

    // Основной обработчик сброса куба в зону башни
    public void OnDrop(PointerEventData eventData)
    {
        var draggedCube = eventData.pointerDrag?.GetComponent<CubeToDrag>();
        if (!_towerData.CanAcceptCube(draggedCube)) return;

        if (cubeViewSize == Vector2.zero) cubeViewSize = draggedCube.GetComponent<RectTransform>().rect.size;

        var cube = SpawnTowerCube(draggedCube);
        Vector2 mousePos = GetMouseDropPosition(eventData, cube.Rect, zoneToPlaceCubes);
        cube.Rect.anchoredPosition = mousePos;

        // Пытаемся разместить куб в башне согласно правилам размещения и возвращаем Enum, описывающий результат попытки
        var result = _towerPlacement.TryPlace(cube, mousePos, _towerData.TopCube, out Vector2 dropPos);

        switch (result)
        {
            case PlaceResult.Success:
                var tween = TowerAnimations.AnimateDrop(cube, dropPos, cubeSpawnHeight, cubeFallDuration);
                tween.OnComplete(() =>
                {
                    _saveData.Save(SaveKey, _towerData.Cubes, cubeViewSize);
                });
                FinalizePlacement(cube);
                break;
            case PlaceResult.Missed:
                RejectCube(cube, GameEventType.CubeMissed);
                break;
            case PlaceResult.HeightLimit:
                RejectCube(cube, GameEventType.HeightLimitReached);
                break;
        }
    }

    private void FinalizePlacement(CubeView cube)
    {
        _towerData.AddCube(cube);
        cube.CubeToDrag.AssignCubeToTower(this);

        // Оповещаем систему событий об успешном размещении куба
        _gameEvents.textEvent.OnNext(GameEventType.CubePlaced);
    }

    private void RejectCube(CubeView cube, GameEventType eventType)
    {
        _gameEvents.textEvent.OnNext(eventType);
        TowerAnimations.FadeAndDestroy(cube);
    }

    private CubeView SpawnTowerCube(CubeToDrag dragged)
    {
        var cube = _container.InstantiatePrefabForComponent<CubeView>(prefabToSpawnForTower, zoneToPlaceCubes);

        cube.Initialize(dragged.CubeSprite, zoneToPlaceCubes, null);
        AdaptCubeSize(cube.Rect);

        return cube;
    }

    public void RemoveCube(CubeToDrag cubeToDrag, PointerEventData eventData)
    {
        CubeView cube = _towerData.FindCubeView(cubeToDrag);
        if (cube == null) return;

        // Проверяем, что точка дропа находится внутри зоны "дыры"
        if (IsPointInsideEllipse(holeZone, eventData.position))
        {

            Vector2 mousePos = GetMouseDropPosition(eventData, cube.Rect, holeZone);
            int startIndex = _towerData.CubeIndex(cube);
            Vector2 firstCubeBaseos = _towerData.RequiredCubeAnchoredPose(startIndex);
            SpawnFallingCube(cube, mousePos);
            _towerData.RemoveCUbe(cube);

            _gameEvents.textEvent.OnNext(GameEventType.CubeThrown);

            TowerAnimations.FadeAndDestroy(cube);

            //После удаления куба из башни, перестраиваем оставшиеся кубы
            var rebuildTween = RebuildTower(firstCubeBaseos, startIndex);

            if (rebuildTween != null)
            {
                rebuildTween.OnComplete(() =>
                {
                    _saveData.Save(SaveKey, _towerData.Cubes, cubeViewSize);
                });
            }
            else
            {
                _saveData.Save(SaveKey, _towerData.Cubes, cubeViewSize);
            }
        }
        else
            _gameEvents.textEvent.OnNext(GameEventType.CubeMissed);
    }

    private bool IsPointInsideEllipse(RectTransform holeRect, Vector2 screenPoint)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            holeRect,
            screenPoint,
            null,
            out localPoint
        );

        float a = holeRect.rect.width * 0.5f;
        float b = holeRect.rect.height * 0.5f;

        // Проверка уравнения эллипса, значение больше 1 означает, что точка вне эллипса
        float value = (localPoint.x * localPoint.x) / (a * a) + (localPoint.y * localPoint.y) / (b * b);

        return value <= 1f;
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


    // Смещаем кубы, которые выше удаленного куба, чтобы заполнить пустоту
    private Tween RebuildTower(Vector2 firstCubePos, int startIndex)
    {

        if (_towerData.GetCubesCount == 0) return null;

        Sequence seq = DOTween.Sequence();


        if (_towerData.GetCubesCount == 1)
        {
            var cube = _towerData.GetCube(0);

            if (cube.Rect.anchoredPosition.y > firstCubePos.y)
            {
                seq.Append(cube.Rect
                    .DOAnchorPos(new Vector2(firstCubePos.x, firstCubePos.y), 0.25f)
                    .SetEase(Ease.OutQuad));
            }

            return seq;
        }


        for (int i = startIndex; i < _towerData.GetCubesCount; i++)
        {
            var cube = _towerData.GetCube(i);
            cube.Rect.DOKill();

            float heightT = (float)i / (_towerData.GetCubesCount - 1);
            float targetX = Mathf.Lerp(cube.Rect.anchoredPosition.x, firstCubePos.x, heightT * 0.5f);

            Vector2 targetPos = new Vector2(targetX, firstCubePos.y);

            seq.Join(cube.Rect
                .DOAnchorPos(targetPos, 0.3f)
                .SetEase(Ease.OutQuad));

            firstCubePos.y += cube.Rect.rect.height;
        }

        return seq;
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

