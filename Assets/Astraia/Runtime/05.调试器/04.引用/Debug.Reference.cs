// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-18 21:12:36
// # Recently: 2024-12-22 20:12:31
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private readonly Dictionary<string, List<Pool>> poolData = new Dictionary<string, List<Pool>>();
        private PoolMode windowOption = PoolMode.Heap;

        private void ReferenceWindow()
        {
            GUILayout.BeginHorizontal();
            GUI.contentColor = windowOption == PoolMode.Heap ? Color.white : Color.gray;
            if (GUILayout.Button("Heap", GUILayout.Height(30)))
            {
                windowOption = PoolMode.Heap;
            }

            GUI.contentColor = windowOption == PoolMode.Event ? Color.white : Color.gray;
            if (GUILayout.Button("Event", GUILayout.Height(30)))
            {
                windowOption = PoolMode.Event;
            }

            GUI.contentColor = windowOption == PoolMode.Pool ? Color.white : Color.gray;
            if (GUILayout.Button("Pool", GUILayout.Height(30)))
            {
                windowOption = PoolMode.Pool;
            }

            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
            switch (windowOption)
            {
                case PoolMode.Heap:
                    Draw(HeapManager.Reference(), "引用池", "未使用\t\t使用中\t\t使用次数\t\t释放次数");
                    break;
                case PoolMode.Event:
                    Draw(EventManager.Reference(), "事件池", "触发数\t\t事件数\t\t添加次数\t\t移除次数");
                    break;
                case PoolMode.Pool:
                    Draw(PoolManager.Reference(), "对象池", "未激活\t\t激活中\t\t出队次数\t\t入队次数");
                    break;
            }
        }

        private void Draw(IList<Pool> references, string message, string module)
        {
            poolData.Clear();
            foreach (var reference in references)
            {
                var assemblyName = Service.Text.Format("{0} - {1}", reference.type.Assembly.GetName().Name, message);
                if (!poolData.TryGetValue(assemblyName, out var results))
                {
                    results = new List<Pool>();
                    poolData.Add(assemblyName, results);
                }

                results.Add(reference);
            }

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            foreach (var poolPair in poolData)
            {
                poolPair.Value.Sort(Comparison);
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 28) / 2));
                GUILayout.Label(poolPair.Key, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    var assetName = data.type.Name;
                    if (!string.IsNullOrEmpty(data.path))
                    {
                        assetName = Service.Text.Format("{0} - {1}", data.type.Name, data.path);
                    }

                    GUILayout.Label(assetName, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                GUILayout.Label(module, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    GUILayout.Label(data.ToString(), GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private static int Comparison(Pool origin, Pool target)
        {
            return string.Compare(origin.type.Name, target.type.Name, StringComparison.Ordinal);
        }

        private enum PoolMode
        {
            Heap,
            Event,
            Pool,
        }
    }
}