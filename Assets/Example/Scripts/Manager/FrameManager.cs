using System;
using System.Collections.Generic;
using Astraia.Core;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public struct InputMessage : IMessage
    {
        public uint objectId;

        public InputMessage(uint objectId)
        {
            this.objectId = objectId;
        }
    }

    [Serializable]
    public struct FrameMessage : IMessage
    {
        public int frame;
        public Vector2Int[] points;

        public FrameMessage(int frame, Vector2Int[] points)
        {
            this.frame = frame;
            this.points = points;
        }
    }

    public class FrameManager : MonoBehaviour
    {
        private readonly Dictionary<int, Dictionary<uint, Vector2Int>> buffer = new Dictionary<int, Dictionary<uint, Vector2Int>>();
        private int current;
        public int playerCount;

        private void Awake()
        {
            NetworkMessage<InputMessage>.Add(InputMessage);
            NetworkMessage<FrameMessage>.Add(FrameMessage);
        }

        public void FixedUpdate()
        {
            var frame = current;
            if (buffer.TryGetValue(frame, out var input))
            {
                if (input.Count >= playerCount)
                {
                    var index = 0;
                    var points = new Vector2Int[input.Count];
                    foreach (var value in input.Values)
                    {
                        points[index++] = value;
                    }

                    foreach (var client in NetworkManager.Server.clients.Values)
                    {
                        client.Send(new FrameMessage(frame, points));
                    }

                    current++;
                    buffer.Remove(frame);
                }
            }
        }

        private void InputMessage(NetworkClient client, InputMessage message)
        {
            if (NetworkManager.Server.connections == playerCount && NetworkManager.Server.isReady)
            {
                if (!buffer.TryGetValue(current, out var input))
                {
                    input = new Dictionary<uint, Vector2Int>();
                    buffer[current] = input;
                }

                var entity = (NetworkEntity)message.objectId;
                var position = entity.transform.position;
                input[message.objectId] = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            }
        }

        private void FrameMessage(FrameMessage message)
        {
            Debug.Log(message.frame);
        }
    }
}