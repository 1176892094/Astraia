// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 20:01:36
// # Recently: 2025-01-10 20:01:56
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Common
{
    public interface IGrid<TItem> : IGrid
    {
        TItem item { get; }
        void SetItem(TItem item);
    }
}