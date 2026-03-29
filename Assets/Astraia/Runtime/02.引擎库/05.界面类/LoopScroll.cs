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
        private TGrid[] grids;

        private float position;
        private int startIndex;
        private bool rotation;
        private bool selected;
        private int count;
        private int max;
        private int min;
        private int row;
        private int col;
        public float width;
        public float height;

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

            count = col * row;

            grids = new TGrid[count];
            for (var i = 0; i < count; i++)
            {
                var grid = PoolManager.Show<TGrid>(result, content, prefab);
                var data = (RectTransform)grid.transform;
                data.pivot = Vector2.up;
                data.anchorMin = Vector2.up;
                data.anchorMax = Vector2.up;
                grid.index = i;
                grids[i] = grid;
            }


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
                var current = rotation ? content.anchoredPosition.y : -content.anchoredPosition.x;
                var step = rotation ? height : width;
                var delta = Mathf.Floor(current - position);

                while (delta >= step)
                {
                    ScrollForward();
                    position += step;
                    delta -= step;
                }

                while (delta <= -step)
                {
                    ScrollReverse();
                    position -= step;
                    delta += step;
                }
            }
        }


        private void ScrollForward()
        {
            var step = rotation ? col : row;
            for (var i = 0; i < step; i++)
            {
                var newIndex = startIndex + count;
                if (newIndex >= items.Count)
                {
                    return;
                }

                startIndex++;
                var grid = grids[min];
                min = (min + 1) % count;
                grids[max] = grid;
                max = (max + 1) % count;

             
                grid.Acquire(items[newIndex]);
                SetPosition(grid, newIndex);
            }
        }

        private void ScrollReverse()
        {
            var step = rotation ? col : row;
            for (var i = 0; i < step; i++)
            {
                if (startIndex <= 0)
                {
                    return;
                }

                startIndex--;
                max = (max + count - 1) % count;
                var grid = grids[max];
                min = (min + count - 1) % count;
                grids[min] = grid;
             
                grid.Acquire(items[startIndex]);
                SetPosition(grid, startIndex);
            }
        }

        public void SetItem(IList<T> item)
        {
            items = item ?? Array.Empty<T>();
            var sizeX = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
            var sizeY = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
            content.sizeDelta = new Vector2(sizeY * width, sizeX * height);
            for (var i = 0; i < count; i++)
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
                    grid.gameObject.SetActive(true);
                }
                else
                {
                    grid.gameObject.SetActive(false);
                }
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
            // var cor = rotation ? col : row;
            // var roc = rotation ? row : col;
            // var pos = content.anchoredPosition;
            // switch (move)
            // {
            //     case 0 when !rotation && index / cor < minIndex / roc + 2: // 左
            //         pos.x += width;
            //         break;
            //     case 1 when rotation && index / cor < minIndex / cor + 2: // 上
            //         pos.y -= height;
            //         break;
            //     case 2 when !rotation && index / cor > maxIndex / roc - 2: // 右
            //         pos.x -= width;
            //         break;
            //     case 3 when rotation && index / cor > maxIndex / cor - 2: // 下
            //         pos.y += height;
            //         break;
            // }
            //
            // cor = rotation ? col : col - 1;
            // roc = rotation ? row - 1 : row;
            // pos.x = Mathf.Clamp(pos.x, 0, content.rect.width - cor * width);
            // pos.y = Mathf.Clamp(pos.y, 0, content.rect.height - roc * height);
            // content.anchoredPosition = pos;
        }

        public override void Enqueue()
        {
            for (var i = count - 1; i >= 0; i--)
            {
                grids[i].Release();
                PoolManager.Hide(grids[i]);
            }

            items = null;
            grids = null;
        }
    }
}