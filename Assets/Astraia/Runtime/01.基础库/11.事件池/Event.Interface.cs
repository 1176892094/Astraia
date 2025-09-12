// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-11 18:01:31
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Common
{
    public interface IEvent
    {
    }

    public interface IEvent<in T> where T : struct, IEvent
    {
        void Execute(T message);
    }
    
    public interface IAutoEvent<in T> : IEvent<T> where T : struct, IEvent
    {
        void Listen() => EventManager.Listen(this);
        void Remove() => EventManager.Remove(this);
    }
}