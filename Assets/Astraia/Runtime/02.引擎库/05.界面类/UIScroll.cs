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
    public abstract class UIPanel<T, TGrid> : UIPanel, IAcquire, ISystem, IMove where TGrid : Component, IGrid<T>
    {
        private Dictionary<int, TGrid> grids = new Dictionary<int, TGrid>();
        private IList<T> items;
        private int minIndex;
        private int maxIndex;

        private bool rotation;
        private bool selected;
        private bool selector;

        private int row;
        private int col;
        private int roc;
        private int cor;

        private string assetName;
        private string assetPath;

        public float width;
        public float height;
        public Action<IGrid> OnMove;
        [Inject] public RectTransform content;

        void IAcquire.Acquire(object item)
        {
            owner = (Entity)item;
            owner.Logic.OnHide += Unload;

            if (GetType().GetAttribute(out UIMaskAttribute mask))
            {
                layer = mask.layer;
                group = mask.group;
            }

            if (GetType().GetAttribute(out UIRectAttribute rect))
            {
                col = rect.col;
                row = rect.row;
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

            col = rotation ? col : col + 1;
            row = rotation ? row + 1 : row;
            cor = rotation ? col : row;
            roc = rotation ? row : col;

            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;
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
                var pos = content.anchoredPosition;
                var min = Mathf.Max(Mathf.FloorToInt(rotation ? pos.y / height : -pos.x / width) * cor, 0);
                var max = Mathf.Min(min + roc * cor - 1, items.Count - 1);

                if (min == minIndex && max == maxIndex)
                {
                    return;
                }

                for (var i = minIndex; i < min; i++)
                {
                    if (grids.Remove(i, out var grid) && grid)
                    {
                        grid.Release();
                        OnMove?.Invoke(grid);
                        PoolManager.Hide(grid);
                    }
                }

                for (var i = maxIndex; i > max; i--)
                {
                    if (grids.Remove(i, out var grid) && grid)
                    {
                        grid.Release();
                        OnMove?.Invoke(grid);
                        PoolManager.Hide(grid);
                    }
                }

                for (var i = min; i <= max; i++)
                {
                    if (!grids.TryGetValue(i, out var grid))
                    {
                        grid = PoolManager.Show<TGrid>(assetPath, content, assetName);
                        grid.index = i;
                        grid.SetItem(items[i]);
                        if (selector)
                        {
                            selector = false;
                            grid.Select();
                        }

                        grids[i] = grid;
                        SetPosition((RectTransform)grid.transform, i);
                    }
                }

                minIndex = min;
                maxIndex = max;
            }
        }

        private void SetPosition(RectTransform rect, int i)
        {
            var c = rotation ? i % col : i / row;
            var r = rotation ? i / col : i % row;
            rect.pivot = Vector2.up;
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(c * width, -r * height);
        }

        public void SetItem(IList<T> item)
        {
            Unload();
            items = item ?? Array.Empty<T>();
            var c = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
            var r = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
            content.sizeDelta = new Vector2(r * width, c * height);
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
                    grid.Release();
                    PoolManager.Hide(grid);
                }
            }

            items = null;
            grids.Clear();
        }

        public void Move(int index, int move)
        {
            var pos = content.anchoredPosition;
            switch (move)
            {
                case 0 when !rotation && index / cor < minIndex / roc + 2: // 左
                    pos.x += width;
                    break;
                case 1 when rotation && index / cor < minIndex / cor + 2: // 上
                    pos.y -= height;
                    break;
                case 2 when !rotation && index / cor > maxIndex / roc - 2: // 右
                    pos.x -= width;
                    break;
                case 3 when rotation && index / cor > maxIndex / cor - 2: // 下
                    pos.y += height;
                    break;
            }

            var c = rotation ? col : col - 1;
            var r = rotation ? row - 1 : row;
            pos.x = Mathf.Clamp(pos.x, 0, content.rect.width - c * width);
            pos.y = Mathf.Clamp(pos.y, 0, content.rect.height - r * height);
            content.anchoredPosition = pos;
        }
    }

    public interface IMove
    {
        void Move(int index, int move);
    }

    public interface IGrid
    {
        int index { set; }
        void Select();
    }

    public interface IGrid<T> : IGrid
    {
        T item { get; }
        void SetItem(T item);
        void Release();
    }
}