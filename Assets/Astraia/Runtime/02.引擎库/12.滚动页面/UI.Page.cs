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
using UnityEngine.EventSystems;

namespace Astraia
{
    [Serializable]
    public sealed class UIPage<TItem> : Agent<Entity>, IPage
    {
        private readonly Dictionary<int, IGrid<TItem>> grids = new Dictionary<int, IGrid<TItem>>();
        private int oldMinIndex;
        private int oldMaxIndex;
        private int cachedCount;
        private bool initialized;
        private bool useSelected;
        private IList<TItem> items;

        internal bool selection;
        internal Type assetType;
        internal Rect assetRect;
        internal UIPage direction;
        internal RectTransform content;
        private int row => (int)assetRect.y + (direction == UIPage.InputY ? 1 : 0);
        private int column => (int)assetRect.x + (direction == UIPage.InputX ? 1 : 0);

        public override void Dequeue()
        {
            selection = false;
            initialized = false;
        }
        
        public override void Enqueue()
        {
            Reset();
            items = null;
        }

        void IPage.OnUpdate(float deltaTime)
        {
            if (content == null)
            {
                return;
            }

            if (!initialized)
            {
                initialized = true;
                if (direction == UIPage.InputY)
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

            if (cachedCount != items.Count)
            {
                Reset();
                cachedCount = items.Count;
            }

            int newIndex;
            int minIndex;
            int maxIndex;
            float position;
            if (direction == UIPage.InputY)
            {
                position = content.anchoredPosition.y;
                newIndex = (int)(position / assetRect.height);
                minIndex = newIndex * column;
                maxIndex = (newIndex + row) * column - 1;
            }
            else
            {
                position = -content.anchoredPosition.x;
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
                    if (direction == UIPage.InputY)
                    {
                        var delta = index / column;
                        posX = index % column * assetRect.width + assetRect.width / 2;
                        posY = -delta * assetRect.height - assetRect.height / 2;
                    }
                    else
                    {
                        var delta = index / row;
                        posX = delta * assetRect.width + assetRect.width / 2;
                        posY = -(index % row) * assetRect.height - assetRect.height / 2;
                    }

                    PoolManager.Show("Prefabs/" + assetType.Name, obj =>
                    {
                        var grid = (IGrid<TItem>)obj.GetOrAddComponent(assetType);
                        var target = (RectTransform)grid.transform;
                        target.SetParent(content);
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

        public void SetItem(IList<TItem> items)
        {
            this.items = items;
            if (items != null)
            {
                float value = items.Count;
                if (direction == UIPage.InputY)
                {
                    value = Mathf.Ceil(value / column);
                    content.sizeDelta = new Vector2(0, value * assetRect.height);
                }
                else
                {
                    value = Mathf.Ceil(value / row);
                    content.sizeDelta = new Vector2(value * assetRect.width, 0);
                }

                cachedCount = items.Count;
            }

            Reset();
            useSelected = selection;
            content.anchoredPosition = Vector2.zero;
        }

        private void Reset()
        {
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

        public void Move(IGrid<TItem> grid, MoveDirection move)
        {
            IGrid<TItem> current;
            switch (move)
            {
                case MoveDirection.Left when direction == UIPage.InputX: 
                    for (int i = 0; i < row; i++)
                    {
                        if (grids.TryGetValue(oldMinIndex + i + row, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.right * assetRect.width;
                            return;
                        }
                    }

                    return;
                case MoveDirection.Up when direction == UIPage.InputY:
                    for (int i = 0; i < column; i++)
                    {
                        if (grids.TryGetValue(oldMinIndex + i + column, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.down * assetRect.height;
                            return;
                        }
                    }

                    return;
                case MoveDirection.Right when direction == UIPage.InputX:
                    for (int i = 0; i < row; i++)
                    {
                        if (grids.TryGetValue(oldMaxIndex - i - row, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.left * assetRect.width;
                            return;
                        }
                    }

                    return;
                case MoveDirection.Down when direction == UIPage.InputY:
                    for (int i = 0; i < column; i++)
                    {
                        if (grids.TryGetValue(oldMaxIndex - i - column, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.up * assetRect.height;
                            return;
                        }
                    }

                    return;
            }
        }
    }
}