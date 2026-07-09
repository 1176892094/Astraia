using System;
using Astraia;
using Astraia.Core;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace Runtime
{
    [UIMask(2)] [Serializable]
    public class LoadPanel : UIPanel, ITween
    {
        [Inject] public Image panel;

        public override async void OnShow()
        {
            await panel.DOFade(1, 0.25f);
            UIManager.Hide<LoadPanel>();
        }

        public override async void OnHide()
        {
            await panel.DOFade(0, 0.5f);
            gameObject.SetActive(false);
            UIManager.Show<LabelPanel>();
        }
    }

    [UIMask(1)] [Serializable]
    public class LabelPanel : UIPanel
    {
        [Inject] public Text message;
        [Inject] public Button prevButton;
        [Inject] public Button nextButton;

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