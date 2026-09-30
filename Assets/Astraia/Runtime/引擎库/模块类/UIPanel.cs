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
        private TGrid[] grids;
        private IList<T> items;
        private int row;
        private int col;
        private int roc;
        private int cor;

        private int current;
        private int minIndex;
        private int maxIndex;
        private bool reversed;
        private bool selected;
        private string assetName;
        private string assetPath;

        public float width;
        public float height;
        public ScrollRect owner;

        private bool rotation => owner.vertical;

        protected override void Awake()
        {
            owner = ExportManager.Export<ScrollRect>(this, "ScrollRect");

            if (GetType().GetAttribute(out UIRectAttribute rect))
            {
                col = rect.col;
                row = rect.row;
                width = rect.width;
                height = rect.height;
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

                owner.vertical = (rect.opcode & ROTATION) == 0;
                owner.horizontal = (rect.opcode & ROTATION) != 0;
            }

            assetName = GlobalSetting.PREFAB.Format(typeof(TGrid).Name);
            assetPath = assetName;
            if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
            {
                assetPath = GlobalSetting.PREFAB.Format(path.asset);
            }

            col = rotation ? col : col + 1;
            row = rotation ? row + 1 : row;
            cor = rotation ? col : row;
            roc = rotation ? row : col;
            grids = new TGrid[col * row];
        }

        protected override void OnEnable()
        {
            owner.onValueChanged.AddListener(ScrollView);
        }

        protected override void OnDisable()
        {
            Unload();
            owner.onValueChanged.RemoveListener(ScrollView);
        }

        protected override void OnDestroy()
        {
            items = null;
            grids = null;
        }

        private void LateUpdate()
        {
            if (items != null && current != items.Count)
            {
                for (var i = 0; i < current; i++)
                {
                    if (i < items.Count)
                    {
                        Reload(i);
                    }
                    else
                    {
                        Unload(i);
                    }
                }

                current = items.Count;
            }
        }

        private void ScrollView(Vector2 position)
        {
            if (items != null && current != 0)
            {
                var index = GetIndex();
                if (index.x == minIndex && index.y == maxIndex)
                {
                    return;
                }

                for (var i = minIndex; i < index.x; i++)
                {
                    Unload(i);
                }

                for (var i = maxIndex; i > index.y; i--)
                {
                    Unload(i);
                }

                Reload(index);
            }
        }

        public void SetItem(params T[] item)
        {
            Unload();
            Resize(item);
            Reload(GetIndex(), selected);
        }

        public void SetItem(IList<T> item)
        {
            Unload();
            Resize(item);
            Reload(GetIndex(), selected);
        }

        private void Resize(IList<T> item)
        {
            items = item ?? Array.Empty<T>();
            current = items.Count;
            var x = rotation ? col : Mathf.CeilToInt((float)current / row);
            var y = rotation ? Mathf.CeilToInt((float)current / col) : row;
            owner.content.sizeDelta = new Vector2(x * width, y * height);
        }

        private Vector2Int GetIndex()
        {
            var pos = owner.content.anchoredPosition;
            var idx = rotation ? pos.y / height : -pos.x / width;
            var min = Mathf.Max(Mathf.FloorToInt(idx) * cor, 0);
            var max = Mathf.Min(min + roc * cor - 1, current - 1);
            return new Vector2Int(min, max);
        }

        private void Reload(Vector2Int index, bool selected = false)
        {
            for (var i = index.x; i <= index.y; i++)
            {
                Reload(i, selected);
            }

            minIndex = index.x;
            maxIndex = index.y;
        }

        private void Reload(int i)
        {
            var index = i % grids.Length;
            var grid = grids[index];
            if (grid)
            {
                grid.SetItem(i, items[i]);
            }
        }

        private void Reload(int i, bool selected)
        {
            var index = i % grids.Length;
            var grid = grids[index];
            if (grid)
            {
                return;
            }

            grid = PoolManager.Show<TGrid>(assetPath, assetName, owner.content);
            grids[index] = grid;

            if (grid.TryGetComponent(out RectTransform rect))
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

            if (selected && index == 0)
            {
                grid.Select();
            }

            if (grid.TryGetComponent(out IItem<T> result))
            {
                result.SetItem(items);
            }

            grid.SetItem(i, items[i]);
        }

        private void Unload()
        {
            for (var i = 0; i < grids.Length; i++)
            {
                Unload(i);
            }

            items = null;
        }

        private void Unload(int i)
        {
            var index = i % grids.Length;
            var grid = grids[index];
            if (grid)
            {
                grids[index] = null;
                grid.Release();
                Move(grid);
                PoolManager.Hide(grid);
            }
        }

        public void Move(int index, MoveDirection move)
        {
            var content = owner.content;
            var pos = content.anchoredPosition;
            switch (move)
            {
                case MoveDirection.Left when !rotation && index / roc == minIndex / roc + 1:
                    pos.x += width;
                    break;
                case MoveDirection.Up when rotation && index / cor == minIndex / cor + 1:
                    pos.y -= height;
                    break;
                case MoveDirection.Right when !rotation && index / roc == maxIndex / roc - 1:
                    pos.x -= width;
                    break;
                case MoveDirection.Down when rotation && index / cor == maxIndex / cor - 1:
                    pos.y += height;
                    break;
            }

            var c = rotation ? col : col - 1;
            var r = rotation ? row - 1 : row;
            pos.x = Mathf.Clamp(pos.x, 0, content.rect.width - Mathf.Min(c, content.rect.width / width) * width);
            pos.y = Mathf.Clamp(pos.y, 0, content.rect.height - Mathf.Min(r, content.rect.height / height) * height);
            content.anchoredPosition = pos;
        }

        protected virtual void Move(IGrid grid) { }
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