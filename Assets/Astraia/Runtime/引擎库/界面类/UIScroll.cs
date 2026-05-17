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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Astraia.Core
{
    [Serializable]
    public abstract class UIPanel<T, TGrid> : UIPanel, IAcquire, IMove where TGrid : Component, IGrid<T>
    {
        private TGrid[] grids;
        private IList<T> items;

        private int row;
        private int col;
        private int roc;
        private int cor;
        private int minIndex;
        private int maxIndex;
        private bool rotation;
        private bool selected;

        private string assetName;
        private string assetPath;

        public float width;
        public float height;
        public Action<IGrid> OnMove;
        [Inject] public ScrollRect scrollView;

        private RectTransform content => scrollView.content;

        void IAcquire.Acquire(object item)
        {
            owner = (Entity)item;
            owner.Logic.OnHide += Unload;
            owner.Logic.OnShow += AddListener;
            owner.Logic.OnHide += RemoveListener;

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

            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;

            col = rotation ? col : col + 1;
            row = rotation ? row + 1 : row;
            cor = rotation ? col : row;
            roc = rotation ? row : col;
            grids = new TGrid[col * row];
        }

        private void AddListener()
        {
            scrollView.onValueChanged.AddListener(OnScroll);
        }

        private void RemoveListener()
        {
            scrollView.onValueChanged.RemoveListener(OnScroll);
        }

        private void OnScroll(Vector2 position)
        {
            if (items != null && items.Count != 0)
            {
                var pos = content.anchoredPosition;
                var min = Mathf.Max(Mathf.FloorToInt(rotation ? pos.y / height : -pos.x / width) * cor, 0);
                var max = Mathf.Min(min + roc * cor - 1, items.Count - 1);

                if (min != minIndex || max != maxIndex)
                {
                    for (var i = minIndex; i < min; i++)
                    {
                        Unload(i % grids.Length);
                    }

                    for (var i = maxIndex; i > max; i--)
                    {
                        Unload(i % grids.Length);
                    }

                    Reload(min, max);
                }
            }
        }

        public void SetItem(IList<T> item)
        {
            Unload();
            items = item ?? Array.Empty<T>();
            var c = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
            var r = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
            content.sizeDelta = new Vector2(r * width, c * height);

            var pos = content.anchoredPosition;
            var min = Mathf.Max(Mathf.FloorToInt(rotation ? pos.y / height : -pos.x / width) * cor, 0);
            var max = Mathf.Min(min + roc * cor - 1, items.Count - 1);
            Reload(min, max, selected);
        }

        private void Unload()
        {
            for (var i = grids.Length - 1; i >= 0; i--)
            {
                Unload(i);
            }

            items = null;
        }

        private void Unload(int i)
        {
            var grid = grids[i];
            if (grid)
            {
                grids[i] = null;
                grid.Release();
                PoolManager.Hide(grid);
                OnMove?.Invoke(grid);
            }
        }

        private void Reload(int min, int max, bool selected = false)
        {
            for (var i = min; i <= max; i++)
            {
                Reload(i, selected);
            }

            minIndex = min;
            maxIndex = max;
        }

        private void Reload(int i, bool selected)
        {
            var index = i % grids.Length;
            var grid = grids[index];
            if (grid)
            {
                return;
            }

            grid = PoolManager.Show<TGrid>(assetPath, content, assetName);
            grids[index] = grid;

            if (grid.TryGetComponent(out RectTransform rect))
            {
                var posX = rotation ? i % col : i / row;
                var posY = rotation ? i / col : i % row;
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

            grid.index = i;
            grid.SetItem(items[i]);
        }

        public void Move(int index, MoveDirection move)
        {
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
    }

    public interface IMove
    {
        void Move(int index, MoveDirection move);
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