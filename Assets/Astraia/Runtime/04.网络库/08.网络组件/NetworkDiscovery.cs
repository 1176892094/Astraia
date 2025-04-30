using System;
using System.Net;
using System.Net.Sockets;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    public class NetworkDiscovery : MonoBehaviour
    {
        public static NetworkDiscovery Instance;

        [SerializeField] private string address = IPAddress.Broadcast.ToString();

        [SerializeField] private ushort port = 47777;

        public int version;

        public int duration = 1;

        private UdpClient udpClient;

        private UdpClient udpServer;

        private void Awake()
        {
            Instance = this;
        }

        public void StartDiscovery()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError(Log.E292);
                return;
            }

            StopDiscovery();
            if (NetworkManager.Server.isActive)
            {
                udpServer = new UdpClient(port)
                {
                    EnableBroadcast = true,
                    MulticastLoopback = false
                };
                MulticastLock(true);
                ServerReceive();
            }
            else
            {
                udpClient = new UdpClient(0)
                {
                    EnableBroadcast = true,
                    MulticastLoopback = false
                };
                ClientReceive();
                InvokeRepeating(nameof(ClientSend), 0, duration);
            }
        }

        public void StopDiscovery()
        {
            MulticastLock(false);
            udpServer?.Close();
            udpClient?.Close();
            udpServer = null;
            udpClient = null;
            CancelInvoke();
        }

        private void OnDestroy()
        {
            StopDiscovery();
        }

        private void ClientSend()
        {
            try
            {
                if (NetworkManager.Client.isConnected)
                {
                    StopDiscovery();
                    return;
                }

                var endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(address), port);
                }

                using var setter = MemorySetter.Pop();
                setter.SetInt(version);
                setter.Invoke(new RequestMessage());
                ArraySegment<byte> segment = setter;
                udpClient.Send(segment.Array, segment.Count, endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async void ServerReceive()
        {
            while (udpServer != null)
            {
                try
                {
                    var result = await udpServer.ReceiveAsync();
                    using var getter = MemoryGetter.Pop(new ArraySegment<byte>(result.Buffer));
                    if (version != getter.GetInt())
                    {
                        Debug.LogError(Log.E293);
                        return;
                    }

                    getter.Invoke<RequestMessage>();
                    ServerSend(result.RemoteEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void ServerSend(IPEndPoint endPoint)
        {
            try
            {
                using var setter = MemorySetter.Pop();
                setter.SetInt(version);
                setter.Invoke(new ResponseMessage(new UriBuilder
                {
                    Scheme = "https",
                    Host = Dns.GetHostName(),
                    Port = Transport.Instance.port
                }.Uri));
                ArraySegment<byte> segment = setter;
                udpServer.Send(segment.Array, segment.Count, endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async void ClientReceive()
        {
            while (udpClient != null)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    using var getter = MemoryGetter.Pop(new ArraySegment<byte>(result.Buffer));
                    if (version != getter.GetInt())
                    {
                        Debug.LogError(Log.E293);
                        return;
                    }

                    var endPoint = result.RemoteEndPoint;
                    var response = getter.Invoke<ResponseMessage>();
                    EventManager.Invoke(new ServerResponse(new UriBuilder(response.uri)
                    {
                        Host = endPoint.Address.ToString()
                    }.Uri, endPoint));
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

#if UNITY_ANDROID
        private bool multicast;
        private AndroidJavaObject multicastLock;
#endif

        private void MulticastLock(bool enabled)
        {
#if UNITY_ANDROID
            if (enabled)
            {
                if (multicast) return;
                using var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                using var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
                multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "lock");
                multicastLock.Call("acquire");
                multicast = true;
            }
            else
            {
                if (!multicast) return;
                multicastLock?.Call("release");
                multicast = false;
            }

#endif
        }
    }
}