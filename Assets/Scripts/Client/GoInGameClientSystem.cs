using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DefaultNamespace
{
    [UpdateInGroup(typeof(ClientSimulationSystemGroup))]

    public partial class GoInGameClientSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<CubeSpawner>();
            RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<NetworkIdComponent>(),
                ComponentType.Exclude<NetworkStreamInGame>()
            ));
        }

        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithNone<NetworkStreamInGame>().ForEach((Entity entity, in NetworkIdComponent id) =>
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(entity);
                var req = commandBuffer.CreateEntity();
                commandBuffer.AddComponent<GoInGameRequest>(req);
                commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent()
                {
                    TargetConnection = entity
                });
            }).Run();
            commandBuffer.Playback(EntityManager);
        }
    }
}