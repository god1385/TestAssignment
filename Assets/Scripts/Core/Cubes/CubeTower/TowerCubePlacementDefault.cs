using UnityEngine;

// Класс для размещения кубов в башне по умолчанию, реализующий интерфейс ITowerPlacement 
public class TowerCubePlacementDefault : ITowerPlacement
{
    private readonly ICubePlacement _cubePlacementRule;
    private readonly float _maxHeight;

    public TowerCubePlacementDefault(RectTransform zoneToPlaceCubes, ICubePlacement cubePlacementRule)
    {
        _maxHeight = zoneToPlaceCubes.rect.height;
        _cubePlacementRule = cubePlacementRule;
    }

    public PlaceResult TryPlace(TowerState state, Vector2 mousePos, CubeModel cube, Vector2 cubeSize, out Vector2 dropPos)
    {
        dropPos = Vector2.zero;

        // Берём верхний куб башни (nullable)
        TowerCube? top = state.Count > 0 ? state.GetTop() : (TowerCube?)null;

        if (top.HasValue)
        {
            float topX = top.Value.Position.x;
            float topY = top.Value.Position.y;
            float halfWidth = top.Value.Size.x / 2f;
            float halfHeight = top.Value.Size.y / 2f;

            // Если мышь вышла за границы верхнего куба по X или ниже верхнего по Y — промах
            if (mousePos.x < topX - halfWidth || mousePos.x > topX + halfWidth || mousePos.y < topY + halfHeight)
                return PlaceResult.Missed;
        }

        // Проверяем правила размещения
        if (_cubePlacementRule != null)
        {
            if (!_cubePlacementRule.CanCubeBePlaced(cube, top))
                return PlaceResult.Missed;
        }

        // Рассчитываем позицию по Y с учётом размера куба
        float targetY = top.HasValue ? top.Value.Position.y + top.Value.Size.y : mousePos.y;

        // Проверяем ограничение по высоте
        if (targetY + cubeSize.y > _maxHeight)
            return PlaceResult.HeightLimit;

        // Случайное смещение по X относительно верхнего куба
        float targetX = mousePos.x;
        if (top.HasValue)
        {
            targetX = Random.Range(
                top.Value.Position.x - top.Value.Size.x * 0.5f,
                top.Value.Position.x + top.Value.Size.x * 0.5f
            );
        }

        dropPos = new Vector2(targetX, targetY);
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
