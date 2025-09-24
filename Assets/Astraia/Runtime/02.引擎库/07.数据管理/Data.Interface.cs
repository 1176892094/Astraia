// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-04 02:09:24
// // # Recently: 2025-09-04 02:09:24
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia.Common
{
    public interface IData
    {
        void Create(string[] sheet, int column);
    }

    internal interface IDataTable
    {
        void AddData(IData data);

        void AddData<T>(string name);
    }
}