#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using Unity.Entities;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Unity.NetCode
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
#if UNITY_DOTSRUNTIME
#if !UNITY_SERVER
    [UpdateBefore(typeof(TickClientSimulationSystem))]
#endif
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
    [UpdateBefore(typeof(TickServerSimulationSystem))]
#endif
#endif
    [UpdateInWorld(TargetWorld.Default)]
    partial class GhostStatsSystem : SystemBase
    {
        #if UNITY_EDITOR
        [MenuItem("Multiplayer/Open NetDbg")]
        public static void OpenDebugger()
        {
            System.Diagnostics.Process.Start(Path.GetFullPath("Packages/com.unity.netcode/Runtime/Stats/netdbg.html"));
        }
        #endif
        private DebugWebSocket m_Socket;
        private List<GhostStatsCollectionSystem> m_StatsCollections;
        internal static ushort Port = 8787;

        protected override void OnCreate()
        {
            if (Port == 0)
                Enabled = false;
            else
                m_Socket = new DebugWebSocket(Port);
        }

        protected override void OnDestroy()
        {
            if (m_Socket != null)
                m_Socket.Dispose();
        }

        protected override void OnUpdate()
        {
            if (m_Socket == null)
                return;
            if (m_Socket.AcceptNewConnection())
            {
                m_StatsCollections = new List<GhostStatsCollectionSystem>();
                foreach (var world in World.All)
                {
                    var stats = world.GetExistingSystem<GhostStatsCollectionSystem>();
                    if (stats != null)
                    {
                        stats.SetIndex(m_StatsCollections.Count);
                        m_StatsCollections.Add(stats);
                    }
                }
            }

            if (!m_Socket.HasConnection)
            {
                if (m_StatsCollections != null)
                {
                    for (var con = 0; con < m_StatsCollections.Count; ++con)
                    {
                        m_StatsCollections[con].SetIndex(-1);
                    }

                    m_StatsCollections = null;
                }
                return;
            }

            if (m_StatsCollections == null || m_StatsCollections.Count == 0)
                return;

            for (var con = 0; con < m_StatsCollections.Count; ++con)
            {
                m_StatsCollections[con].SendPackets(m_Socket);
            }
        }
    }
}
#endif

