using Leopotam.Ecs;
using Photon.Pun;
using UnityEngine;

public class EcsStartup : MonoBehaviourPunCallbacks
{
    [SerializeField] private SceneData _sceneData;
    [SerializeField] private Config _config;

    private EcsWorld _ecsWorld;
    private EcsSystems _systems;
    
    private void Start()
    {
        _ecsWorld = new EcsWorld();
        _systems = new EcsSystems(_ecsWorld);

        _systems
            .Add(new PlayerInitSystem())
            .Add(new PlayerInputSystem())
            .Add(new TimerSystem())
            .Inject(_sceneData)
            .Inject(_config)
            .Init();
    }
    
    private void Update()
    {
        _systems?.Run();
    }

    private void OnDestroy()
    {
        _systems?.Destroy();
        _systems = null;
        _ecsWorld?.Destroy();
        _ecsWorld = null;
    }
}
