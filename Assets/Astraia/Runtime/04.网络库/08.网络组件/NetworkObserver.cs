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

using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Net
{
    public class NetworkObserver : MonoBehaviour
    {
        public static NetworkObserver Instance;

        private readonly Dictionary<NetworkClient, NetworkEntity> players = new Dictionary<NetworkClient, NetworkEntity>();
        private readonly HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();
        private readonly HashSet<NetworkClient> clients = new HashSet<NetworkClient>();
        private readonly List<NetworkClient> copies = new List<NetworkClient>();
        private SpatialHash<NetworkClient> visible;
        private double waitTime;

        [SerializeField] private Vector2Int extents = Vector2Int.one;
        [SerializeField] private float cellSize = 1;

        private void Awake()
        {
            Instance = this;
            visible = new SpatialHash<NetworkClient>(cellSize);
        }

        private void OnDestroy()
        {
            copies.Clear();
            clients.Clear();
            visible.Clear();
            players.Clear();
            entities.Clear();
        }

        private void OnDrawGizmos()
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

        public void Register(NetworkEntity entity)
        {
            players[entity.client] = entity;
            visible.Insert(entity.client, entity.transform.position);
        }

        public void UnRegister(NetworkClient client)
        {
            if (client != null)
            {
                players.Remove(client);
                visible.Remove(client);
                waitTime = Time.unscaledTimeAsDouble + 0.2;
            }
        }

        public void LateUpdate()
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

                if (waitTime < Time.unscaledTimeAsDouble)
                {
                    waitTime = Time.unscaledTimeAsDouble + 0.2;
                    foreach (var entity in entities)
                    {
                        Tick(entity);
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

            foreach (var client in clients)
            {
                if (client.isReady && !entity.clients.Contains(client))
                {
                    entity.AddObserver(client);
                }
            }

            copies.Clear();
            copies.AddRange(entity.clients);

            foreach (var client in copies)
            {
                if (!clients.Contains(client))
                {
                    entity.SubObserver(client);
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
                    entity.AddObserver(client);
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