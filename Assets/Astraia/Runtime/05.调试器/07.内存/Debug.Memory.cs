// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-15 22:12:17
// # Recently: 2024-12-22 20:12:47
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private readonly Dictionary<string, float> minMemory = new Dictionary<string, float>();
        private readonly Dictionary<string, float> maxMemory = new Dictionary<string, float>();

        private void MemoryWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 内存信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label(Calculate("已保留的内存总量", Profiler.GetTotalReservedMemoryLong()));
            GUILayout.Label(Calculate("已分配的内存总量", Profiler.GetTotalAllocatedMemoryLong()));
            GUILayout.Label(Calculate("未使用的内存总量", Profiler.GetTotalUnusedReservedMemoryLong()));
            GUILayout.Label(Calculate("图形资源使用内存", Profiler.GetAllocatedMemoryForGraphicsDriver()));
            GUILayout.Label(Calculate("Mono分配的托管堆", Profiler.GetMonoHeapSizeLong()));
            GUILayout.Label(Calculate("Mono使用的托管堆", Profiler.GetMonoUsedSizeLong()));
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("垃圾回收", GUILayout.Height(30)))
            {
                GC.Collect();
            }

            GUILayout.EndHorizontal();
        }

        private string Calculate(string key, long memory)
        {
            var value = memory / 1024F / 1024F;
            if (!minMemory.TryGetValue(key, out var minValue))
            {
                minValue = 1024 * 1024;
                minMemory.Add(key, minValue);
            }

            if (!maxMemory.TryGetValue(key, out var maxValue))
            {
                maxValue = 0;
                maxMemory.Add(key, maxValue);
            }

            if (value > maxValue)
            {
                maxMemory[key] = value;
            }
            else if (value < minValue)
            {
                minMemory[key] = value;
            }

            return Service.Text.Format("{0}:  \t{1:F2} MB\t\t[ 最小值: {2:F2}\t最大值: {3:F2} ]", key, value, minMemory[key], maxMemory[key]);
        }
    }
}