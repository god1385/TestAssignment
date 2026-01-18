using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlacementRules : ICubePlacement
{
    public bool CanCubeBePlaced(CubeView currentCube, CubeView topCube) => true;
}

public class CubePlacementRulesWithColor : ICubePlacement
{
    public bool CanCubeBePlaced(CubeView currentCube, CubeView topCube)
    {
        if (topCube == null) return true;

        return currentCube.CubeSprite == topCube.CubeSprite;
    }
}
