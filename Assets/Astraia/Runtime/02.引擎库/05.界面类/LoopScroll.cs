using System;
using System.Collections.Generic;
using Astraia.Core;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public class LoopScroll<T, TGrid> where TGrid : Component, IGrid<T>
    {
        private readonly RingQueue<TGrid> grids;
        private Dictionary<TGrid, int> nodes;
        private IList<T> items;
        private int startIndex;
        private bool selector;
        private float position;
        private readonly int row;
        private readonly int column;
        private readonly float width;
        private readonly float height;
        private readonly bool rotation;
        private readonly bool selected;

        private readonly RectTransform content;
        public event Action<TGrid> OnModified;

        public LoopScroll(RectTransform content)
        {
            this.content = content;

            if (typeof(TGrid).GetAttribute(out UIRectAttribute rect))
            {
                row = rect.row;
                width = rect.width;
                column = rect.column;
                height = rect.height;
                rotation = rect.rotation;
                selected = rect.selected;
            }

            var assetName = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
            var assetPath = assetName;
            if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
            {
                assetPath = GlobalSetting.Prefab.Format(path.asset);
            }

            nodes = new Dictionary<TGrid, int>();
            grids = new RingQueue<TGrid>(column * row + (rotation ? column : row));
            for (var i = 0; i < grids.Count; i++)
            {
                var grid = PoolManager.Show<TGrid>(assetPath, content, assetName);
                var item = (RectTransform)grid.transform;
                item.pivot = Vector2.up;
                item.anchorMin = Vector2.up;
                item.anchorMax = Vector2.up;
                item.sizeDelta = new Vector2(width, height);
                grids.Enqueue(grid);
                nodes[grid] = i;
            }

            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;
        }

        public void SetItem(IList<T> item)
        {
            items = item;
            selector = true;

            var r = rotation ? Mathf.CeilToInt((float)items.Count / column) : row;
            var c = rotation ? column : Mathf.CeilToInt((float)items.Count / row);
            content.sizeDelta = new Vector2(c * width, r * height);

            for (int index = 0; index < grids.Index; index++)
            {
                var grid = grids[index];
                if (index < items.Count)
                {
                    if (selected && selector)
                    {
                        selector = false;
                        grid.Select();
                    }

                    grid.SetItem(items[index]);
                    grid.gameObject.SetActive(true);
                    SetPosition(grid, index);
                }
                else
                {
                    grid.gameObject.SetActive(false);
                }
            }
        }


        public void OnTick(Vector2 pos)
        {
            if (items != null && items.Count != 0)
            {
                var current = rotation ? content.anchoredPosition.y : -content.anchoredPosition.x;
                var step = rotation ? height : width;
                var delta = Mathf.FloorToInt(current - position);

                while (delta >= step)
                {
                    ScrollForward();
                    position += step;
                    delta -= (int)step;
                }

                while (delta <= -step)
                {
                    ScrollReverse();
                    position -= step;
                    delta += (int)step;
                }
            }
        }

        private void ScrollForward()
        {
            var step = rotation ? column : row;
            for (var i = 0; i < step; i++)
            {
                var newIndex = startIndex + grids.Index;
                if (newIndex >= items.Count)
                {
                    return;
                }

                startIndex++;
                var grid = grids.Dequeue();

                grid.Dispose();
                OnModified?.Invoke(grid);
                grids.Enqueue(grid);
                nodes[grid] = newIndex;
                grid.SetItem(items[newIndex]);
                SetPosition(grid, newIndex);
            }
        }

        private void ScrollReverse()
        {
            var step = rotation ? column : row;
            for (var i = 0; i < step; i++)
            {
                if (startIndex <= 0)
                {
                    return;
                }

                startIndex--;
                var grid = grids.DequeueLast();
                grid.Dispose();
                OnModified?.Invoke(grid);
                grids.EnqueueFront(grid);
                nodes[grid] = startIndex;
                grid.SetItem(items[startIndex]);
                SetPosition(grid, startIndex);
            }
        }

        private void SetPosition(TGrid grid, int index)
        {
            var r = rotation ? index / column : index % row;
            var c = rotation ? index % column : index / row;
            ((RectTransform)grid.transform).anchoredPosition = new Vector2(c * width, -r * height);
        }

        private void Unload()
        {
            foreach (var grid in grids)
            {
                grid.Dispose();
                PoolManager.Hide(grid);
            }

            items = null;
        }

        public void Move(TGrid grid, int move)
        {
            var visibleX = rotation ? column : row;
            var visibleY = rotation ? row : column;
            var minRow = startIndex / visibleX + 2;
            var maxRow = startIndex / visibleX + visibleY - 2;
            var curRow = nodes[grid] / visibleX;
            switch (move)
            {
                case 0 when !rotation && curRow < minRow: // 左
                    content.anchoredPosition += Vector2.right * width;
                    break;
                case 1 when rotation && curRow < minRow: // 上
                    content.anchoredPosition += Vector2.down * height;
                    break;
                case 2 when !rotation && curRow > maxRow: // 右
                    content.anchoredPosition += Vector2.left * width;
                    break;
                case 3 when rotation && curRow > maxRow: // 下
                    content.anchoredPosition += Vector2.up * height ;
                    break;
            }
        }
    }
}