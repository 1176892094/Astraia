using Astraia;
using Astraia.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public class CameraPanel : Export, IEvent<ServerReady>
    {
        [Export] private Button hostButton;
        [Export] private Button clientButton;
        [Export] private Button startButton;

        protected override void Awake()
        {
            hostButton.gameObject.SetActive(true);
            startButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(true);
        }

        private void HostButton()
        {
            NetworkManager.StartHost();
            hostButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            clientButton.gameObject.SetActive(false);
        }

        private void ClientButton()
        {
            NetworkManager.StartClient();

            hostButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(false);
        }

        private void StartButton()
        {
            if (NetworkManager.Server.isReady)
            {
                foreach (var client in NetworkManager.Server.clients.Values)
                {
                    NetworkManager.Server.Spawn(AssetManager.Load<GameObject>("Prefabs/10001"), client);
                }

                SyncManager.Instance.playerCount = NetworkManager.Server.connections;
            }

            startButton.gameObject.SetActive(false);
        }

        public void Execute(ServerReady message)
        {
            if (NetworkManager.Server.connections == 1)
            {
                NetworkManager.Server.Spawn(AssetManager.Load<GameObject>("Prefabs/10004"));
            }
        }
    }
}