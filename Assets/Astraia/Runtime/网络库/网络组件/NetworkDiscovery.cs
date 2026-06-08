using System;
using System.Net;
using System.Net.Sockets;
using Astraia.Core;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    internal class NetworkDiscovery : Singleton<NetworkDiscovery>
    {
        [SerializeField] private string address;
        [SerializeField] private ushort port = 47777;
        private UdpClient udpClient;
        private UdpClient udpServer;

        public override void Enqueue()
        {
            StopDiscovery();
        }

        public void StartDiscovery()
        {
            StopDiscovery();
            if (NetworkManager.isServer)
            {
                udpServer = new UdpClient(port) { EnableBroadcast = true, MulticastLoopback = false };
#if UNITY_ANDROID
                MulticastLock(true);
#endif
                ServerReceive();
            }
            else
            {
                udpClient = new UdpClient(0) { EnableBroadcast = true, MulticastLoopback = false };
                ClientReceive();
                ClientSend();
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
        }

        private void ClientSend()
        {
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(address), port);
                }

                using var writer = MemoryWriter.Pop();
                writer.Invoke(new RequestMessage());
                ArraySegment<byte> segment = writer;
                udpClient.Send(segment.Array!, segment.Count, endPoint);
            }
            catch (SocketException)
            {
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
                writer.Invoke(new ResponseMessage(NetworkManager.kcp.port));
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
                    var response = reader.Invoke<ResponseMessage>();
                    EventManager.Invoke(new ServerResponse(result.RemoteEndPoint.Address.ToString(), response.port));
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