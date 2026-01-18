using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameConfiguratioSo gameConfig;
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
        Container.Bind<ISaveService>()
            .To<JsonDataSaveService>()
            .AsSingle();
    }
}
