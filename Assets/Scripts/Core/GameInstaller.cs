using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameConfiguratioSo gameConfig;
    [SerializeField] private RectTransform cubePlacementZone;
    public override void InstallBindings()
    {
        Container.Bind<GameEventsHandler>()
            .AsSingle();
        Container.Bind<IGameConfiguration>()
            .To<GameConfiguratioSo>()
            .FromInstance(gameConfig);
        Container.Bind<ICubePlacement>()
            .To<CubePlacementRules>()
            .AsSingle();
        Container.Bind<ITowerPlacement>()
            .To<TowerCubePlacementDefault>()
            .AsSingle()
            .WithArguments(cubePlacementZone);
        Container.Bind<ISaveService>()
            .To<JsonDataSaveService>()
            .AsSingle();
        Container.Bind<TowerService>()
            .AsSingle();
    }
}
