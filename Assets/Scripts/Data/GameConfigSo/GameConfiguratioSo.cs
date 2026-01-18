using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config", order = 1),]
public class GameConfiguratioSo : ScriptableObject, IGameConfiguration
{
    public int blockCount;
    public List<Sprite> cubeColorSprites;

    public int BlockCount => blockCount;
    public List<Sprite> CubeColorSprites => cubeColorSprites;
}
