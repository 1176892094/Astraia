// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 15:09:41
// # Recently: 2026-09-06 15:32:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Astraia
{
    public abstract class UIPanel : Export
    {
        protected const int ROTATION = 1 << 0;
        protected const int SELECTED = 1 << 1;
        protected const int REVERSED = 1 << 2;

        public int state;

        internal void ShowInternal() => OnShow();

        internal void HideInternal() => OnHide();

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }

        public static implicit operator bool(UIPanel panel)
        {
            return panel != null && panel.isActiveAndEnabled;
        }
    }

    public abstract class UIPanel<T, TGrid> : UIPanel, IMove where TGrid : Component, IGrid<T>
    {
        protected Dictionary<int, TGrid> grids = new Dictionary<int, TGrid>();
        protected float width;
        protected float height;
        protected ScrollRect owner;

        private IList<T> items;
        private int row;
        private int col;
        private int colUse;
        private int rowUse;

        private int current;
        private int minIndex;
        private int maxIndex;
        private bool rotation;
        private bool reversed;
        private bool selected;
        private string assetName;
        private string assetPath;

        protected override void Awake()
        {
            owner = ExportManager.Export<ScrollRect>(this, "ScrollRect");

            if (GetType().GetAttribute(out UIRectAttribute rect))
            {
                col = rect.col;
                row = rect.row;
                width = rect.width;
                height = rect.height;
                rotation = (rect.opcode & ROTATION) == 0;
                selected = (rect.opcode & SELECTED) != 0;
                reversed = (rect.opcode & REVERSED) != 0;

                if (owner.viewport)
                {
                    owner.viewport.anchorMin = Vector2.zero;
                    owner.viewport.anchorMax = Vector2.one;
                    owner.viewport.offsetMin = new Vector2(rect.offset, rect.offset);
                    owner.viewport.offsetMax = -new Vector2(rect.offset, rect.offset);
                }

                if (owner.content)
                {
                    owner.content.pivot = Vector2.up;
                    owner.content.anchorMin = Vector2.up;
                    owner.content.anchorMax = Vector2.up;
                }

                owner.vertical = rotation;
                owner.horizontal = !rotation;
            }

            assetName = GlobalSetting.PREFAB.Format(typeof(TGrid).Name);
            assetPath = assetName;
            if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
            {
                assetPath = GlobalSetting.PREFAB.Format(path.asset);
            }

            col = rotation ? col : col + 1;
            row = rotation ? row + 1 : row;
            colUse = rotation ? col : row;
            rowUse = rotation ? row : col;
        }

        protected override void OnEnable()
        {
            minIndex = -1;
            maxIndex = -1;
        }

        protected override void OnDisable()
        {
            Unload();
        }

        protected override void OnDestroy()
        {
            grids.Clear();
            grids = null;
            items = null;
        }

        protected virtual void Update()
        {
            if (items != null)
            {
                Modified(false);
                Verified();
            }
        }

        public void SetItem(params T[] item)
        {
            items = item ?? Array.Empty<T>();
            Modified(true);
            Verified();
            Reloaded();
        }

        public void SetItem(IList<T> item)
        {
            items = item ?? Array.Empty<T>();
            Modified(true);
            Verified();
            Reloaded();
        }

        protected virtual void Reloaded() { }

        private void Modified(bool force)
        {
            var count = items.Count;
            if (current != count || force)
            {
                Resize(count);

                if (current > count)
                {
                    var index = Mathf.Max(minIndex, count);
                    for (var i = index; i <= maxIndex; i++)
                    {
                        Remove(i);
                    }
                }

                foreach (var kv in grids)
                {
                    ReloadInternal(kv.Key, kv.Value);
                }

                current = count;
            }
        }

        private void Verified()
        {
            if (current != 0)
            {
                var index = GetIndex();
                if (index.x != minIndex || index.y != maxIndex)
                {
                    for (var i = minIndex; i < index.x; i++)
                    {
                        Remove(i);
                    }

                    for (var i = maxIndex; i > index.y; i--)
                    {
                        Remove(i);
                    }

                    minIndex = index.x;
                    maxIndex = index.y;

                    for (var i = minIndex; i <= maxIndex; i++)
                    {
                        if (!grids.ContainsKey(i))
                        {
                            Reload(i);
                        }
                    }
                }
            }
        }

        private void Resize(float count)
        {
            var x = rotation ? col : Mathf.CeilToInt(count / row);
            var y = rotation ? Mathf.CeilToInt(count / col) : row;
            owner.content.sizeDelta = new Vector2(x * width, y * height);
        }

        private Vector2Int GetIndex()
        {
            var pos = owner.content.anchoredPosition;
            var min = Mathf.Max(Mathf.FloorToInt(rotation ? pos.y / height : -pos.x / width) * colUse, 0);
            var max = Mathf.Min(min + rowUse * colUse, current) - 1;
            return new Vector2Int(min, max);
        }

        private void Reload(int i)
        {
            var grid = PoolManager.Show<TGrid>(assetPath, assetName, owner.content);
            if (grid.transform is RectTransform rect)
            {
                var posX = rotation ? i % col : i / row;
                var posY = rotation ? i / col : i % row;

                if (reversed)
                {
                    if (rotation)
                    {
                        posX = col - 1 - posX;
                    }
                    else
                    {
                        posY = row - 1 - posY;
                    }
                }

                rect.pivot = Vector2.up;
                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = new Vector2(posX * width, -posY * height);
            }

            grids.Add(i, grid);
            ReloadInternal(i, grid);
        }

        private void ReloadInternal(int i, TGrid grid)
        {
            if (grid is IItem<T> item)
            {
                item.SetItem(items);
            }

            if (i < items.Count)
            {
                grid.SetItem(i, items[i]);
            }

            if (selected && i == 0)
            {
                grid.Select();
            }
        }

        private void Unload(int i)
        {
            if (grids.TryGetValue(i, out var grid) && grid)
            {
                grid.Release();
                PoolManager.Hide(grid);
            }
        }

        private void Unload()
        {
            foreach (var i in grids.Keys)
            {
                Unload(i);
            }

            grids.Clear();
            items = null;
        }

        private void Remove(int i)
        {
            Unload(i);
            grids.Remove(i);
        }

        public void Move(int index, MoveDirection move)
        {
            var content = owner.content;
            var pos = content.anchoredPosition;
            switch (move)
            {
                case MoveDirection.Left when !rotation && index / rowUse == minIndex / rowUse + 1:
                    pos.x += width;
                    break;
                case MoveDirection.Up when rotation && index / colUse == minIndex / colUse + 1:
                    pos.y -= height;
                    break;
                case MoveDirection.Right when !rotation && index / rowUse == maxIndex / rowUse - 1:
                    pos.x -= width;
                    break;
                case MoveDirection.Down when rotation && index / colUse == maxIndex / colUse - 1:
                    pos.y += height;
                    break;
            }

            var c = rotation ? col : col - 1;
            var r = rotation ? row - 1 : row;
            pos.x = Mathf.Clamp(pos.x, 0, content.rect.width - Mathf.Min(c, content.rect.width / width) * width);
            pos.y = Mathf.Clamp(pos.y, 0, content.rect.height - Mathf.Min(r, content.rect.height / height) * height);
            content.anchoredPosition = pos;
        }
    }

    public interface IMove
    {
        void Move(int index, MoveDirection move);
    }

    public interface IGrid
    {
        void Select();
    }

    public interface IGrid<in T> : IGrid
    {
        void SetItem(int index, T item);
        void Release();
    }

    public interface IItem<T> : IGrid
    {
        void SetItem(IList<T> items);
    }
}