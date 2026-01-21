using UnityEngine;

public interface ICubePlacement
{
    bool CanCubeBePlaced(CubeModel cubeData, TowerCube? topCube);
}

public class CubeModel
{
    public Sprite CubeSprite;
    public string CubeId;
}
