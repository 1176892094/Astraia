// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-30 17:11:43
// # Recently: 2024-12-22 20:12:07
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Astraia
{
    internal static class Utils
    {
        public static bool IsReliable(byte value, out Reliable header)
        {
            if (Enum.IsDefined(typeof(Reliable), value))
            {
                header = (Reliable)value;
                return true;
            }

            header = Reliable.Ping;
            return false;
        }

        public static bool IsUnreliable(byte value, out Unreliable header)
        {
            if (Enum.IsDefined(typeof(Unreliable), value))
            {
                header = (Unreliable)value;
                return true;
            }

            header = Unreliable.Disconnect;
            return false;
        }

        public static void SetSocket(Socket socket, int buffer = 1024 * 1024 * 7)
        {
            socket.Blocking = false;
            var sendBuffer = socket.SendBufferSize;
            var receiveBuffer = socket.ReceiveBufferSize;
            try
            {
                socket.SendBufferSize = buffer;
                socket.ReceiveBufferSize = buffer;
            }
            catch (SocketException)
            {
                Logs.Info(Service.Text.Format(Log.E101, buffer, sendBuffer, sendBuffer / buffer));
                Logs.Info(Service.Text.Format(Log.E102, buffer, receiveBuffer, receiveBuffer / buffer));
            }
        }
    }
}