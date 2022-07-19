using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial class GoInGameServerSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<CubeSpawner>();
            RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<GoInGameRequest>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
        }

        protected override void OnUpdate()
        {
            Entities
                .WithNone<SendRpcCommandRequestComponent>()
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity requestEntity, in GoInGameRequest request,
                    in ReceiveRpcCommandRequestComponent received) =>
                {
                    EntityManager.AddComponent<NetworkStreamInGame>(received.SourceConnection);
                    var networkId = GetComponent<NetworkIdComponent>(received.SourceConnection).Value;
                    Debug.Log($"Server setting connection {networkId} to in game");
                    EntityManager.DestroyEntity(requestEntity);
                }).Run();
                
        }
    }
}