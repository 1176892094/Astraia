// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-27 21:09:40
// // # Recently: 2025-09-27 21:09:40
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using Astraia.Core;
using UnityEngine;

namespace Astraia.Net
{
    public class NetworkObserver : Singleton<NetworkObserver, Entity>, ISystem, IEvent<OnVisibleUpdate>, IEvent<ServerDisconnect>, IEvent<ServerObserver>
    {
        private readonly Dictionary<int, NetworkEntity> players = new Dictionary<int, NetworkEntity>();
        private readonly HashSet<NetworkClient> items = new HashSet<NetworkClient>();
        private readonly List<NetworkClient> nodes = new List<NetworkClient>();
        private Vector3Int range = Vector3Int.one;
        private Visible<NetworkClient> grids;
        private double waitTime;

        public void Execute(OnVisibleUpdate message)
        {
            waitTime = 0;
            range = new Vector3Int(message.x, message.y, message.z);
            grids = new Visible<NetworkClient>(message.x, message.y, message.z);
        }

        public void Execute(ServerDisconnect message)
        {
            players.Remove(message.client);
        }

        public void Execute(ServerObserver message)
        {
            players[message.entity.client.clientId] = message.entity;
            grids.Add(message.entity.client, message.entity.transform.position);
        }

        public void Update()
        {
            if (NetworkManager.isServer && grids != null)
            {
                foreach (var player in players.Values)
                {
                    grids.Update(player.client, player.transform.position);
                }

                if (waitTime < Time.unscaledTimeAsDouble)
                {
                    waitTime = Time.unscaledTimeAsDouble + 0.5;
                    foreach (var entity in NetworkManager.Server.spawns.Values)
                    {
                        if (entity.visible != Visible.Owner)
                        {
                            Tick(entity);
                        }
                    }
                }
            }
        }

        public void Tick(NetworkEntity entity, NetworkClient client)
        {
            if (players.TryGetValue(client.clientId, out var player))
            {
                var node = grids.Position(entity.transform.position) - grids.Position(player.transform.position);
                if (Mathf.Abs(node.x) <= range.x && Mathf.Abs(node.y) <= range.y)
                {
                    entity.AddObserver(client);
                }
            }
        }

        public void Tick(NetworkEntity entity)
        {
            grids.Find(grids.Position(entity.transform.position), items);

            if (entity.client != null)
            {
                items.Add(entity.client);
            }

            foreach (var client in items)
            {
                if (client.isReady && !entity.clients.Contains(client))
                {
                    entity.AddObserver(client);
                }
            }

            nodes.Clear();
            foreach (var client in entity.clients)
            {
                nodes.Add(client);
            }

            foreach (var client in nodes)
            {
                if (!items.Contains(client))
                {
                    entity.SubObserver(client);
                }
            }

            entity.gameObject.SetActive(items.Count > 0);
        }
    }
}