//THIS FILE IS AUTOGENERATED BY GHOSTCOMPILER. DON'T MODIFY OR ALTER.
using AOT;
using Unity.Burst;
using Unity.Networking.Transport;
#region __COMMAND_USING_STATEMENT__
using __COMMAND_USING__;
#endregion


namespace __COMMAND_NAMESPACE__
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    [BurstCompile]
    public struct __COMMAND_NAME__Serializer : IComponentData, IRpcCommandSerializer<__COMMAND_COMPONENT_TYPE__>
    {
        public void Serialize(ref DataStreamWriter writer, in RpcSerializerState state, in __COMMAND_COMPONENT_TYPE__ data)
        {
            #region __COMMAND_WRITE__
            #endregion
        }

        public void Deserialize(ref DataStreamReader reader, in RpcDeserializerState state, ref __COMMAND_COMPONENT_TYPE__ data)
        {
            #region __COMMAND_READ__
            #endregion
        }
        [BurstCompile(DisableDirectCall = true)]
        [MonoPInvokeCallback(typeof(RpcExecutor.ExecuteDelegate))]
        private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
        {
            RpcExecutor.ExecuteCreateRequestComponent<__COMMAND_NAME__Serializer, __COMMAND_COMPONENT_TYPE__>(ref parameters);
        }

        static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
            new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);
        public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
        {
            return InvokeExecuteFunctionPointer;
        }
    }

    [System.Runtime.CompilerServices.CompilerGenerated]
    class __COMMAND_NAME__RpcCommandRequestSystem : RpcCommandRequestSystem<__COMMAND_NAME__Serializer, __COMMAND_COMPONENT_TYPE__>
    {
        [BurstCompile]
        protected struct SendRpc : IJobEntityBatch
        {
            public SendRpcData data;
            public void Execute(ArchetypeChunk chunk, int orderIndex)
            {
                data.Execute(chunk, orderIndex);
            }
        }
        protected override void OnUpdate()
        {
            var sendJob = new SendRpc{data = InitJobData()};
            ScheduleJobData(sendJob);
        }
    }
}
