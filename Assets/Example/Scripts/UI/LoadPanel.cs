// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-22 23:09:41
// // # Recently: 2025-09-22 23:09:42
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using Astraia.Common;
using UnityEngine.UI;

namespace Runtime
{
    [UIMask(2)]
    public class LoadPanel : UIPanel, ITween
    {
        [Inject] public Image panel;

        Tween ITween.OnShow()
        {
            var animation = panel.DOFade(1, 0.25f);
            animation.OnComplete(UIManager.Hide<LoadPanel>);
            return animation;
        }

        Tween ITween.OnHide()
        {
            return panel.DOFade(0, 0.5f);
        }
    }
}