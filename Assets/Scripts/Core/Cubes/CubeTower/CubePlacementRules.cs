using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlacementRules : ICubePlacement
{
    public bool CanCubeBePlaced(CubeModel cubeData, TowerCube? topCube) => true;
}

public class CubePlacementRulesWithColor : ICubePlacement
{
    public bool CanCubeBePlaced(CubeModel cubeData, TowerCube? topCube)
    {
        if (!topCube.HasValue) return true;

        return cubeData.CubeSprite == topCube.Value.CubeSprite;
    }
}
