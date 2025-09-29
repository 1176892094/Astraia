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

        [SerializeField] private int version;

        [SerializeField] private int duration = 1;

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
                Log.Error("网络发现不支持WebGL");
                return;
            }

            StopDiscovery();
            if (NetworkManager.isServer)
            {
                udpServer = new UdpClient(port)
                {
                    EnableBroadcast = true,
                    MulticastLoopback = false
                };
#if UNITY_ANDROID
                MulticastLock(true);
#endif
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
#if UNITY_ANDROID
            MulticastLock(false);
#endif
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
                if (NetworkManager.Client.isActive)
                {
                    StopDiscovery();
                    return;
                }

                var endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(address), port);
                }

                using var writer = MemoryWriter.Pop();
                writer.WriteInt(version);
                writer.Invoke(new RequestMessage());
                ArraySegment<byte> segment = writer;
                udpClient.Send(segment.Array!, segment.Count, endPoint);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async void ServerReceive()
        {
            while (udpServer != null)
            {
                try
                {
                    var result = await udpServer.ReceiveAsync();
                    using var reader = MemoryReader.Pop(new ArraySegment<byte>(result.Buffer));
                    if (version != reader.ReadInt())
                    {
                        Log.Error("接收到的消息版本不同!");
                        return;
                    }

                    reader.Invoke<RequestMessage>();
                    ServerSend(result.RemoteEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private void ServerSend(IPEndPoint endPoint)
        {
            try
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteInt(version);
                writer.Invoke(new ResponseMessage(new UriBuilder
                {
                    Scheme = "https",
                    Host = Dns.GetHostName(),
                    Port = Transport.Instance.port
                }.Uri));
                ArraySegment<byte> segment = writer;
                udpServer.Send(segment.Array!, segment.Count, endPoint);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async void ClientReceive()
        {
            while (udpClient != null)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    using var reader = MemoryReader.Pop(new ArraySegment<byte>(result.Buffer));
                    if (version != reader.ReadInt())
                    {
                        Log.Error("接收到的消息版本不同!");
                        return;
                    }

                    var endPoint = result.RemoteEndPoint;
                    var response = reader.Invoke<ResponseMessage>();
                    EventManager.Invoke(new ServerResponse(new UriBuilder(response.uri) { Host = endPoint.Address.ToString() }.Uri));
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

#if UNITY_ANDROID
        private bool multicast;
        private AndroidJavaObject multicastLock;

        private void MulticastLock(bool enabled)
        {

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
        }
#endif
    }
}