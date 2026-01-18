using System.Collections.Generic;
using UnityEngine;

public interface IGameConfiguration
{
    public int BlockCount { get; }
    public List<Sprite> CubeColorSprites { get; }
}
