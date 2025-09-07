// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-19 02:12:55
// # Recently: 2024-12-22 20:12:37
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private void ScreenWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 屏幕信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("像素密度: " + Screen.dpi);
            GUILayout.Label("启用全屏: " + Screen.fullScreen);
            GUILayout.Label("屏幕模式: " + Screen.fullScreenMode);
            GUILayout.Label("程序分辨率: " + "{0} x {1}".Format(Screen.width, Screen.height));
            GUILayout.Label("设备分辨率: " + Screen.currentResolution);
            GUILayout.Label("显示区域: " + Screen.safeArea);
            GUILayout.Label("质量等级: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                var label = QualitySettings.names[i];
                if (GUILayout.Button(label, GUILayout.Height(30)))
                {
                    QualitySettings.SetQualityLevel(i);
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}