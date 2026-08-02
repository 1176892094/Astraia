using System;
using System.Collections.Generic;
using Astraia;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class SyncManager : NetworkSingleton<SyncManager>, IEvent<OnFixedUpdate>
    {
        private Dictionary<uint, Position> clientPosition = new Dictionary<uint, Position>();
        private Dictionary<uint, Position> serverPosition = new Dictionary<uint, Position>();
        private List<SyncData> copied = new List<SyncData>();
        private double sendTime;

        public Fixation syncTime;
        public int playerReady;
        public int playerCount;

        public void AddPosition(uint objectId, Position position) // 客户端将坐标提交到发送列表
        {
            clientPosition[objectId] = position;
        }

        public void SetPosition(uint objectId, Position position) // 服务器接收客户端提交的坐标
        {
            serverPosition[objectId] = position;
        }

        public void Execute(OnFixedUpdate message)
        {
            if (!NetworkSystem.Tick(ref sendTime))
            {
                return;
            }

            if (NetworkManager.isClient && NetworkManager.Client.isReady)
            {
                foreach (var kv in clientPosition)
                {
                    var actor = (Player)kv.Key;
                    if (actor)
                    {
                        actor.Sender.SetPositionServerRpc(kv.Value); //上报位置
                    }
                }

                clientPosition.Clear();
            }

            if (NetworkManager.isServer)
            {
                if (playerReady == playerCount && playerCount > 0)
                {
                    copied.Clear();
                    foreach (var kv in serverPosition)
                    {
                        copied.Add(new SyncData(kv.Key, kv.Value));
                    }

                    SendPositionClientRpc(Time.fixedTime, copied);
                }
            }
        }

        [ClientRpc]
        private void SendPositionClientRpc(Fixation syncTime, List<SyncData> syncs) // 存储下发的玩家坐标
        {
            foreach (var sync in syncs)
            {
                var player = (Player)sync.Id;
                if (player)
                {
                    if (!player.isOwner)
                    {
                        player.Machine.position = sync.Position;
                    }

                    player.Machine.syncPosition = sync.Position;
                    serverPosition[sync.Id] = sync.Position;
                }
            }

            Instance.syncTime = syncTime;
        }

        [Serializable]
        public struct SyncData
        {
            public uint Id;
            public Position Position;

            public SyncData(uint id, Position position)
            {
                Id = id;
                Position = position;
            }
        }
    }
}