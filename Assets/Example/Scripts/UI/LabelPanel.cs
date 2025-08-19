// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-21 02:04:48
// // # Recently: 2025-04-21 02:04:48
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using UnityEngine.UI;

namespace Runtime
{
    public class LabelPanel : UIPanel
    {
        [Inject] private Text message;
        [Inject] private Button prevButton;
        [Inject] private Button nextButton;
        
        public override void OnShow()
        {
            message.text = "1.点击调试器\n2.找到Network面板\n3.房主启动 Host 模式\n4.房员启动 Client 模式";
        }

        private void PrevButton()
        {
            message.text = "1.点击调试器\n2.找到Network面板\n3.房主启动 Host 模式\n4.房员启动 Client 模式";
        }

        private void NextButton()
        {
            message.text = "1.使用方向键进行移动\n2.使用Z键抓取墙壁\n3.使用X键进行跳跃\n4.使用C键进行冲刺";
        }
    }
}