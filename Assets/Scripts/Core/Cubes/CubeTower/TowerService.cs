using Codice.Client.BaseCommands.BranchExplorer;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerService
{
    private TowerState _towerState;
    private readonly ITowerPlacement _placement;
    private readonly ICubePlacement _placementRules;
    private readonly ISaveService _saveService;
    private readonly IGameConfiguration _config;
    private readonly string _saveKey = "TOWER_SAVE_JSON";

    public TowerService(ITowerPlacement placement, ICubePlacement placementRules, ISaveService saveService, IGameConfiguration config)
    {
        _towerState = new TowerState();
        _placement = placement;
        _placementRules = placementRules;
        _saveService = saveService;
        _config = config;
    }

    public TowerCube? GetTopCube() => _towerState.GetTop();

    /// <summary>Попытка разместить куб в башне, возвращает результат и позицию для визуала.</summary>
    public PlaceResult TryPlaceCube(CubeModel cube, Vector2 mousePos, Vector2 size, out Vector2 dropPos)
    {
        if (_towerState.FindCubeIndex(cube.CubeId))
        {
            dropPos = Vector2.zero;
            return PlaceResult.Missed;
        }
        return _placement.TryPlace(_towerState, mousePos, cube, size, out dropPos);
    }
    public TowerCube FinalizePlacement(DraggableCube cube, Vector2 dropPos, Vector2 size)
    {
        var towerCube = new TowerCube
        {
            Id = cube.Id,
            CubeSprite = cube.CubeSprite,
            SpriteId = cube.CubeSprite.name,
            Position = dropPos,
            Size = size
        };

        _towerState.Add(towerCube);
        return towerCube;
    }

    /// <summary>Добавление куба в TowerState после успешного размещения</summary>
    public void AddCube(CubeModel cube, Vector2 position, Vector2 size)
    {
        var towerCube = new TowerCube
        {
            Id = cube.CubeId,
            CubeSprite = cube.CubeSprite,
            SpriteId = cube.CubeSprite.name,
            Position = position,
            Size = size
        };
        _towerState.Add(towerCube);
    }

    /// <summary>Проверка возможности удаления куба из башни по позиции дропа</summary>
    public bool CanRemoveCube(DraggableCube cube, RectTransform holeZone, Vector2 screenPos)
    {
        return IsPointInsideEllipse(holeZone, screenPos);
    }

    /// <summary>Удаляет куб из башни, возвращает удалённый TowerCube для анимации</summary>
    public void RemoveCube(int index)
    {
        if (index < 0 || index >= _towerState.Cubes.Count)
            throw new System.ArgumentOutOfRangeException(nameof(index));

        var cube = _towerState.GetCube(index);
        _towerState.RemoveAt(index);
    }
    /// <summary>Перестраивает башню после удаления куба</summary>
    public List<(int index, Vector2 targetPos)> GetRebuildPositions(Vector2 firstCubePos, int startIndex)
    {
        var result = new List<(int index, Vector2 targetPos)>();

        int count = _towerState.Cubes.Count;
        if (count == 0) return result;

        if (count == 1)
        {
            var cube = _towerState.GetCube(0);
            if (cube.Position.y > firstCubePos.y)
            {
                result.Add((0, firstCubePos));
            }
            return result;
        }

        for (int i = startIndex; i < count; i++)
        {
            var cube = _towerState.GetCube(i);

            float heightT = (float)i / (count - 1);
            float targetX = Mathf.Lerp(cube.Position.x, firstCubePos.x, heightT * 0.5f);
            Vector2 targetPos = new Vector2(targetX, firstCubePos.y);

            result.Add((i, targetPos));

            // Смещаем Y для следующего куба
            firstCubePos.y += cube.Size.y;
        }

        return result;
    }

    /// <summary>
    /// Применение новых позиций после анимации
    /// </summary>
    public void ApplyRebuildPositions(List<(int index, Vector2 targetPos)> rebuildData)
    {
        foreach (var item in rebuildData)
        {
            var cube = _towerState.GetCube(item.index);
            cube.Position = item.targetPos;
            _towerState.SetCubeAt(item.index, cube); // Нужен метод в TowerState
        }
    }

    /// <summary>Загрузка сохранённой башни</summary>
    public List<TowerCube> LoadTower(out Vector2 cubeSize)
    {
        cubeSize = Vector2.zero;
        var data = _saveService.Load<TowerData>(_saveKey);
        if (data == null) return null;

        cubeSize = data.size;
        _towerState = new TowerState();
        foreach (var cube in data.Cubes)
        {
            var sprite = _config.CubeColorSprites.FirstOrDefault(p => p.name == cube.SpriteName); ;
            _towerState.Add(new TowerCube
            {
                Id = cube.CubeId,
                Position = new Vector2(cube.OffsetX, cube.OffsetY),
                Size = cubeSize,
                SpriteId = cube.SpriteName,
                CubeSprite = sprite // спрайт можно получить через конфиг при визуализации
            });
        }

        return new List<TowerCube>(_towerState.Cubes);
    }

    /// <summary>Сохранение текущего состояния башни</summary>
    public void SaveTower(Vector2 cubeSize)
    {
        var data = new TowerData
        {
            Cubes = new List<CubeSaveData>(),
            size = cubeSize
        };

        foreach (var cube in _towerState.Cubes)
        {
            data.Cubes.Add(new CubeSaveData
            {
                CubeId = cube.Id,
                OffsetX = cube.Position.x,
                OffsetY = cube.Position.y,
                SpriteName = cube.CubeSprite.name
            });
        }

        _saveService.Save(data, _saveKey);
    }

    private bool IsPointInsideEllipse(RectTransform rect, Vector2 screenPoint)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, null, out localPoint);

        float a = rect.rect.width * 0.5f;
        float b = rect.rect.height * 0.5f;

        float value = (localPoint.x * localPoint.x) / (a * a) + (localPoint.y * localPoint.y) / (b * b);
        return value <= 1f;
    }
}
