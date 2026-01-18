using UnityEngine;

// Класс для размещения кубов в башне по умолчанию, реализующий интерфейс ITowerPlacement 
public class TowerCubePlacementDefault : ITowerPlacement
{
    private ICubePlacement _cubePlacementRule;
    private readonly RectTransform _placementZone;

    public TowerCubePlacementDefault(RectTransform placementZone, ICubePlacement cubePlacementRule)
    {
        _placementZone = placementZone;
        _cubePlacementRule = cubePlacementRule;
    }

    public PlaceResult TryPlace(CubeView cube, Vector2 mousePos, CubeView topCube, out Vector2 dropPos)
    {
        if (!TryGetCubePos(mousePos, topCube, out dropPos))
            return PlaceResult.Missed;

        if (_cubePlacementRule != null && !_cubePlacementRule.CanCubeBePlaced(cube, topCube))
            return PlaceResult.Missed;

        if (dropPos.y + cube.Rect.rect.height > _placementZone.rect.height)
            return PlaceResult.HeightLimit;

        return PlaceResult.Success;
    }

    private bool TryGetCubePos(Vector2 mousePos, CubeView topCube, out Vector2 dropPosition)
    {
        float x, y;

        if (topCube == null)
        {
            dropPosition = mousePos;
            return true;
        }


        float topX = topCube.Rect.anchoredPosition.x;
        float halfWidth = topCube.Rect.rect.width / 2f;
        float topY = topCube.Rect.anchoredPosition.y;
        float halfHeight = topCube.Rect.rect.height / 2f;


        if (mousePos.x < topX - halfWidth || mousePos.x > topX + halfWidth || mousePos.y < topY + halfHeight)
        {
            dropPosition = Vector2.zero;
            return false;
        }

        y = topCube.Rect.anchoredPosition.y + topCube.Rect.rect.height;
        x = Random.Range(topCube.Rect.anchoredPosition.x - topCube.Rect.rect.width * 0.5f,
                         topCube.Rect.anchoredPosition.x + topCube.Rect.rect.width * 0.5f);

        dropPosition = new Vector2(x, y);

        return true;
    }
}

public enum PlaceResult
{
    Success,
    Missed,
    HeightLimit
}
