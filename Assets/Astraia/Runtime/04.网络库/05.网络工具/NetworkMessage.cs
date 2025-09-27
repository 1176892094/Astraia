// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 18:01:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************


namespace Astraia.Net
{
    public static class NetworkMessage<T>
    {
        public static readonly ushort Id = (ushort)NetworkMessage.Id(typeof(T).FullName);
    }

    public static class NetworkMessage
    {
        public static uint Id(string name)
        {
            var result = 23U;
            unchecked
            {
                foreach (var c in name)
                {
                    result = result * 31 + c;
                }

                return result;
            }
        }
    }
}