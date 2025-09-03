// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 17:01:14
// # Recently: 2025-01-08 17:01:30
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class DataTable<T> : ScriptableObject, IDataTable where T : IData
    {
        [SerializeField] internal List<T> items = new List<T>();

        public int Count => items.Count;

        public void AddData(IData data) => items.Add((T)data);
    }
}