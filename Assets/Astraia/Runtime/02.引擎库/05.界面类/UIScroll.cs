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
        private readonly List<T> items = new List<T>();
        private int minIndex;
        private int maxIndex;
        private bool selector;
        private bool selected;
        private bool rotation;
        private string assetName;
        private string assetPath;

        private int row;
        private int col;

        public float width;
        public float height;
        public Action OnMove;
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
                var pos = content.anchoredPosition;
                var cor = rotation ? col : row;
                var roc = rotation ? row : col;
                var how = rotation ? pos.y / height : -pos.x / width;
                var min = Mathf.Max((int)how * cor, 0);
                var max = Mathf.Min((int)how * cor + roc * cor - 1, items.Count - 1);

                for (var i = minIndex; i < min; i++)
                {
                    if (grids.Remove(i, out var grid) && grid)
                    {
                        grid.Dispose();
                        OnMove?.Invoke();
                        PoolManager.Hide(grid);
                    }
                }

                for (var i = max + 1; i <= maxIndex; i++)
                {
                    if (grids.Remove(i, out var grid) && grid)
                    {
                        grid.Dispose();
                        OnMove?.Invoke();
                        PoolManager.Hide(grid);
                    }
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

                        grid.index = i;
                        grid.SetItem(items[i]);
                        grids[i] = grid;
                    }
                }
            }
        }

        public void SetItem(ICollection<T> item)
        {
            Unload();
            if (item != null)
            {
                items.AddRange(item);
            }

            var sizeX = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
            var sizeY = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
            content.sizeDelta = new Vector2(sizeY * width, sizeX * height);
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

            items.Clear();
            grids.Clear();
        }

        public void Move(int index, int move)
        {
            var cor = rotation ? col : row;
            var roc = rotation ? row : col;
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

            pos.x = Mathf.Clamp(pos.x, 0, content.rect.width - (col - (rotation ? 0 : 1)) * width);
            pos.y = Mathf.Clamp(pos.y, 0, content.rect.height - (row - (rotation ? 1 : 0)) * height);
            content.anchoredPosition = pos;
        }
    }

    public interface IMove
    {
        void Move(int index, int move);
    }

    public interface IGrid
    {
        int index { get; set; }

        void Select();

        void Dispose();
    }

    public interface IGrid<T> : IGrid
    {
        T item { get; }
        void SetItem(T item);
    }
}