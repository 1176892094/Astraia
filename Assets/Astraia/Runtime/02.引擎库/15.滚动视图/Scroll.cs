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
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public sealed class Scroll<TItem, TGrid> : Agent<RectTransform>, IScroll<TItem> where TGrid : Component, IGrid<TItem>
    {
        private readonly Dictionary<int, TGrid> grids = new Dictionary<int, TGrid>();
        private int oldMinIndex;
        private int oldMaxIndex;
        private bool initialized;
        private bool useSelected;
        private List<TItem> items;

        public bool selection { get; set; }
        public Rect assetRect { get; set; }
        public string assetPath { get; set; }
        public UIState direction { get; set; }

        private int row => (int)assetRect.y + (direction == UIState.Vertical ? 1 : 0);
        private int column => (int)assetRect.x + (direction == UIState.Horizontal ? 1 : 0);

        void IAgent.OnHide()
        {
            OnHide();
        }

        protected override void OnShow()
        {
            selection = false;
            initialized = false;
        }

        protected override void OnHide()
        {
            items = null;
            oldMinIndex = -1;
            oldMaxIndex = -1;
            foreach (var i in grids.Keys)
            {
                if (grids.TryGetValue(i, out var grid))
                {
                    if (grid != null)
                    {
                        grid.Dispose();
                        PoolManager.Hide(grid.gameObject);
                    }
                }
            }

            grids.Clear();
        }

        protected override void OnUpdate()
        {
            if (owner == null)
            {
                return;
            }

            if (!initialized)
            {
                initialized = true;
                if (direction == UIState.Vertical)
                {
                    owner.anchorMin = Vector2.up;
                    owner.anchorMax = Vector2.one;
                }
                else
                {
                    owner.anchorMin = Vector2.zero;
                    owner.anchorMax = Vector2.up;
                }

                owner.pivot = Vector2.up;
            }

            if (items == null)
            {
                return;
            }

            int newIndex;
            int minIndex;
            int maxIndex;
            float position;
            if (direction == UIState.Vertical)
            {
                position = owner.anchoredPosition.y;
                newIndex = (int)(position / assetRect.height);
                minIndex = newIndex * column;
                maxIndex = (newIndex + row) * column - 1;
            }
            else
            {
                position = -owner.anchoredPosition.x;
                newIndex = (int)(position / assetRect.width);
                minIndex = newIndex * row;
                maxIndex = (newIndex + column) * row - 1;
            }

            if (minIndex < 0)
            {
                minIndex = 0;
            }

            if (maxIndex > items.Count - 1)
            {
                maxIndex = items.Count - 1;
            }

            if (minIndex != oldMinIndex || maxIndex != oldMaxIndex)
            {
                for (var i = oldMinIndex; i < minIndex; ++i)
                {
                    if (grids.TryGetValue(i, out var grid))
                    {
                        if (grid != null)
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid.gameObject);
                        }

                        grids.Remove(i);
                    }
                }

                for (var i = maxIndex + 1; i <= oldMaxIndex; ++i)
                {
                    if (grids.TryGetValue(i, out var grid))
                    {
                        if (grid != null)
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid.gameObject);
                        }

                        grids.Remove(i);
                    }
                }
            }

            oldMinIndex = minIndex;
            oldMaxIndex = maxIndex;
            for (var i = minIndex; i <= maxIndex; ++i)
            {
                if (!grids.ContainsKey(i))
                {
                    float posX;
                    float posY;
                    var index = i;
                    grids[index] = null;
                    if (direction == UIState.Vertical)
                    {
                        posX = index % column * assetRect.width + assetRect.width / 2;
                        posY = -(index / column) * assetRect.height - assetRect.height / 2;
                    }
                    else
                    {
                        posX = index / row * assetRect.width + assetRect.width / 2;
                        posY = -(index % row) * assetRect.height - assetRect.height / 2;
                    }

                    PoolManager.Show(assetPath, obj =>
                    {
                        var grid = obj.GetComponent<TGrid>();
                        if (grid == null)
                        {
                            grid = obj.AddComponent<TGrid>();
                        }

                        var target = (RectTransform)grid.transform;
                        target.SetParent(owner);
                        target.sizeDelta = new Vector2(assetRect.width, assetRect.height);
                        target.localScale = Vector3.one;
                        target.localPosition = new Vector3(posX, posY, 0);
                        if (!grids.ContainsKey(index))
                        {
                            grid.Dispose();
                            PoolManager.Hide(grid.gameObject);
                            return;
                        }

                        grids[index] = grid;
                        if (useSelected && index == maxIndex)
                        {
                            useSelected = false;
                            grids[minIndex].Select();
                        }

                        grid.SetItem(items[index]);
                    });
                }
            }
        }

        public void SetItem(List<TItem> items)
        {
            OnHide();
            if (owner == null)
            {
                return;
            }

            this.items = items;
            if (items != null)
            {
                float value = items.Count;
                if (direction == UIState.Vertical)
                {
                    value = Mathf.Ceil(value / column);
                    owner.sizeDelta = new Vector2(0, value * assetRect.height);
                }
                else
                {
                    value = Mathf.Ceil(value / row);
                    owner.sizeDelta = new Vector2(value * assetRect.width, 0);
                }
            }


            useSelected = selection;
            owner.anchoredPosition = Vector2.zero;
        }

        public void Move(Component component, int direction)
        {
            if (component.TryGetComponent(out TGrid grid))
            {
                TGrid current;
                switch (direction)
                {
                    case 0 when this.direction == UIState.Horizontal: // 左
                        for (int i = 0; i < row; i++)
                        {
                            if (grids.TryGetValue(oldMinIndex + i + row, out current) && current == grid)
                            {
                                owner.anchoredPosition -= Vector2.left * assetRect.width;
                                break;
                            }
                        }

                        return;
                    case 1 when this.direction == UIState.Vertical: // 上
                        for (int i = 0; i < column; i++)
                        {
                            if (grids.TryGetValue(oldMinIndex + i + column, out current) && current == grid)
                            {
                                owner.anchoredPosition -= Vector2.up * assetRect.height;
                                break;
                            }
                        }

                        return;
                    case 2 when this.direction == UIState.Horizontal: // 右
                        for (int i = 0; i < row; i++)
                        {
                            if (grids.TryGetValue(oldMaxIndex - i - row, out current) && current == grid)
                            {
                                owner.anchoredPosition += Vector2.left * assetRect.width;
                                break;
                            }
                        }

                        return;
                    case 3 when this.direction == UIState.Vertical: // 下
                        for (int i = 0; i < column; i++)
                        {
                            if (grids.TryGetValue(oldMaxIndex - i - column, out current) && current == grid)
                            {
                                owner.anchoredPosition += Vector2.up * assetRect.height;
                                break;
                            }
                        }

                        return;
                }
            }
        }
    }
}