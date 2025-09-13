// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 23:01:36
// # Recently: 2025-01-10 20:01:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Common;

namespace Astraia
{
    [Serializable]
    public abstract partial class UIPanel : Module<Entity>, IActive, IPanel
    {
        public UIState state = UIState.Common;
        public UILayer layer = UILayer.Layer1;

        public virtual void Update()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }
    }

    public abstract partial class UIPanel
    {
        internal HashSet<int> group = new HashSet<int>();
        
        internal void Listen()
        {
            GlobalManager.panelPage.Add(this);
        }

        internal void Remove()
        {
            GlobalManager.panelPage.Remove(this);
        }
    }
}