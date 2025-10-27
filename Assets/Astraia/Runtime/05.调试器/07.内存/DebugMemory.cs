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
        private readonly Dictionary<int, float> minMemory = new Dictionary<int, float>();
        private readonly Dictionary<int, float> maxMemory = new Dictionary<int, float>();

        private void MemoryWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 内存信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            var pair1 = Calculate(1, Profiler.GetTotalReservedMemoryLong());
            var pair2 = Calculate(2, Profiler.GetTotalAllocatedMemoryLong());
            var pair3 = Calculate(3, Profiler.GetTotalUnusedReservedMemoryLong());
            var pair4 = Calculate(4, Profiler.GetAllocatedMemoryForGraphicsDriver());
            var pair5 = Calculate(5, Profiler.GetMonoHeapSizeLong());
            var pair6 = Calculate(6, Profiler.GetMonoUsedSizeLong());
            
            screenView = GUILayout.BeginScrollView(screenView, "Box");
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("已保留的内存总量: " + pair1.Item1);
            GUILayout.Label("已分配的内存总量: " + pair2.Item1);
            GUILayout.Label("未使用的内存总量: " + pair3.Item1);
            GUILayout.Label("图形资源使用内存: " + pair4.Item1);
            GUILayout.Label("Mono分配的托管堆: " + pair5.Item1);
            GUILayout.Label("Mono使用的托管堆: " + pair6.Item1);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label(pair1.Item2);
            GUILayout.Label(pair2.Item2);
            GUILayout.Label(pair3.Item2);
            GUILayout.Label(pair4.Item2);
            GUILayout.Label(pair5.Item2);
            GUILayout.Label(pair6.Item2);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("垃圾回收", GUILayout.Height(30)))
            {
                GC.Collect();
            }

            GUILayout.EndHorizontal();
        }

        private (string, string) Calculate(int key, long memory)
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

            var item1 = "{0:F2} MB".Format(value);
            var item2 = "[ 最小值: {0:F2} MB  \t最大值: {1:F2} MB]".Format(minMemory[key], maxMemory[key]);
            return (item1, item2);
        }
    }
}