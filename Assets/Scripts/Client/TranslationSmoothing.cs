using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Variants
{
    [UpdateInWorld(TargetWorld.Client)]
    public partial class SmoothingRegisteringSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            var smoothing = World.GetExistingSystem<GhostPredictionSmoothingSystem>();
            if (smoothing != null)
            {
                smoothing.RegisterSmoothingAction<Translation>(CustomSmoothing.Action);
            }
        }

        protected override void OnUpdate()
        {
        }
    }
    
    [BurstCompile]
    public unsafe class CustomSmoothing
    {
        public static PortableFunctionPointer<GhostPredictionSmoothingSystem.SmoothingActionDelegate>
            Action =
                new PortableFunctionPointer<GhostPredictionSmoothingSystem.SmoothingActionDelegate>(SmoothingAction);

        [BurstCompile(DisableDirectCall = true)]
        private static void SmoothingAction(void* currentData, void* previousData, void* userData)
        {
            ref var trans = ref UnsafeUtility.AsRef<Translation>(currentData);
            ref var backup = ref UnsafeUtility.AsRef<Translation>(previousData);
            if (math.any(math.isnan(trans.Value)) || math.any(math.isnan(backup.Value))) return;

            var dist = math.distance(trans.Value, backup.Value);
            if (dist > 0f)
            {
                Debug.Log($"Custom smoothing, y_b: {backup.Value.y} y_c: {trans.Value.y}   diff: {trans.Value - backup.Value}, dist {dist}, b: {backup.Value} c: {trans.Value} ");
                trans.Value = backup.Value + ((trans.Value - backup.Value) / dist) * 0.1f;
            }
        }
    }
}