// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-30 16:08:44
// # Recently: 2026-08-30 16:21:44
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Linq;

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

                client.entities.Add(entity);
                using var owner = MemoryWriter.Pop();
                using var other = MemoryWriter.Pop();
                if (entity.modules.Length > 0)
                {
                    entity.modules.ServerSend(owner, other, true);
                }

                client.Send(new SpawnMessage(entity, client, entity.client == client ? owner : other));
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