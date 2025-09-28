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
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    internal struct Grid<T>
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

    [Serializable]
    internal class SpatialHashing : NetworkObserver, IEvent<InterestUpdate>, IEvent<ServerChangeScene>
    {
        private Dictionary<int, NetworkEntity> players = new Dictionary<int, NetworkEntity>();
        private Grid<NetworkClient> grids = new Grid<NetworkClient>(1024);
        private double waitTime;

        [SerializeField] private Vector2Int distance = new Vector2Int(30, 20);
        [SerializeField] private float interval = 1;
        public Vector2Int resolution => distance / 2;

        private void OnEnable()
        {
            EventManager.Listen<InterestUpdate>(this);
            EventManager.Listen<ServerChangeScene>(this);
        }

        private void OnDisable()
        {
            EventManager.Remove<InterestUpdate>(this);
            EventManager.Remove<ServerChangeScene>(this);
        }

        private void Update()
        {
            grids.Clear();
            foreach (var client in NetworkManager.Server.clients.Values)
            {
                if (players.TryGetValue(client, out var player))
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

        public void Execute(ServerChangeScene message)
        {
            players.Clear();
        }

        public void Execute(InterestUpdate message)
        {
            players.Add(message.entity.client, message.entity);
        }

        public override bool OnExecute(NetworkEntity entity, NetworkClient client)
        {
            var entityGrid = EntityToGrid(entity.transform.position);
            if (players.TryGetValue(client, out var player))
            {
                var playerGrid = EntityToGrid(player.transform.position);
                var delta = entityGrid - playerGrid;
                return Mathf.Abs(delta.x) <= 1 && Mathf.Abs(delta.y) <= 1;
            }

            return false;
        }

        private Vector2Int EntityToGrid(Vector3 position)
        {
            int rx = Mathf.Max(1, resolution.x);
            int ry = Mathf.Max(1, resolution.y);
            return new Vector2Int(Mathf.FloorToInt(position.x / rx), Mathf.FloorToInt(position.y / ry));
        }

        public override void OnRebuild(NetworkEntity entity, HashSet<NetworkClient> clients)
        {
            var position = EntityToGrid(entity.transform.position);
            grids.Set(position, clients);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (players == null || players.Count == 0) return;

            int rx = Mathf.Max(1, resolution.x);
            int ry = Mathf.Max(1, resolution.y);
            float rxf = rx;
            float ryf = (float)ry;

            // 可视化每个玩家格子与其 3x3 邻域
            foreach (var kv in players)
            {
                var player = kv.Value;
                if (player == null) continue;

                Vector3 pPos = player.transform.position;
                Vector2Int gridPos = EntityToGrid(pPos);

                // 格子左下角世界坐标
                Vector3 cellOrigin = new Vector3(gridPos.x * rxf, gridPos.y * ryf, pPos.z);

                // 格子中心、尺寸
                Vector3 center = cellOrigin + new Vector3(rxf / 2f, ryf / 2f, 0f);
                Vector3 size = new Vector3(rxf, ryf, 0.1f);

                // 中心格（玩家所在格）
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(center, size);

                // 画 3x3 邻域（包括中心）
                Gizmos.color = Color.yellow;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int nb = new Vector2Int(gridPos.x + dx, gridPos.y + dy);
                        Vector3 nbOrigin = new Vector3(nb.x * rxf, nb.y * ryf, pPos.z);
                        Vector3 nbCenter = nbOrigin + new Vector3(rxf / 2f, ryf / 2f, 0f);
                        Gizmos.DrawWireCube(nbCenter, size);
                    }
                }

                // 玩家位置点
                Gizmos.color = Color.cyan;
                float dotSize = Mathf.Min(rxf, ryf) * 0.12f;
                Gizmos.DrawSphere(pPos, dotSize);

                // 标签（需要 UnityEditor.Handles，安全地写成全名）
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center + Vector3.up * (ryf * 0.6f), $"P{kv.Key}");
#endif
            }
        }
#endif
    }
}