using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
//  ласс имеющий доступ к интерфейсу сохранений, сохран€ет и загружает данные башни кубов, использу€ сериализуемые классы TowerData и CubeSaveData.
public class TowerSaveData
{
    private ISaveService _saveService;
    private TowerData _data;
    [Inject]
    public void Construct(ISaveService saveService)
    {
        _saveService = saveService;
        _data = new TowerData();
    }


    public void Save(string key, List<CubeView> cubes, Vector2 sizeDelta)
    {
        _data.Cubes.Clear();

        foreach (var cube in cubes)
        {
            _data.Cubes.Add(new CubeSaveData
            {
                CubeId = cube.CubeToDrag.CubeSprite.name,
                OffsetX = cube.Rect.anchoredPosition.x,
                OffsetY = cube.Rect.anchoredPosition.y
            });
        }
        _data.size = sizeDelta;
        _saveService.Save(_data, key);
    }

    // «агружает данные кубов в зависимости от структуры данных поданых в generic метод Load.
    public List<CubeSaveData> LoadCubes(string key, ref Vector2 cubeViewSize)
    {
        var result = _saveService.Load<TowerData>(key);

        if (result != null)
            cubeViewSize = result.size;

        if (result != null) return result.Cubes;
        return null;
    }
}

[Serializable]
public class TowerData
{
    public List<CubeSaveData> Cubes = new();
    public Vector2 size;
}

[Serializable]
public class CubeSaveData
{
    public string CubeId;
    public float OffsetX;
    public float OffsetY;
}
