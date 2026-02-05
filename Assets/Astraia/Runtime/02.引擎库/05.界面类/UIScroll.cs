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
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Core
{
    [Serializable]
    public abstract class UIPanel<T, TGrid> : UIPanel, IModule, ISystem, IMove where TGrid : Component, IGrid<T>
    {
        protected readonly Dictionary<int, TGrid> grids = new Dictionary<int, TGrid>();
        private IList<T> items;
        private int minIndex;
        private int maxIndex;
        private bool selector;
        private bool selected;
        private bool vertical;
        private string assetName;
        private string assetPath;

        private int row;
        private int column;
        protected float width;
        protected float height;

        [Inject] public RectTransform content;

        void IModule.Acquire(Entity owner)
        {
            this.owner = owner;
            var panel = Service.Ref<UIMaskAttribute>.GetAttribute(GetType());
            if (panel != null)
            {
                layer = panel.layer;
                group = panel.group;
            }

            var value = Service.Ref<UIRectAttribute>.GetAttribute(GetType());
            if (value != null)
            {
                row = value.row;
                width = value.width;
                column = value.column;
                height = value.height;
                vertical = value.vertical;
                selected = value.selected;
            }

            assetName = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
            assetPath = assetName;
            var asset = Service.Ref<UIPathAttribute>.GetAttribute(typeof(TGrid));
            if (asset != null)
            {
                assetPath = GlobalSetting.Prefab.Format(asset.asset);
            }

            owner.OnHide += Unload;
            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.one;
        }

        void ISystem.Update()
        {
            Update(false);
            Update();
        }

        private void Update(bool unload)
        {
            if (unload)
            {
                Unload();
            }

            if (content && items != null)
            {
                Reload();
            }
        }

        private void Unload()
        {
            minIndex = -1;
            maxIndex = -1;
            selector = selected;
            foreach (var i in grids.Keys)
            {
                if (grids.TryGetValue(i, out var grid) && grid)
                {
                    grid.Dispose();
                    PoolManager.Hide(grid);
                }
            }

            grids.Clear();
        }

        private void Reload()
        {
            int min, max, idx;
            if (vertical)
            {
                idx = (int)(content.anchoredPosition.y / height);
                min = idx * column;
                max = (idx + row) * column - 1;
            }
            else
            {
                idx = (int)(-content.anchoredPosition.x / width);
                min = idx * row;
                max = (idx + column) * row - 1;
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
                    if (grids.Remove(i, out var grid) && grid)
                    {
                        grid.Dispose();
                        PoolManager.Hide(grid);
                    }
                }

                for (var i = max + 1; i <= maxIndex; ++i)
                {
                    if (grids.Remove(i, out var grid) && grid)
                    {
                        grid.Dispose();
                        PoolManager.Hide(grid);
                    }
                }
            }

            minIndex = min;
            maxIndex = max;
            for (var i = min; i <= max; i++)
            {
                if (grids.ContainsKey(i))
                {
                    continue;
                }

                var position = Vector2.zero;
                if (vertical)
                {
                    idx = i / column;
                    position.x = i % column * width;
                    position.y = -idx * height;
                }
                else
                {
                    idx = i / row;
                    position.x = idx * width;
                    position.y = -(i % row) * height;
                }

                var grid = PoolManager.Show<TGrid>(assetPath, content, assetName);
                var rect = (RectTransform)grid.transform;
                rect.pivot = Vector2.up;
                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = position;
                if (selector && i == min)
                {
                    selector = false;
                    grid.Select();
                }

                grids[i] = grid;
                grid.SetItem(items[i]);
            }
        }

        public void SetItem(IList<T> items)
        {
            this.items = items;
            if (items != null)
            {
                float value = items.Count;
                if (vertical)
                {
                    value = Mathf.Ceil(value / column);
                    content.sizeDelta = new Vector2(0, value * height);
                }
                else
                {
                    value = Mathf.Ceil(value / row);
                    content.sizeDelta = new Vector2(value * width, 0);
                }
            }

            Update(true);
        }

        public void Move(IGrid grid, int move)
        {
            switch (move)
            {
                case 0 when !vertical:
                    for (int i = 0; i < row; i++)
                    {
                        if (grids.TryGetValue(minIndex + i + row, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.right * width;
                            return;
                        }
                    }

                    return;
                case 1 when vertical:
                    for (int i = 0; i < column; i++)
                    {
                        if (grids.TryGetValue(minIndex + i + column, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.down * height;
                            return;
                        }
                    }

                    return;
                case 2 when !vertical:
                    for (int i = 0; i < row; i++)
                    {
                        if (grids.TryGetValue(maxIndex - i - row, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.left * width;
                            return;
                        }
                    }

                    return;
                case 3 when vertical:
                    for (int i = 0; i < column; i++)
                    {
                        if (grids.TryGetValue(maxIndex - i - column, out var current) && current == (TGrid)grid)
                        {
                            content.anchoredPosition += Vector2.up * height;
                            return;
                        }
                    }

                    return;
            }
        }
    }

    public interface IMove
    {
        void Move(IGrid grid, int move);
    }

    public interface IGrid
    {
        void Select();

        void Dispose();
    }

    public interface IGrid<T> : IGrid
    {
        T item { get; }
        void SetItem(T item);
    }
}