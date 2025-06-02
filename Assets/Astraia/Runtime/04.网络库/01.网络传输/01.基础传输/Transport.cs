// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 01:01:43
// # Recently: 2025-01-10 01:01:43
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Net
{
    public abstract class Transport : MonoBehaviour
    {
        public static Transport Instance;
        
        public string address = "localhost";
        public ushort port = 20974;
        
        public Action OnClientConnect;
        public Action OnClientDisconnect;
        public Action<ArraySegment<byte>, int> OnClientSend;
        public Action<ArraySegment<byte>, int> OnClientReceive;
        public Action<int> OnServerConnect;
        public Action<int> OnServerDisconnect;
        public Action<int, ArraySegment<byte>, int> OnServerSend;
        public Action<int, ArraySegment<byte>, int> OnServerReceive;
        
        public abstract int SendLength(int channel);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void StopClient(int clientId);
        public abstract void StartClient();
        public abstract void StartClient(Uri uri);
        public abstract void StopClient();
        public abstract void ClientEarlyUpdate();
        public abstract void ClientAfterUpdate();
        public abstract void ServerEarlyUpdate();
        public abstract void ServerAfterUpdate();
    }
}