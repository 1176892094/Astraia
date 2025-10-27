// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-08-24 17:08:14
// # Recently: 2024-12-22 20:12:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private void ProjectWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 环境配置", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("项目名称: " + Application.productName);
            GUILayout.Label("项目版本: " + Application.version);
            GUILayout.Label("运行平台: " + Application.platform);
            GUILayout.Label("项目标识: " + Application.identifier);
            GUILayout.Label("公司名称: " + Application.companyName);
            GUILayout.Label("Unity版本: " + Application.unityVersion);
            GUILayout.Label("Unity专业版: " + Application.HasProLicense());
            var message = "";
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    message = "当前设备无法访问互联网";
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    message = "当前设备通过 蜂窝移动网络 连接到互联网";
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    message = "当前设备通过 WiFi 或有线网络连接到互联网";
                    break;
            }

            GUILayout.Label("网络状态: " + message);
            GUILayout.Label("项目路径: " + Application.dataPath);
            GUILayout.Label("存储路径: " + Application.persistentDataPath);
            GUILayout.Label("流动资源路径: " + Application.streamingAssetsPath);
            GUILayout.Label("临时缓存路径: " + Application.temporaryCachePath);

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.5x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(3200, 1800);
            }

            if (GUILayout.Button("1.0x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(2560, 1440);
            }

            if (GUILayout.Button("1.5x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(1920, 1080);
            }

            if (GUILayout.Button("2.0x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(1280, 720);
            }

            GUILayout.EndHorizontal();
        }
    }
}