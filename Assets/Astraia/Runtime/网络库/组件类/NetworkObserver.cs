// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 23:09:52
// # Recently: 2026-09-02 23:17:52
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Net
{
    public class NetworkObserver : Singleton<NetworkObserver>
    {
        private readonly Dictionary<NetworkClient, NetworkEntity> players = new Dictionary<NetworkClient, NetworkEntity>();
        private readonly SpatialHash<NetworkClient> visible = new SpatialHash<NetworkClient>();
        private readonly HashSet<NetworkClient> clients = new HashSet<NetworkClient>();
        private readonly List<NetworkClient> copies = new List<NetworkClient>();
        private double waitTime;

        [SerializeField]
        private int extentX = 1;

        [SerializeField]
        private int extentY = 1;

        [SerializeField]
        private int cellSize = 1;

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }

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

        private void LateUpdate()
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

                    foreach (var entity in NetworkManager.Server.spawns.Values)
                    {
                        if (entity.visible)
                        {
                            Tick(entity);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (var player in players.Values)
            {
                if (player)
                {
                    var center = WorldToNode(player.transform.position);
                    var centerX = center.x.FloorToInt();
                    var centerY = center.y.FloorToInt();

                    var minX = centerX - extentX;
                    var maxX = centerX + extentX;
                    var minY = centerY - extentY;
                    var maxY = centerY + extentY;

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
            visible.Query(WorldToNode(entity.transform.position), extentX, extentY, clients);

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

            if (entity.clients.Count == 0)
            {
                entity.gameObject.SetActive(false);
            }
        }

        public void Tick(NetworkEntity entity, NetworkClient client)
        {
            if (players.TryGetValue(client, out var player) && player && entity != player)
            {
                var pos = WorldToNode(entity.transform.position) - WorldToNode(player.transform.position);
                if (Mathf.Abs(pos.x.FloorToInt()) <= extentX && Mathf.Abs(pos.y.FloorToInt()) <= extentY)
                {
                    NetworkSpawner.Add(entity, client);
                }
            }

            if (entity.clients.Count == 0)
            {
                entity.gameObject.SetActive(false);
            }
        }

        private Position WorldToNode(Vector2 position)
        {
            return new Position(Mathf.FloorToInt(position.x / cellSize), Mathf.FloorToInt(position.y / cellSize));
        }

        public void Clear()
        {
            copies.Clear();
            clients.Clear();
            visible.Clear();
            players.Clear();
        }
    }
}