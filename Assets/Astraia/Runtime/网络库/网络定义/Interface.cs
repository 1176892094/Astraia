// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-29 13:11:20
// # Recently: 2024-12-22 20:12:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Core
{
    public interface IStartClient
    {
        void OnStartClient();
    }

    public interface IStopClient
    {
        void OnStopClient();
    }

    public interface IStartServer
    {
        void OnStartServer();
    }

    public interface IStopServer
    {
        void OnStopServer();
    }

    public interface IStartAuthority
    {
        void OnStartAuthority();
    }

    public interface IStopAuthority
    {
        void OnStopAuthority();
    }
}