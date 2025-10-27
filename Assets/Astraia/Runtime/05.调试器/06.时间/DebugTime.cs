// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-08-24 17:08:14
// # Recently: 2024-12-22 20:12:42
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private void TimeWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 时间信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("DataTime:");
            GUILayout.Label("Time.frameCount:");
            GUILayout.Label("Time.realtimeSinceStartup:");
            GUILayout.Label("Time.timeScale:");
            GUILayout.Label("Time.time:");
            GUILayout.Label("Time.deltaTime:");
            GUILayout.Label("Time.unscaledTime:");
            GUILayout.Label("Time.unscaledDeltaTime:");
            GUILayout.Label("Time.fixedTime:");
            GUILayout.Label("Time.fixedDeltaTime:");
            GUILayout.Label("Time.fixedUnscaledTime:");
            GUILayout.Label("Time.fixedUnscaledDeltaTime:");
        
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            GUILayout.Label(Time.frameCount.ToString());
            GUILayout.Label(Time.realtimeSinceStartup.ToString("F"));
            GUILayout.Label(Time.timeScale.ToString("F"));
            GUILayout.Label(Time.time.ToString("F"));
            GUILayout.Label(Time.deltaTime.ToString("F"));
            GUILayout.Label(Time.unscaledTime.ToString("F"));
            GUILayout.Label(Time.unscaledDeltaTime.ToString("F"));
            GUILayout.Label(Time.fixedTime.ToString("F"));
            GUILayout.Label(Time.fixedDeltaTime.ToString("F"));
            GUILayout.Label(Time.fixedUnscaledTime.ToString("F"));
            GUILayout.Label(Time.fixedUnscaledDeltaTime.ToString("F"));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.0x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.0f;
            }

            if (GUILayout.Button("0.1x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.1f;
            }

            if (GUILayout.Button("0.2x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.2f;
            }

            if (GUILayout.Button("0.5x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.5f;
            }

            if (GUILayout.Button("1x", GUILayout.Height(30)))
            {
                Time.timeScale = 1f;
            }

            if (GUILayout.Button("2x", GUILayout.Height(30)))
            {
                Time.timeScale = 2f;
            }

            if (GUILayout.Button("5x ", GUILayout.Height(30)))
            {
                Time.timeScale = 5f;
            }

            if (GUILayout.Button("10x", GUILayout.Height(30)))
            {
                Time.timeScale = 10f;
            }

            GUILayout.EndHorizontal();
        }
    }
}