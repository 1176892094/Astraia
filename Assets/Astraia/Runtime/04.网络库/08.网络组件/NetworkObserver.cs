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
        private readonly HashSet<int> clients = new HashSet<int>();
        private readonly List<int> copies = new List<int>();
        private Grid<int> grids = new Grid<int>(1024);
        private double waitTime;

        [SerializeField] private Vector2Int distance = new Vector2Int(30, 20);
        [SerializeField] private float interval = 1;

        private void Awake()
        {
            NetworkManager.Server.observer = this;
            NetworkManager.Client.observer = this;
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
            if (NetworkManager.isServer)
            {
                grids.Clear();
                foreach (var client in NetworkManager.Server.clients.Values)
                {
                    if (players.TryGetValue(client, out var player) && player)
                    {
                        grids.Add(EntityToGrid(player.transform.position), client);
                    }
                }

                if (waitTime + interval <= Time.unscaledTimeAsDouble)
                {
                    foreach (var entity in NetworkManager.Server.spawns.Values)
                    {
                        NetworkManager.Server.SpawnObserver(entity, false);
                    }

                    waitTime = Time.unscaledTimeAsDouble;
                }
            }
        }

        private Vector2Int EntityToGrid(Vector3 position)
        {
            var x = Mathf.Max(1, distance.x / 2);
            var y = Mathf.Max(1, distance.y / 2);
            return new Vector2Int(Mathf.FloorToInt(position.x / x), Mathf.FloorToInt(position.y / y));
        }

        public void Rebuild(NetworkEntity entity, bool reload)
        {
            clients.Clear();
            if (entity.visible != EntityType.Hide)
            {
                grids.Set(EntityToGrid(entity.transform.position), clients);
            }

            if (entity.client != null)
            {
                clients.Add(entity.client);
            }

            var queries = NetworkListener.Query(entity);
            foreach (NetworkClient client in clients)
            {
                if (client.isReady && (reload || !queries.Contains(client)))
                {
                    NetworkListener.Listen(entity, client);
                }
            }

            copies.Clear();
            foreach (var client in queries)
            {
                copies.Add(client);
            }

            foreach (NetworkClient client in copies)
            {
                if (!clients.Contains(client))
                {
                    NetworkListener.Remove(entity, client);
                }
            }
        }

        public bool OnExecute(NetworkEntity entity, NetworkClient client)
        {
            var entityGrid = EntityToGrid(entity.transform.position);
            if (players.TryGetValue(client, out var player) && player)
            {
                var playerGrid = entityGrid - EntityToGrid(player.transform.position);
                return Mathf.Abs(playerGrid.x) <= 1 && Mathf.Abs(playerGrid.y) <= 1;
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var x = Mathf.Max(1, distance.x / 2);
            var y = Mathf.Max(1, distance.y / 2);

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
            private Dictionary<Vector2Int, ICollection<T>> grids;
            private Vector2Int[] direction;

            public Grid(int capacity)
            {
                direction = new[]
                {
                    new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
                    new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1),
                    new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1),
                };
                grids = new Dictionary<Vector2Int, ICollection<T>>(capacity);
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

            public void Set(Vector2Int position, ICollection<T> result)
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