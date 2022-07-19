using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace DefaultNamespace
{
    [UnityEngine.Scripting.Preserve]
    public class GameBootstrap: ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7979;
            var server_ip = "127.0.0.1";

            DefaultConnectAddress = NetworkEndPoint.Parse(server_ip, 7979);
            var result = base.Initialize(defaultWorldName);
            Debug.Log($"We are connecting to the server {server_ip}");
           
            return result;
        }
        
       
    }
}