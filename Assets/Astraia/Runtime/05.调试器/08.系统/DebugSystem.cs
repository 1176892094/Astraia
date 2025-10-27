// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-08-24 17:08:14
// # Recently: 2024-12-22 20:12:48
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private void SystemWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 系统信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("操作系统: " + SystemInfo.operatingSystem);
            GUILayout.Label("系统内存: " + SystemInfo.systemMemorySize + "MB");
            GUILayout.Label("处理器: " + SystemInfo.processorType);
            GUILayout.Label("处理器数量: " + SystemInfo.processorCount);
            GUILayout.Label("显卡名称: " + SystemInfo.graphicsDeviceName);
            GUILayout.Label("显卡类型: " + SystemInfo.graphicsDeviceType);
            GUILayout.Label("显卡内存: " + SystemInfo.graphicsMemorySize + "MB");
            GUILayout.Label("显卡标识: " + SystemInfo.graphicsDeviceID);
            GUILayout.Label("显卡供应商: " + SystemInfo.graphicsDeviceVendor);
            GUILayout.Label("显卡供应商标识: " + SystemInfo.graphicsDeviceVendorID);
            GUILayout.Label("设备模式: " + SystemInfo.deviceModel);
            GUILayout.Label("设备名称: " + SystemInfo.deviceName);
            GUILayout.Label("设备类型: " + SystemInfo.deviceType);
            GUILayout.Label("设备唯一标识符: " + SystemInfo.deviceUniqueIdentifier);
            GUILayout.EndScrollView();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("退出游戏", GUILayout.Height(30)))
            {
                Application.Quit();
            }
            
            GUILayout.EndHorizontal();
        }
    }
}