// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-09-27 21:09:40
// # Recently: 2025-09-27 21:09:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public class NetworkObserver : Singleton<NetworkObserver>, IEvent<OnAfterUpdate>, IEvent<OnGizmoUpdate>
    {
        private readonly Dictionary<NetworkClient, NetworkEntity> players = new Dictionary<NetworkClient, NetworkEntity>();
        private readonly HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();
        private readonly HashSet<NetworkClient> clients = new HashSet<NetworkClient>();
        private readonly List<NetworkClient> copies = new List<NetworkClient>();
        private SpatialHash<NetworkClient> visible;
        private double waitTime;

        [SerializeField] private Vector2Int extents = Vector2Int.one;
        [SerializeField] private float cellSize = 1;

        public override void Dequeue()
        {
            visible = new SpatialHash<NetworkClient>(cellSize);
        }

        public override void Enqueue()
        {
            copies.Clear();
            clients.Clear();
            visible.Clear();
            players.Clear();
            entities.Clear();
        }

        public void Register(NetworkEntity entity)
        {
            players[entity.client] = entity;
            visible.Insert(entity.client, entity.transform.position);
        }

        public void UnRegister(NetworkClient client)
        {
            players.Remove(client);
            visible.Remove(client);
            waitTime = NetworkManager.syncTime + 0.2;
        }

        public void Execute(OnAfterUpdate message)
        {
            if (NetworkManager.isServer)
            {
                foreach (var player in players.Values)
                {
                    if (player)
                    {
                        visible.Update(player.client, player.transform.position);
                    }
                }

                if (waitTime < NetworkManager.syncTime)
                {
                    waitTime = NetworkManager.syncTime + 0.2;
                    foreach (var entity in entities)
                    {
                        Tick(entity);
                    }
                }
            }
        }

        public void Execute(OnGizmoUpdate message)
        {
            foreach (var player in players.Values)
            {
                Gizmos.color = Color.cyan;
                var pos = visible.WorldToNode(player.transform.position);
                var min = pos - extents;
                var max = pos + extents;

                for (var x = min.x; x <= max.x; x++)
                {
                    for (var y = min.y; y <= max.y; y++)
                    {
                        Gizmos.DrawWireCube(new Vector2(x + 0.5F, y + 0.5F) * cellSize, Vector2.one * cellSize);
                    }
                }
            }
        }

        public void Tick(NetworkEntity entity)
        {
            visible.Query(entity.transform.position, extents, clients);

            if (entity.client != null)
            {
                clients.Add(entity.client);
            }

            var copied = entity.Clients();
            foreach (var client in clients)
            {
                if (client.isReady && !copied.Contains(client))
                {
                    entity.Add(client);
                }
            }

            copies.Clear();
            copies.AddRange(copied);

            foreach (var client in copies)
            {
                if (!clients.Contains(client))
                {
                    entity.Remove(client);
                }
            }
        }

        public void Tick(NetworkEntity entity, NetworkClient client)
        {
            if (players.TryGetValue(client, out var player) && player && entity != player)
            {
                var pos = visible.WorldToNode(entity.transform.position) - visible.WorldToNode(player.transform.position);
                if (Mathf.Abs(pos.x) <= extents.x && Mathf.Abs(pos.y) <= extents.y)
                {
                    entity.Add(client);
                }
            }
        }

        public void Add(NetworkEntity entity)
        {
            entities.Add(entity);
        }

        public void Remove(NetworkEntity entity)
        {
            entities.Remove(entity);
        }

        public void Clear()
        {
            entities.Clear();
        }
    }
}