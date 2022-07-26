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
    public struct __COMMAND_NAME__Serializer : ICommandDataSerializer<__COMMAND_COMPONENT_TYPE__>
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

        public void Serialize(ref DataStreamWriter writer, in RpcSerializerState state, in __COMMAND_COMPONENT_TYPE__ data, in __COMMAND_COMPONENT_TYPE__ baseline, NetworkCompressionModel compressionModel)
        {
            #region __COMMAND_WRITE_PACKED__
            #endregion
        }

        public void Deserialize(ref DataStreamReader reader, in RpcDeserializerState state, ref __COMMAND_COMPONENT_TYPE__ data, in __COMMAND_COMPONENT_TYPE__ baseline, NetworkCompressionModel compressionModel)
        {
            #region __COMMAND_READ_PACKED__
            #endregion
        }
    }

    [System.Runtime.CompilerServices.CompilerGenerated]
    public class __COMMAND_NAME__SendCommandSystem : CommandSendSystem<__COMMAND_NAME__Serializer, __COMMAND_COMPONENT_TYPE__>
    {
        [BurstCompile]
        struct SendJob : IJobEntityBatch
        {
            public SendJobData data;
            public void Execute(ArchetypeChunk chunk, int orderIndex)
            {
                data.Execute(chunk, orderIndex);
            }
        }
        protected override void OnUpdate()
        {
            if (!ShouldRunCommandJob())
                return;
            var sendJob = new SendJob{data = InitJobData()};
            ScheduleJobData(sendJob);
        }
    }

    [System.Runtime.CompilerServices.CompilerGenerated]
    public class __COMMAND_NAME__ReceiveCommandSystem : CommandReceiveSystem<__COMMAND_NAME__Serializer, __COMMAND_COMPONENT_TYPE__>
    {
        [BurstCompile]
        struct ReceiveJob : IJobEntityBatch
        {
            public ReceiveJobData data;
            public void Execute(ArchetypeChunk chunk, int orderIndex)
            {
                data.Execute(chunk, orderIndex);
            }
        }
        protected override void OnUpdate()
        {
            var recvJob = new ReceiveJob{data = InitJobData()};
            ScheduleJobData(recvJob);
        }
    }
}
