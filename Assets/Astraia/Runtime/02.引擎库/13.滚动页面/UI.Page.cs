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
        private readonly List<int, IGrid<TItem>> grids = new List<int, IGrid<TItem>>();
        private IList<TItem> items;
        private int minIndex;
        private int maxIndex;
        private int numCache;
        private bool selected;
        private bool restarted;

        internal bool selection;
        internal bool direction;
        internal Type assetType;
        internal Rect assetRect;
        internal RectTransform content;
        private int numX => (int)assetRect.x + (direction ? 0 : 1);
        private int numY => (int)assetRect.y + (direction ? 1 : 0);

        public IEnumerable<IGrid<TItem>> Values => grids.Values;

        public static UIPage<TItem> Create(Entity owner)
        {
            return (UIPage<TItem>)owner.AddAgent(typeof(IPage), typeof(UIPage<TItem>), typeof(UIPage<TItem>));
        }

        void IAgent.Dequeue()
        {
            selection = false;
            restarted = false;
        }

        void IAgent.Enqueue()
        {
            Rebuild();
            items = null;
        }

        void IPage.Update()
        {
            if (!content)
            {
                return;
            }

            if (!restarted)
            {
                restarted = true;
                if (direction)
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

            if (numCache != items.Count)
            {
                Rebuild();
                numCache = items.Count;
            }

            int min;
            int max;
            int idx;
            float pos;
            if (direction)
            {
                pos = content.anchoredPosition.y;
                idx = (int)(pos / assetRect.height);
                min = idx * numX;
                max = (idx + numY) * numX - 1;
            }
            else
            {
                pos = -content.anchoredPosition.x;
                idx = (int)(pos / assetRect.width);
                min = idx * numY;
                max = (idx + numX) * numY - 1;
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

                for (var i = max + 1; i <= maxIndex; ++i)
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

            minIndex = min;
            maxIndex = max;
            Load(min, max);
        }

        private async void Load(int min, int max)
        {
            for (var i = min; i <= max; ++i)
            {
                if (grids.ContainsKey(i))
                {
                    continue;
                }

                float posX;
                float posY;
                if (direction)
                {
                    var idx = i / numX;
                    posX = i % numX * assetRect.width + assetRect.width / 2;
                    posY = -idx * assetRect.height - assetRect.height / 2;
                }
                else
                {
                    var idx = i / numY;
                    posX = idx * assetRect.width + assetRect.width / 2;
                    posY = -(i % numY) * assetRect.height - assetRect.height / 2;
                }

                grids[i] = null;
                var item = await PoolManager.Show("Prefabs/" + assetType.Name);
                var grid = (IGrid<TItem>)item.GetOrAddComponent(assetType);
                var rect = (RectTransform)grid.transform;
                rect.SetParent(content);
                rect.sizeDelta = new Vector2(assetRect.width, assetRect.height);
                rect.localScale = Vector3.one;
                rect.localPosition = new Vector3(posX, posY, 0);
                if (!grids.ContainsKey(i))
                {
                    grid.Dispose();
                    PoolManager.Hide(grid.gameObject);
                    return;
                }

                grids[i] = grid;
                if (selected && i == max)
                {
                    selected = false;
                    grids[min].Select();
                }

                grid.SetItem(items[i]);
            }
        }

        public void SetItem(IList<TItem> items)
        {
            this.items = items;
            if (items != null)
            {
                float value = items.Count;
                if (direction)
                {
                    value = Mathf.Ceil(value / numX);
                    content.sizeDelta = new Vector2(0, value * assetRect.height);
                }
                else
                {
                    value = Mathf.Ceil(value / numY);
                    content.sizeDelta = new Vector2(value * assetRect.width, 0);
                }

                numCache = items.Count;
            }

            Rebuild();
            selected = selection;
            content.anchoredPosition = Vector2.zero;
        }

        private void Rebuild()
        {
            minIndex = -1;
            maxIndex = -1;
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
                case MoveDirection.Left when !direction:
                    for (int i = 0; i < numY; i++)
                    {
                        if (grids.TryGetValue(minIndex + i + numY, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.right * assetRect.width;
                            return;
                        }
                    }

                    return;
                case MoveDirection.Up when direction:
                    for (int i = 0; i < numX; i++)
                    {
                        if (grids.TryGetValue(minIndex + i + numX, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.down * assetRect.height;
                            return;
                        }
                    }

                    return;
                case MoveDirection.Right when !direction:
                    for (int i = 0; i < numY; i++)
                    {
                        if (grids.TryGetValue(maxIndex - i - numY, out current) && current == grid)
                        {
                            content.anchoredPosition += Vector2.left * assetRect.width;
                            return;
                        }
                    }

                    return;
                case MoveDirection.Down when direction:
                    for (int i = 0; i < numX; i++)
                    {
                        if (grids.TryGetValue(maxIndex - i - numX, out current) && current == grid)
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