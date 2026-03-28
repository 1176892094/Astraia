using System;
using System.Collections.Generic;
using Astraia.Core;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class UIPanel<T, TGrid> : UIPanel, IModule, ISystem, IMove where TGrid : Component, IGrid<T>
    {
        private IList<T> items;
        private int minIndex;
        private int maxIndex;
        private bool rotation;
        private bool selected;

        private int row;
        private int col;
        public float width;
        public float height;

        public Queue grids;
        public Action<IGrid> OnMove;
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
                row = rect.row;
                col = rect.col;
                width = rect.width;
                height = rect.height;
                rotation = rect.rotation;
                selected = rect.selected;
            }

            var prefab = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
            var result = prefab;
            if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
            {
                result = GlobalSetting.Prefab.Format(path.asset);
            }

            grids = new Queue(col * row);
            for (var i = 0; i < grids.Count; i++)
            {
                var grid = PoolManager.Show<TGrid>(result, content, prefab);
                var data = (RectTransform)grid.transform;
                data.pivot = Vector2.up;
                data.anchorMin = Vector2.up;
                data.anchorMax = Vector2.up;
                grid.index = i;
                grids.AddHead(grid);
            }

            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.one;
        }


        void ISystem.Update()
        {
            if (items != null && items.Count != 0)
            {
                var pos = content.anchoredPosition;
                var cor = rotation ? col : row;
                var roc = rotation ? row : col;
                var val = rotation ? pos.y / height : -pos.x / width;
                var min = Mathf.Max((int)val * cor, 0);
                var max = Mathf.Min((int)val * cor + roc * cor - 1, items.Count - 1);
                for (var i = minIndex; i < min; i++)
                {
                    var index = i + roc * cor;
                    if (index < items.Count)
                    {
                        var grid = grids.GetLast();
                        grids.AddHead(grid);
                        OnMove?.Invoke(grid);
                        grid.Release();
                        grid.index = index;
                        grid.Acquire(items[index]);
                        SetPosition(grid, index);
                    }
                }

                for (var i = max + 1; i <= maxIndex; i++)
                {
                    var index = i - roc * cor;
                    if (index >= 0)
                    {
                        var grid = grids.GetHead();
                        grids.AddLast(grid);
                        OnMove?.Invoke(grid);
                        grid.Release();
                        grid.index = index;
                        grid.Acquire(items[index]);
                        SetPosition(grid, index);
                    }
                }

                minIndex = min;
                maxIndex = max;
            }

            Update();
        }

        public void SetItem(IList<T> item)
        {
            items = item ?? Array.Empty<T>();
            var sizeX = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
            var sizeY = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
            content.sizeDelta = new Vector2(sizeY * width, sizeX * height);

            for (var i = 0; i < grids.Count; i++)
            {
                var grid = grids[i];
                if (i < items.Count)
                {
                    if (selected && i == 0)
                    {
                        grid.Select();
                    }

                    grid.index = i;
                    grid.Acquire(items[i]);
                    SetPosition(grid, i);
                }

                grid.gameObject.SetActive(i < items.Count);
            }
        }

        private void SetPosition(TGrid grid, int index)
        {
            var x = rotation ? index % col : index / row;
            var y = rotation ? index / col : index % row;
            var rect = (RectTransform)grid.transform;
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x * width, -y * height);
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

            cor = rotation ? col : col - 1;
            roc = rotation ? row - 1 : row;
            pos.x = Mathf.Clamp(pos.x, 0, content.rect.width - cor * width);
            pos.y = Mathf.Clamp(pos.y, 0, content.rect.height - roc * height);
            content.anchoredPosition = pos;
        }

        public override void Enqueue()
        {
            for (var i = grids.Count - 1; i >= 0; i--)
            {
                grids[i].Release();
                PoolManager.Hide(grids[i]);
            }

            items.Clear();
            grids.Clear();
        }

        public class Queue
        {
            private readonly TGrid[] Data;
            private int Head;
            private int Last;
            public int Count => Data.Length;
            public Queue(int count) => Data = new TGrid[count];
            public TGrid this[int index] => Data[(Last + index) % Count];

            public void AddHead(TGrid item)
            {
                Data[Head] = item;
                Head = (Head + 1) % Count;
            }

            public TGrid GetLast()
            {
                var item = Data[Last];
                Last = (Last + 1) % Count;
                return item;
            }

            public void AddLast(TGrid item)
            {
                Last = (Last + Count - 1) % Count;
                Data[Last] = item;
            }

            public TGrid GetHead()
            {
                Head = (Head + Count - 1) % Count;
                var item = Data[Head];
                return item;
            }

            public void Clear()
            {
                Head = 0;
                Last = 0;
                Array.Clear(Data, 0, Count);
            }
        }
    }
}