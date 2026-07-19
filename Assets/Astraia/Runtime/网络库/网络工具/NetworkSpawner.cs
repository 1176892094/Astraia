using System;
using System.Linq;
using Astraia;

namespace Astraia.Net
{
    internal static class NetworkSpawner
    {
        public static void Add(NetworkEntity entity, NetworkClient client)
        {
            var clients = entity.clients;
            if (clients.Add(client))
            {
                if (clients.Count == 1)
                {
                    entity.ClearDirty(true);
                }

                using var owner = MemoryWriter.Pop();
                using var other = MemoryWriter.Pop();
                ArraySegment<byte> segment = default;
                if (entity.modules.Length > 0)
                {
                    entity.modules.ServerSend(owner, other, true);
                    segment = (ArraySegment<byte>)(entity.client == client ? owner : other);
                }

                client.entities.Add(entity);
                client.Send(new SpawnMessage(entity, client, segment));
            }
        }

        public static void Remove(NetworkEntity entity, NetworkClient client)
        {
            var clients = entity.clients;
            if (clients.Remove(client))
            {
                if (clients.Count == 0)
                {
                    entity.ClearDirty(true);
                }

                client.entities.Remove(entity);
                client.Send(new DespawnMessage(entity.objectId));
            }
        }

        public static void Clear(NetworkEntity entity)
        {
            var clients = entity.clients;
            foreach (var client in clients)
            {
                client.entities.Remove(entity);
            }

            clients.Clear();
        }

        public static void Clear(NetworkClient client)
        {
            var entities = client.entities;
            foreach (var entity in entities.ToList())
            {
                var clients = entity.clients;
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
}