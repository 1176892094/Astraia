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
    public class NetworkObserving : IEvent<OnAfterUpdate>, IEvent<OnGizmoUpdate>
    {
        public static NetworkObserving Instance;

        private readonly Dictionary<NetworkClient, NetworkEntity> players = new Dictionary<NetworkClient, NetworkEntity>();
        private readonly SpatialHash<NetworkClient> visible = new SpatialHash<NetworkClient>();
        private readonly HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();
        private readonly HashSet<NetworkClient> clients = new HashSet<NetworkClient>();
        private readonly List<NetworkClient> copies = new List<NetworkClient>();

        private double waitTime;

        [SerializeField] private Vector2Int extents = Vector2Int.one;
        [SerializeField] private float cellSize = 1;

        public void Register(NetworkEntity entity)
        {
            players[entity.client] = entity;
            visible.Insert(entity.client, WorldToNode(entity.transform.position));
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
                        visible.Update(player.client, WorldToNode(player.transform.position));
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
            Gizmos.color = Color.cyan;
            foreach (var player in players.Values)
            {
                if (player)
                {
                    var center = WorldToNode(player.transform.position);
                    var minX = center.X - extents.x;
                    var maxX = center.X + extents.x;
                    var minY = center.Y - extents.y;
                    var maxY = center.Y + extents.y;

                    for (var x = minX; x <= maxX; x++)
                    {
                        for (var y = minY; y <= maxY; y++)
                        {
                            Gizmos.DrawWireCube(new Vector2(x + 0.5F, y + 0.5F) * cellSize, Vector2.one * cellSize);
                        }
                    }
                }
            }
        }

        public void Tick(NetworkEntity entity)
        {
            visible.Query(WorldToNode(entity.transform.position), extents.x, extents.y, clients);

            if (entity.client != null)
            {
                clients.Add(entity.client);
            }

            var copied = entity.clients;
            foreach (var client in clients)
            {
                if (client.isReady && !copied.Contains(client))
                {
                    NetworkSpawner.Add(entity, client);
                }
            }

            copies.Clear();
            copies.AddRange(copied);

            foreach (var client in copies)
            {
                if (!clients.Contains(client))
                {
                    NetworkSpawner.Remove(entity, client);
                }
            }
        }

        public void Tick(NetworkEntity entity, NetworkClient client)
        {
            if (players.TryGetValue(client, out var player) && player && entity != player)
            {
                var pos = WorldToNode(entity.transform.position) - WorldToNode(player.transform.position);
                if (Mathf.Abs(pos.X) <= extents.x && Mathf.Abs(pos.Y) <= extents.y)
                {
                    NetworkSpawner.Add(entity, client);
                }
                else
                {
                    entity.gameObject.SetActive(false);
                }
            }
        }

        public Position WorldToNode(Vector2 position)
        {
            var x = Mathf.FloorToInt(position.x / cellSize);
            var y = Mathf.FloorToInt(position.y / cellSize);
            return new Position(x, y);
        }

        public void Dispose()
        {
            copies.Clear();
            clients.Clear();
            visible.Clear();
            players.Clear();
            entities.Clear();
        }

        public void Add(NetworkEntity entity)
        {
            entities.Add(entity);
        }

        public void Remove(NetworkEntity entity)
        {
            entities.Remove(entity);
        }
    }
}