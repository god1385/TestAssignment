using UnityEngine;

public interface ITowerPlacement
{
    public PlaceResult TryPlace(TowerState state, Vector2 mousePos, CubeModel cube, Vector2 cubeSize, out Vector2 dropPos);
}
