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

using System;
using System.Collections.Generic;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    public class NetworkObserver : MonoBehaviour, IEvent<ServerDisconnect>, IEvent<ServerObserver>
    {
        private readonly Dictionary<int, NetworkEntity> players = new Dictionary<int, NetworkEntity>();
        private readonly HashSet<NetworkClient> clients = new HashSet<NetworkClient>();
        private Grid<NetworkClient> grids = new Grid<NetworkClient>(1024);
        private double waitTime;

        [SerializeField] private Vector2Int distance = new Vector2Int(30, 20);
        [SerializeField] private float interval = 1;

        private void Awake()
        {
            NetworkManager.Server.observer = this;
        }

        private void OnEnable()
        {
            EventManager.Listen<ServerObserver>(this);
            EventManager.Listen<ServerDisconnect>(this);
        }

        private void OnDisable()
        {
            EventManager.Remove<ServerObserver>(this);
            EventManager.Remove<ServerDisconnect>(this);
        }

        public void Execute(ServerDisconnect message)
        {
            players.Remove(message.client);
        }

        public void Execute(ServerObserver message)
        {
            players[message.entity.client] = message.entity;
        }

        private void Update()
        {
            if (NetworkManager.Server.isActive)
            {
                grids.Clear();
                foreach (var client in NetworkManager.Server.clients.Values)
                {
                    if (players.TryGetValue(client, out var player) && player)
                    {
                        var position = EntityToGrid(player.transform.position);
                        grids.Add(position, client);
                    }
                }

                if (waitTime + interval <= Time.unscaledTimeAsDouble)
                {
                    waitTime = Time.unscaledTimeAsDouble;
                    foreach (var entity in NetworkManager.Server.spawns.Values)
                    {
                        NetworkManager.Server.SendToClients(entity, false);
                    }
                }
            }
        }

        private Vector2Int EntityToGrid(Vector3 position)
        {
            var resolution = distance / 2;
            var x = Mathf.Max(1, resolution.x);
            var y = Mathf.Max(1, resolution.y);
            return new Vector2Int(Mathf.FloorToInt(position.x / x), Mathf.FloorToInt(position.y / y));
        }

        private void OnRebuild(NetworkEntity entity, HashSet<NetworkClient> clients)
        {
            var position = EntityToGrid(entity.transform.position);
            grids.Set(position, clients);
        }

        public void Rebuild(NetworkEntity entity, bool isReady)
        {
            clients.Clear();
            if (entity.data != EntityData.Hide)
            {
                OnRebuild(entity, clients);
            }

            if (entity.client != null)
            {
                clients.Add(entity.client);
            }

            var changed = false;
            foreach (var client in clients)
            {
                if (client.isReady)
                {
                    if (isReady || !entity.clients.Contains(client))
                    {
                        client.entities.Add(entity);
                        NetworkManager.Server.SpawnToClient(client, entity);
                        changed = true;
                    }
                }
            }

            foreach (var client in entity.clients)
            {
                if (!clients.Contains(client))
                {
                    client.entities.Remove(entity);
                    client.Send(new DespawnMessage(entity.objectId));
                    changed = true;
                }
            }

            if (changed)
            {
                entity.clients.Clear();
                foreach (var client in clients)
                {
                    if (client.isReady)
                    {
                        entity.clients.Add(client);
                    }
                }
            }
        }

        public bool OnExecute(NetworkEntity entity, NetworkClient client)
        {
            var entityGrid = EntityToGrid(entity.transform.position);
            if (players.TryGetValue(client, out var player) && player)
            {
                var playerGrid = EntityToGrid(player.transform.position);
                var delta = entityGrid - playerGrid;
                return Mathf.Abs(delta.x) <= 1 && Mathf.Abs(delta.y) <= 1;
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var resolution = distance / 2;
            var x = Mathf.Max(1, resolution.x);
            var y = Mathf.Max(1, resolution.y);

            foreach (var player in players.Values)
            {
                if (player)
                {
                    var target = player.transform.position;
                    var source = EntityToGrid(target);
                    var origin = new Vector3(source.x * x, source.y * y, target.z);
                    var center = origin + new Vector3(x / 2f, y / 2f, 0f);

                    Gizmos.color = Color.cyan;
                    for (var dx = -1; dx <= 1; dx++)
                    {
                        for (var dy = -1; dy <= 1; dy++)
                        {
                            var nb = new Vector2Int(source.x + dx, source.y + dy);
                            var nbOrigin = new Vector3(nb.x * x, nb.y * y, target.z);
                            var nbCenter = nbOrigin + new Vector3(x / 2f, y / 2f, 0f);
                            Gizmos.DrawWireCube(nbCenter, new Vector3(x, y, 0));
                        }
                    }

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(center, new Vector3(x, y, 0));
                }
            }
        }
#endif
        [Serializable]
        private struct Grid<T>
        {
            private Dictionary<Vector2Int, HashSet<T>> grids;
            private Vector2Int[] direction;

            public Grid(int capacity)
            {
                direction = new[]
                {
                    new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
                    new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1),
                    new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1),
                };
                grids = new Dictionary<Vector2Int, HashSet<T>>(capacity);
            }

            public void Add(Vector2Int position, T value)
            {
                if (!grids.TryGetValue(position, out var items))
                {
                    items = new HashSet<T>(128);
                    grids[position] = items;
                }

                items.Add(value);
            }

            public void Set(Vector2Int position, HashSet<T> result)
            {
                result.Clear();
                foreach (var offset in direction)
                {
                    if (grids.TryGetValue(position + offset, out var items))
                    {
                        foreach (var item in items)
                        {
                            result.Add(item);
                        }
                    }
                }
            }

            public void Clear()
            {
                foreach (var items in grids.Values)
                {
                    items.Clear();
                }
            }
        }
    }
}