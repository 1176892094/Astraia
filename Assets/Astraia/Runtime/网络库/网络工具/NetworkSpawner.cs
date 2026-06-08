using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Core;

namespace Astraia.Net
{
    internal static class NetworkSpawner
    {
        private static readonly Dictionary<NetworkEntity, HashSet<NetworkClient>> clientDict = new Dictionary<NetworkEntity, HashSet<NetworkClient>>();
        private static readonly Dictionary<NetworkClient, HashSet<NetworkEntity>> entityDict = new Dictionary<NetworkClient, HashSet<NetworkEntity>>();

        public static void Add(this NetworkEntity entity, NetworkClient client)
        {
            if (!clientDict.TryGetValue(entity, out var clients))
            {
                clients = new HashSet<NetworkClient>();
                clientDict[entity] = clients;
            }

            if (clients.Add(client))
            {
                if (clients.Count == 1)
                {
                    entity.ClearDirty(true);
                }

                if (!entityDict.TryGetValue(client, out var entities))
                {
                    entities = new HashSet<NetworkEntity>();
                    entityDict[client] = entities;
                }

                entities.Add(entity);

                using var owner = MemoryWriter.Pop();
                using var other = MemoryWriter.Pop();
                ArraySegment<byte> segment = default;
                if (entity.modules.Length > 0)
                {
                    entity.modules.ServerSend(owner, other, true);
                    segment = (ArraySegment<byte>)(entity.client == client ? owner : other);
                }

                client.Send(new SpawnMessage(entity, client, segment));
            }
        }

        public static void Remove(this NetworkEntity entity, NetworkClient client)
        {
            if (clientDict.TryGetValue(entity, out var clients))
            {
                if (clients.Remove(client))
                {
                    if (clients.Count == 0)
                    {
                        entity.ClearDirty(true);
                    }

                    client.Entities().Remove(entity);
                    client.Send(new DespawnMessage(entity.objectId));
                }
            }
        }

        public static void Clear(this NetworkEntity entity)
        {
            if (clientDict.Remove(entity, out var clients))
            {
                foreach (var client in clients)
                {
                    client.Entities().Remove(entity);
                }

                clients.Clear();
            }
        }

        public static void Clear(this NetworkClient client)
        {
            if (entityDict.Remove(client, out var entities))
            {
                var copies = entities.Where(entity => entity).ToList();
                foreach (var entity in copies)
                {
                    var clients = entity.Clients();
                    if (clients.Remove(client))
                    {
                        if (clients.Count == 0)
                        {
                            entity.ClearDirty(true);
                        }

                        entities.Remove(entity);
                    }
                }
            }
        }

        public static HashSet<NetworkClient> Clients(this NetworkEntity entity)
        {
            return clientDict.GetValueOrDefault(entity);
        }

        public static HashSet<NetworkEntity> Entities(this NetworkClient entity)
        {
            return entityDict.GetValueOrDefault(entity);
        }
    }
}