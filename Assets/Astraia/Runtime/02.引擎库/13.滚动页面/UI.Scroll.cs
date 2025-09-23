// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-02-12 01:02:28
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class UIPanel<T, TGrid> : UIPanel, IMove, ISystem, IEnumerable<KeyValuePair<int, TGrid>> where TGrid : Component, IGrid<T>
    {
        private readonly Dictionary<int, TGrid> grids = new Dictionary<int, TGrid>();
        private string assetName;
        private IList<T> items;
        private int minIndex;
        private int maxIndex;
        private int numIndex;
        private bool selected;
        private bool vertical;
        private bool restarted;
        private bool selection;
        private Rect assetRect;
        private string assetPath;

        [Inject] public RectTransform content;
        private int numX => (int)assetRect.x + (vertical ? 0 : 1);
        private int numY => (int)assetRect.y + (vertical ? 1 : 0);

        internal override void Create(Entity owner)
        {
            base.Create(owner);
            var panelAttr = Attribute<UIRectAttribute>.GetAttribute(GetType());
            if (panelAttr != null)
            {
                vertical = panelAttr.vertical;
                selection = panelAttr.selection;
                assetRect = panelAttr.assetRect;
            }

            assetName = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
            assetPath = assetName;

            var asstAttr = Attribute<UIPathAttribute>.GetAttribute(typeof(TGrid));
            if (asstAttr != null)
            {
                assetPath = GlobalSetting.Prefab.Format(asstAttr.assetPath);
            }

            restarted = false;
            owner.OnHide += Reload;
        }

        void ISystem.Update()
        {
            Scroll();
            Update();
        }

        private void Scroll()
        {
            if (!content)
            {
                return;
            }

            if (!restarted)
            {
                restarted = true;
                if (vertical)
                {
                    content.anchorMin = Vector2.up;
                    content.anchorMax = Vector2.one;
                }
                else
                {
                    content.anchorMin = Vector2.zero;
                    content.anchorMax = Vector2.up;
                }

                content.pivot = Vector2.up;
            }

            if (items == null)
            {
                return;
            }

            if (numIndex != items.Count)
            {
                Reload(false);
                numIndex = items.Count;
            }

            int min;
            int max;
            int idx;
            float pos;
            if (vertical)
            {
                pos = content.anchoredPosition.y;
                idx = (int)(pos / assetRect.height);
                min = idx * numX;
                max = (idx + numY) * numX - 1;
            }
            else
            {
                pos = -content.anchoredPosition.x;
                idx = (int)(pos / assetRect.width);
                min = idx * numY;
                max = (idx + numX) * numY - 1;
            }

            if (min < 0)
            {
                min = 0;
            }

            if (max > items.Count - 1)
            {
                max = items.Count - 1;
            }

            if (min != minIndex || max != maxIndex)
            {
                for (var i = minIndex; i < min; ++i)
                {
                    if (grids.TryGetValue(i, out var grid))
                    {
                        if (grid)
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid.gameObject);
                        }

                        grids.Remove(i);
                    }
                }

                for (var i = max + 1; i <= maxIndex; ++i)
                {
                    if (grids.TryGetValue(i, out var grid))
                    {
                        if (grid)
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid.gameObject);
                        }

                        grids.Remove(i);
                    }
                }
            }

            minIndex = min;
            maxIndex = max;
            Load(min, max);
        }

        private void Load(int min, int max)
        {
            for (var i = min; i <= max; i++)
            {
                if (grids.ContainsKey(i))
                {
                    continue;
                }

                float posX;
                float posY;
                if (vertical)
                {
                    var idx = i / numX;
                    posX = i % numX * assetRect.width + assetRect.width / 2;
                    posY = -idx * assetRect.height - assetRect.height / 2;
                }
                else
                {
                    var idx = i / numY;
                    posX = idx * assetRect.width + assetRect.width / 2;
                    posY = -(i % numY) * assetRect.height - assetRect.height / 2;
                }

                grids[i] = null;
                var grid = PoolManager.Show(assetPath, assetName).GetOrAddComponent<TGrid>();
                SetGrid(grid.GetComponent<RectTransform>(), posX, posY);
                if (!grids.ContainsKey(i))
                {
                    grid.Dispose();
                    PoolManager.Hide(grid.gameObject);
                    return;
                }

                grids[i] = grid;
                if (selected && i == max)
                {
                    selected = false;
                    grids[min].Select();
                }

                grid.SetItem(items[i]);
            }
        }

        private void SetGrid(RectTransform transform, float posX, float posY)
        {
            transform.transform.SetParent(content);
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(posX, posY, 0);
            transform.sizeDelta = new Vector2(assetRect.width, assetRect.height);
        }

        public void SetItem(IList<T> items)
        {
            this.items = items;
            if (items != null)
            {
                float value = items.Count;
                if (vertical)
                {
                    value = Mathf.Ceil(value / numX);
                    content.sizeDelta = new Vector2(0, value * assetRect.height);
                }
                else
                {
                    value = Mathf.Ceil(value / numY);
                    content.sizeDelta = new Vector2(value * assetRect.width, 0);
                }

                numIndex = items.Count;
            }

            Reload(false);
            selected = selection;
            content.anchoredPosition = Vector2.zero;
        }

        public void Reload()
        {
            Reload(true);
        }

        public void Reload(bool remove)
        {
            minIndex = -1;
            maxIndex = -1;
            foreach (var i in grids.Keys)
            {
                if (grids.TryGetValue(i, out var grid))
                {
                    if (grid)
                    {
                        grid.Dispose();
                        PoolManager.Hide(grid.gameObject);
                    }
                }
            }

            if (remove)
            {
                items = null;
            }

            grids.Clear();
        }

        public void Move(IGrid grid, int move)
        {
            switch (move)
            {
                case 0 when !vertical:
                    for (int i = 0; i < numY; i++)
                    {
                        if (grids.TryGetValue(minIndex + i + numY, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.right * assetRect.width;
                            return;
                        }
                    }

                    return;
                case 1 when vertical:
                    for (int i = 0; i < numX; i++)
                    {
                        if (grids.TryGetValue(minIndex + i + numX, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.down * assetRect.height;
                            return;
                        }
                    }

                    return;
                case 2 when !vertical:
                    for (int i = 0; i < numY; i++)
                    {
                        if (grids.TryGetValue(maxIndex - i - numY, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.left * assetRect.width;
                            return;
                        }
                    }

                    return;
                case 3 when vertical:
                    for (int i = 0; i < numX; i++)
                    {
                        if (grids.TryGetValue(maxIndex - i - numX, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.up * assetRect.height;
                            return;
                        }
                    }

                    return;
            }
        }

        public IEnumerator<KeyValuePair<int, TGrid>> GetEnumerator()
        {
            return grids.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}