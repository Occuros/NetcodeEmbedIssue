using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport.Utilities;

namespace Unity.NetCode
{
    public struct NetworkSnapshotAckComponent : IComponentData
    {
        public void UpdateReceivedByRemote(uint tick, uint mask)
        {
            if (tick == 0)
            {
                ReceivedSnapshotByRemoteMask3 = 0;
                ReceivedSnapshotByRemoteMask2 = 0;
                ReceivedSnapshotByRemoteMask1 = 0;
                ReceivedSnapshotByRemoteMask0 = 0;
                LastReceivedSnapshotByRemote = 0;
            }
            else if (LastReceivedSnapshotByRemote == 0)
            {
                ReceivedSnapshotByRemoteMask3 = 0;
                ReceivedSnapshotByRemoteMask2 = 0;
                ReceivedSnapshotByRemoteMask1 = 0;
                ReceivedSnapshotByRemoteMask0 = mask;
                LastReceivedSnapshotByRemote = tick;
            }
            else if (SequenceHelpers.IsNewer(tick, LastReceivedSnapshotByRemote))
            {
                int shamt = (int) (tick - LastReceivedSnapshotByRemote);
                if (shamt >= 256)
                {
                    ReceivedSnapshotByRemoteMask3 = 0;
                    ReceivedSnapshotByRemoteMask2 = 0;
                    ReceivedSnapshotByRemoteMask1 = 0;
                    ReceivedSnapshotByRemoteMask0 = mask;
                }
                else
                {
                    while (shamt >= 64)
                    {
                        ReceivedSnapshotByRemoteMask3 = ReceivedSnapshotByRemoteMask2;
                        ReceivedSnapshotByRemoteMask2 = ReceivedSnapshotByRemoteMask1;
                        ReceivedSnapshotByRemoteMask1 = ReceivedSnapshotByRemoteMask0;
                        ReceivedSnapshotByRemoteMask0 = 0;
                        shamt -= 64;
                    }

                    if (shamt == 0)
                        ReceivedSnapshotByRemoteMask0 |= mask;
                    else
                    {
                        ReceivedSnapshotByRemoteMask3 = (ReceivedSnapshotByRemoteMask3 << shamt) |
                                                        (ReceivedSnapshotByRemoteMask2 >> (64 - shamt));
                        ReceivedSnapshotByRemoteMask2 = (ReceivedSnapshotByRemoteMask2 << shamt) |
                                                        (ReceivedSnapshotByRemoteMask1 >> (64 - shamt));
                        ReceivedSnapshotByRemoteMask1 = (ReceivedSnapshotByRemoteMask1 << shamt) |
                                                        (ReceivedSnapshotByRemoteMask0 >> (64 - shamt));
                        ReceivedSnapshotByRemoteMask0 = (ReceivedSnapshotByRemoteMask0 << shamt) |
                                                        mask;
                    }
                }

                LastReceivedSnapshotByRemote = tick;
            }
        }

        public bool IsReceivedByRemote(uint tick)
        {
            if (tick == 0 || LastReceivedSnapshotByRemote == 0)
                return false;
            if (SequenceHelpers.IsNewer(tick, LastReceivedSnapshotByRemote))
                return false;
            int bit = (int) (LastReceivedSnapshotByRemote - tick);
            if (bit >= 256)
                return false;
            if (bit >= 192)
            {
                bit -= 192;
                return (ReceivedSnapshotByRemoteMask3 & (1ul << bit)) != 0;
            }

            if (bit >= 128)
            {
                bit -= 128;
                return (ReceivedSnapshotByRemoteMask2 & (1ul << bit)) != 0;
            }

            if (bit >= 64)
            {
                bit -= 64;
                return (ReceivedSnapshotByRemoteMask1 & (1ul << bit)) != 0;
            }

            return (ReceivedSnapshotByRemoteMask0 & (1ul << bit)) != 0;
        }

        public uint LastReceivedSnapshotByRemote;
        private ulong ReceivedSnapshotByRemoteMask0;
        private ulong ReceivedSnapshotByRemoteMask1;
        private ulong ReceivedSnapshotByRemoteMask2;
        private ulong ReceivedSnapshotByRemoteMask3;
        public uint LastReceivedSnapshotByLocal;
        public uint ReceivedSnapshotByLocalMask;
        public uint NumLoadedPrefabs;

        public void UpdateRemoteAckedData(uint remoteTime, uint numLoadedPrefabs, uint interpolationDelay)
        {
            //Because the remote time is updated also by RPC and there is no order guarante for witch is handled
            //first (snapshost or rpc message) it is necessary to accept update if received remoteTime
            //is also equals to the LastReceivedRemoteTime.
            if (remoteTime != 0 && (!SequenceHelpers.IsNewer(LastReceivedRemoteTime, remoteTime) || LastReceivedRemoteTime == 0))
            {
                NumLoadedPrefabs = numLoadedPrefabs;
                RemoteInterpolationDelay = interpolationDelay;
            }
        }

        public void UpdateRemoteTime(uint remoteTime, uint localTimeMinusRTT, uint localTime)
        {
            //Because we sync time using both RPC and snapshot it is more correct to also accept
            //update the stats for a remotetime that is equals to the last received one.
            if (remoteTime != 0 && (!SequenceHelpers.IsNewer(LastReceivedRemoteTime, remoteTime) || LastReceivedRemoteTime == 0))
            {
                LastReceivedRemoteTime = remoteTime;
                LastReceiveTimestamp = localTime;
                if (localTimeMinusRTT == 0)
                    return;
                uint lastReceivedRTT = localTime - localTimeMinusRTT;
                // Highest bit set means we got a negative value, which can happen on low ping due to clock difference between client and server
                if ((lastReceivedRTT & (1<<31)) != 0)
                    lastReceivedRTT = 0;
                if (EstimatedRTT == 0)
                    EstimatedRTT = lastReceivedRTT;
                else
                    EstimatedRTT = EstimatedRTT * 0.875f + lastReceivedRTT * 0.125f;
                DeviationRTT = DeviationRTT * 0.75f + math.abs(lastReceivedRTT - EstimatedRTT) * 0.25f;
            }
        }

        public uint LastReceivedRemoteTime;
        public uint LastReceiveTimestamp;
        public float EstimatedRTT;
        public float DeviationRTT;
        public int ServerCommandAge;
        public uint RemoteInterpolationDelay;
    }
}
