using System.Collections.Generic;
using UnityEngine;

// Класс для хранения данных о кубах в башне.
public class TowerCubeData
{
    private List<CubeView> cubesToDrag = new();
    private HashSet<CubeToDrag> cubesInTower = new();

    public CubeView TopCube => cubesToDrag.Count > 0 ? cubesToDrag[^1] : null;
    public CubeView FindCubeView(CubeToDrag cubeToDrag) => cubesToDrag.Find(c => c.CubeToDrag == cubeToDrag);
    public bool CanAcceptCube(CubeToDrag dragged) => dragged != null && !cubesInTower.Contains(dragged);
    public int CubeIndex(CubeView cube) => cubesToDrag.IndexOf(cube);
    public CubeView GetCube(int index) => cubesToDrag[index];
    public int GetCubesCount => cubesToDrag.Count;
    public List<CubeView> Cubes => cubesToDrag;
    public Vector2 RequiredCubeAnchoredPose(int index)
    {
        if (index >= cubesToDrag.Count || index < 0) return Vector2.zero;

        return cubesToDrag[index].GetComponent<RectTransform>().anchoredPosition;
    }

    public void AddCube(CubeView cube)
    {
        cubesToDrag.Add(cube);
        cubesInTower.Add(cube.CubeToDrag);
    }

    public void RemoveCUbe(CubeView cube)
    {
        cubesToDrag.Remove(cube);
        cubesInTower.Remove(cube.CubeToDrag);
    }

}
