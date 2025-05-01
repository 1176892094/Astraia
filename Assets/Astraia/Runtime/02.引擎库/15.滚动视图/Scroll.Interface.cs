using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Common
{
    public interface IScroll<TItem> : IAgent
    {
        public bool selection { get; set; }
        public Rect assetRect { get; set; }
        public string assetPath { get; set; }
        public UIState direction { get; set; }
        void SetItem(IList<TItem> items);
        void Move(Component grid, int direction);
    }
}