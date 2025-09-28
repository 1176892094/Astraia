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

using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Net
{
    public abstract class NetworkObserver : MonoBehaviour
    {
        private readonly HashSet<NetworkClient> observers = new HashSet<NetworkClient>();

        private void Awake()
        {
            NetworkManager.Server.observer = this;
        }

        public void Rebuild(NetworkEntity entity, bool initialize)
        {
            observers.Clear();
            if (entity.spawn != EntitySpawn.Hide)
            {
                OnRebuild(entity, observers);
            }

            if (entity.client != null)
            {
                observers.Add(entity.client);
            }

            var changed = false;
            foreach (var client in observers)
            {
                if (client.isReady)
                {
                    if (initialize || !entity.clients.Contains(client))
                    {
                        NetworkManager.Server.AddToClient(entity, client);
                        changed = true;
                    }
                }
            }

            foreach (NetworkClient client in entity.clients)
            {
                if (!observers.Contains(client))
                {
                    NetworkManager.Server.SubToClient(entity, client, false);
                    changed = true;
                }
            }

            if (changed)
            {
                entity.clients.Clear();
                foreach (var client in observers)
                {
                    if (client.isReady)
                    {
                        entity.clients.Add(client);
                    }
                }
            }
        }

        public abstract bool OnExecute(NetworkEntity entity, NetworkClient client);
        public abstract void OnRebuild(NetworkEntity entity, HashSet<NetworkClient> clients);
    }
}