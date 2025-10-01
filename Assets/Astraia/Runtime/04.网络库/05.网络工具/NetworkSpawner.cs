// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-30 14:09:00
// // # Recently: 2025-09-30 14:09:00
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astraia.Net
{
    internal static class NetworkSpawner
    {
        private static readonly Dictionary<int, HashSet<NetworkEntity>> entityData = new();
        private static readonly Dictionary<uint, HashSet<NetworkClient>> clientData = new();

        public static void Spawn(NetworkEntity entity, NetworkClient client)
        {
            if (!clientData.TryGetValue(entity, out var clients))
            {
                clients = new HashSet<NetworkClient>();
                clientData.Add(entity, clients);
            }

            if (clients.Count == 0)
            {
                entity.ClearDirty(true);
            }

            if (clients.Add(client))
            {
                if (!entityData.TryGetValue(client, out var entities))
                {
                    entities = new HashSet<NetworkEntity>();
                    entityData.Add(client, entities);
                }
              
                entities.Add(entity);
                if (client.isReady)
                {
                    NetworkManager.Server.SpawnMessage(entity, client);
                }
            }
        }

        public static void Despawn(NetworkEntity entity, NetworkClient client)
        {
            if (clientData.TryGetValue(entity, out var clients))
            {
                if (clients.Remove(client) && clients.Count == 0)
                {
                    entity.ClearDirty(true);
                    clientData.Remove(entity);
                }
            }

            if (entityData.TryGetValue(client, out var entities))
            {
                if (entities.Remove(entity) && entities.Count == 0)
                {
                    entityData.Remove(client);
                }
            }
        }

        public static void Destroy(NetworkClient client)
        {
            if (entityData.TryGetValue(client, out var entities))
            {
                foreach (var entity in entities.ToArray())
                {
                    Despawn(entity, client);
                }
            }
        }

        public static void Destroy(NetworkEntity entity)
        {
            if (clientData.TryGetValue(entity, out var clients))
            {
                foreach (var client in clients.ToArray())
                {
                    Despawn(entity, client);
                }
            }
        }

        public static ICollection<NetworkEntity> Query(NetworkClient client)
        {
            return entityData.TryGetValue(client, out var entities) ? entities : Array.Empty<NetworkEntity>();
        }

        public static ICollection<NetworkClient> Query(NetworkEntity entity)
        {
            return clientData.TryGetValue(entity, out var clients) ? clients : Array.Empty<NetworkClient>();
        }

        public static void Dispose()
        {
            foreach (var clients in clientData.Values)
            {
                clients.Clear();
            }

            clientData.Clear();

            foreach (var entities in entityData.Values)
            {
                entities.Clear();
            }

            entityData.Clear();
        }
    }
}