// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-10 20:01:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;


namespace Astraia.Common
{
    public interface IMove
    {
        void Move(IGrid grid, int move);
    }

    public interface IGrid
    {
        Transform transform { get; }

        GameObject gameObject { get; }

        void Select();

        void Dispose();
    }

    public interface IGrid<T> : IGrid
    {
        T item { get; }
        void SetItem(T item);
    }
}