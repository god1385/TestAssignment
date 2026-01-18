using UnityEngine;

public interface ITowerPlacement
{
    public PlaceResult TryPlace(CubeView cube, Vector2 mousePos, CubeView topCube, out Vector2 dropPos);
}
