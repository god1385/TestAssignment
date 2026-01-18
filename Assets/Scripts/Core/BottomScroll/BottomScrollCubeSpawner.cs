using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BottomScrollCubeSpawner : MonoBehaviour
{
    [SerializeField] private CubeView cubeViewPrefab;
    [SerializeField] private ScrollRect mainScrollRect;
    [SerializeField] private Transform spawnRoot;
    [SerializeField] private RectTransform dragRoot;
    [SerializeField] private RectTransform bottomZone;
    [SerializeField] private float heightFactor = 0.6f;

    private IGameConfiguration _gameConfiguration;
    private List<CubeView> _cubes = new List<CubeView>();
    [Inject] private DiContainer _container;

    [Inject]
    public void Construct(IGameConfiguration gameConfiguration, GameEventsHandler eventsHandler)
    {
        _gameConfiguration = gameConfiguration;

        for (int i = 0; i < _gameConfiguration.BlockCount; i++)
        {
            CubeView cubeViewInstance = _container.InstantiatePrefabForComponent<CubeView>(cubeViewPrefab, spawnRoot);
            _cubes.Add(cubeViewInstance);
            cubeViewInstance.Initialize(_gameConfiguration.CubeColorSprites[i], dragRoot, mainScrollRect);
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!bottomZone || bottomZone.rect.height <= 0)
            return;

        for (int i = 0; i < _cubes.Count; i++)
            ResizeCube(_cubes[i]);
    }

    private void ResizeCube(CubeView cube)
    {
        float size = bottomZone.rect.height * heightFactor;

        var rect = cube.GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

        if (cube.LayoutElement == null) return;

        cube.LayoutElement.preferredWidth = size;
        cube.LayoutElement.preferredHeight = size;
    }
}

