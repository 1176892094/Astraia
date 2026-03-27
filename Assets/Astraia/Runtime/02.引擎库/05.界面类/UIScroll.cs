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
        protected readonly Dictionary<TGrid, int> nodes = new Dictionary<TGrid, int>();
        private IList<T> items;
        private int minIndex;
        private int maxIndex;
        private bool selector;
        private bool selected;
        private bool rotation;
        private string assetName;
        private string assetPath;

        private int row;
        private int col;
        protected float width;
        protected float height;
        protected Action OnMove;

        [Inject] public RectTransform content;

        void IAcquire.Acquire(object item)
        {
            owner = (Entity)item;
            if (GetType().GetAttribute(out UIMaskAttribute mask))
            {
                layer = mask.layer;
                group = mask.group;
            }

            if (GetType().GetAttribute(out UIRectAttribute rect))
            {
                row = rect.row + (rotation ? 0 : 1);
                col = rect.col + (rotation ? 1 : 0);
                width = rect.width;
                height = rect.height;
                rotation = rect.rotation;
                selected = rect.selected;
            }

            assetName = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
            assetPath = assetName;
            if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
            {
                assetPath = GlobalSetting.Prefab.Format(path.asset);
            }

            owner.Logic.OnHide += Unload;
            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.one;
        }

        void ISystem.Update()
        {
            Reload();
            Update();
        }


        private void Reload()
        {
            if (items != null && items.Count != 0)
            {
                var v1 = rotation ? col : row;
                var v2 = rotation ? row : col;
                var v3 = rotation ? height : width;
                var v4 = rotation ? content.anchoredPosition.y : -content.anchoredPosition.x;

                var min = (int)Mathf.Max(v4 / v3 * v1, 0);
                var max = (int)Mathf.Min((v4 / v3 + v2) * v1 - 1, items.Count - 1);

                if (min != minIndex || max != maxIndex)
                {
                    for (var i = minIndex; i < min; i++)
                    {
                        if (grids.Remove(i, out var grid) && grid)
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid);
                        }
                    }

                    for (var i = max + 1; i <= maxIndex; i++)
                    {
                        if (grids.Remove(i, out var grid) && grid)
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid);
                        }
                    }

                    OnMove?.Invoke();
                }

                minIndex = min;
                maxIndex = max;
                for (var i = min; i <= max; i++)
                {
                    if (!grids.ContainsKey(i))
                    {
                        var grid = PoolManager.Show<TGrid>(assetPath, content, assetName);
                        var rect = (RectTransform)grid.transform;
                        rect.pivot = Vector2.up;
                        rect.anchorMin = Vector2.up;
                        rect.anchorMax = Vector2.up;
                        rect.sizeDelta = new Vector2(width, height);
                        var posX = rotation ? i % col : i / row;
                        var posY = rotation ? i / col : i % row;
                        rect.anchoredPosition = new Vector2(posX * width, -posY * height);
                        if (selector && i == min)
                        {
                            selector = false;
                            grid.Select();
                        }

                        grids[i] = grid;
                        nodes[grid] = i;
                        grid.SetItem(items[i]);
                    }
                }
            }
        }

        public void SetItem(IList<T> items)
        {
            if (items != null)
            {
                var sizeX = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
                var sizeY = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
                content.sizeDelta = new Vector2(sizeY * width, sizeX * height);
            }

            Unload();
            this.items = items;
            Reload();
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

            items = null;
            grids.Clear();
        }

        public void Move(IGrid grid, int move)
        {
            var index = nodes[(TGrid)grid] / (rotation ? col : row);
            var newPos = content.anchoredPosition;
            switch (move)
            {
                case 0 when !rotation && index < minIndex + 2: // 左
                    newPos.x += width;
                    break;
                case 1 when rotation && index < minIndex + 2: // 上
                    newPos.y -= height;
                    break;
                case 2 when !rotation && index > maxIndex - 2: // 右
                    newPos.x -= width;
                    break;
                case 3 when rotation && index > maxIndex - 2: // 下
                    newPos.y += height;
                    break;
            }

            newPos.x = Mathf.Clamp(newPos.x, 0, content.rect.width - (col - (rotation ? 0 : 1)) * width);
            newPos.y = Mathf.Clamp(newPos.y, 0, content.rect.height - (row - (rotation ? 1 : 0)) * height);
            content.anchoredPosition = newPos;
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