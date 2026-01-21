using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Класс для хранения данных о кубах в башне.
public class TowerState
{
    private readonly List<TowerCube> _cubes = new();

    public IReadOnlyList<TowerCube> Cubes => _cubes;
    public int Count => _cubes.Count;

    public void Add(TowerCube cube)
    {
        _cubes.Add(cube);
    }

    public void RemoveAt(int index)
    {
        _cubes.RemoveAt(index);
    }

    public TowerCube? GetTop()
    {
        return _cubes.Count == 0 ? null : _cubes[^1];
    }

    public float GetTotalHeight()
    {
        float height = 0f;
        foreach (var cube in _cubes)
            height += cube.Size.y;
        return height;
    }

    public void SetCubeAt(int index, TowerCube cube)
    {
        if (index >= 0 && index < _cubes.Count)
            _cubes[index] = cube;
    }

    public TowerCube GetCube(int index) => _cubes[index];
    public bool FindCubeIndex(string id)
    {
        for (int i = 0; i < _cubes.Count; i++)
        {
            if (_cubes[i].Id == id)
                return true;
        }
        return false;
    }

}

public struct TowerCube
{
    public string Id;
    public Vector2 Position;
    public Vector2 Size;
    public Sprite CubeSprite;
    public string SpriteId;
}

