using Unity.NetCode;
using UnityEngine;

namespace DefaultNamespace.Components
{
    public class PredictedPhysicsConfigAuthoring: MonoBehaviour
    {
        public bool DisableWhenNoConnections = false;
        public int PhysicsTicksPerSimTick = 1;
    }

    public class PredictedPhysicsConfigConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((PredictedPhysicsConfigAuthoring physicsConfig) =>
            {
                var entity = GetPrimaryEntity(physicsConfig);

                DstEntityManager.AddComponentData(entity, new PredictedPhysicsConfig()
                {
                    DisableWhenNoConnections = physicsConfig.DisableWhenNoConnections,
                    PhysicsTicksPerSimTick = physicsConfig.PhysicsTicksPerSimTick
                });
            });
        }
    }
}