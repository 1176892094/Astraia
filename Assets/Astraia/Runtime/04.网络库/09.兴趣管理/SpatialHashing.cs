// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-28 18:09:21
// // # Recently: 2025-09-28 18:09:22
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    internal struct Grid<T>
    {
        private Dictionary<Vector2Int, HashSet<T>> grids;
        private Vector2Int[] offsets;

        public Grid(int capacity)
        {
            offsets = new[]
            {
                new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(0, 1),
                new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(-1, 1),
                new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(1, 1),
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

        private void Get(Vector2Int position, HashSet<T> result)
        {
            if (grids.TryGetValue(position, out var items))
            {
                foreach (var item in items)
                {
                    result.Add(item);
                }
            }
        }

        public void Find(Vector2Int position, HashSet<T> result)
        {
            result.Clear();
            foreach (var offset in offsets)
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

    [Serializable]
    public class SpatialHashing : NetworkObserver
    {
        private Dictionary<NetworkClient, NetworkEntity> players = new Dictionary<NetworkClient, NetworkEntity>();
        private Grid<NetworkClient> grid = new Grid<NetworkClient>(1024);
        private double waitTime;

        [SerializeField] private int distance = 30;
        [SerializeField] private float interval = 1;
        public int resolution => distance / 2;

        private Vector2Int EntityToGrid(Vector3 position)
        {
            return Vector2Int.RoundToInt(new Vector2(position.x, position.y) / resolution);
        }

        public override bool IsObserver(NetworkEntity entity, NetworkClient client)
        {
            var position = EntityToGrid(entity.transform.position);
            if (players.TryGetValue(client, out var player))
            {
                return (position - EntityToGrid(player.transform.position)).sqrMagnitude <= 2;
            }

            return false;
        }

        public override void OnRebuild(NetworkEntity entity, HashSet<NetworkClient> clients)
        {
            grid.Find(EntityToGrid(entity.transform.position), clients);
        }
        
        public override void Reset()
        {
            waitTime = 0;
        }

        private void Update()
        {
            grid.Clear();
            foreach (var client in NetworkManager.Server.clients.Values)
            {
                if (players.TryGetValue(client, out var player))
                {
                    var position = EntityToGrid(player.transform.position);
                    grid.Add(position, client);
                }
            }

            if (Time.unscaledTimeAsDouble >= waitTime + interval)
            {
                foreach (var entity in NetworkManager.Server.spawns.Values)
                {
                    NetworkManager.Server.SendToClients(entity, false);
                }

                waitTime = Time.unscaledTimeAsDouble;
            }
        }
    }
}