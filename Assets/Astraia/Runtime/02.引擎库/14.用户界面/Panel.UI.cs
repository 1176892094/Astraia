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

using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    public abstract class UIPanel : MonoBehaviour
    {
        public UILayer layer = UILayer.Layer2;
        
        public UIState state = UIState.Common;
        
        public List<string> groups = new List<string>();

        protected virtual void Awake()
        {
            this.Inject();
        }

        protected virtual void OnDestroy()
        {
            groups.Clear();
            groups = null;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}