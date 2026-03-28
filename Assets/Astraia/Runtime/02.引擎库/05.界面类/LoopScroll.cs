using System;
using System.Collections.Generic;
using Astraia.Core;
using UnityEngine;

namespace Astraia
{
    // [Serializable]
    // public abstract class UIPanel<T, TGrid> : UIPanel, IModule, ISystem, IMove where TGrid : Component, IGrid<T>
    // {
    //     private readonly List<T> items = new List<T>();
    //     private int minIndex;
    //     private int maxIndex;
    //     private bool rotation;
    //     private bool selected;
    //
    //     private int row;
    //     private int col;
    //     public float width;
    //     public float height;
    //
    //     public Queue grids;
    //     public Action OnMove;
    //     [Inject] public RectTransform content;
    //
    //
    //     void IAcquire.Acquire(object item)
    //     {
    //         owner = (Entity)item;
    //         if (GetType().GetAttribute(out UIMaskAttribute mask))
    //         {
    //             layer = mask.layer;
    //             group = mask.group;
    //         }
    //
    //         if (GetType().GetAttribute(out UIRectAttribute rect))
    //         {
    //             row = rect.row + (rotation ? 0 : 1);
    //             col = rect.col + (rotation ? 1 : 0);
    //             width = rect.width;
    //             height = rect.height;
    //             rotation = rect.rotation;
    //             selected = rect.selected;
    //         }
    //
    //         var prefab = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
    //         var result = prefab;
    //         if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
    //         {
    //             result = GlobalSetting.Prefab.Format(path.asset);
    //         }
    //
    //         grids = new Queue(col * row);
    //         for (var i = 0; i < grids.Count; i++)
    //         {
    //             var grid = PoolManager.Show<TGrid>(result, content, prefab);
    //             var data = (RectTransform)grid.transform;
    //             data.pivot = Vector2.up;
    //             data.anchorMin = Vector2.up;
    //             data.anchorMax = Vector2.up;
    //             grid.index = i;
    //             grids.AddHead(grid);
    //         }
    //
    //         content.pivot = Vector2.up;
    //         content.anchorMin = Vector2.up;
    //         content.anchorMax = Vector2.one;
    //     }
    //
    //
    //     void ISystem.Update()
    //     {
    //         if (items != null && items.Count != 0)
    //         {
    //             var pos = content.anchoredPosition;
    //             var step = rotation ? new Vector2Int(col, row) : new Vector2Int(row, col);
    //             var size = rotation ? pos.y / height : -pos.x / width;
    //             var min = Mathf.Max((int)size * step.x, 0);
    //             var max = Mathf.Min((int)size * step.x + step.y * step.x - 1, items.Count - 1);
    //             for (var i = minIndex; i < min; i++)
    //             {
    //                 var index = i + step.y * step.x;
    //                 if (index < items.Count)
    //                 {
    //                     var grid = grids.GetLast();
    //                     grid.Dispose();
    //                     OnMove?.Invoke();
    //                     grids.AddHead(grid);
    //                     grid.index = index;
    //                     grid.SetItem(items[index]);
    //                     SetPosition(grid, index);
    //                 }
    //             }
    //
    //             for (var i = max + 1; i <= maxIndex; i++)
    //             {
    //                 var index = i - step.y * step.x;
    //                 if (index >= 0)
    //                 {
    //                     var grid = grids.GetHead();
    //                     grid.Dispose();
    //                     OnMove?.Invoke();
    //                     grids.AddLast(grid);
    //                     grid.index = index;
    //                     grid.SetItem(items[index]);
    //                     SetPosition(grid, index);
    //                 }
    //             }
    //
    //             minIndex = min;
    //             maxIndex = max;
    //             pos.x = (int)Mathf.Max(pos.x, 0);
    //             pos.y = (int)Mathf.Max(pos.y, 0);
    //             content.anchoredPosition = pos;
    //         }
    //
    //         Update();
    //     }
    //
    //     public void SetItem(ICollection<T> item)
    //     {
    //         items.Clear();
    //         if (item != null)
    //         {
    //             items.AddRange(item);
    //         }
    //
    //         var sizeX = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
    //         var sizeY = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
    //         content.sizeDelta = new Vector2(sizeY * width, sizeX * height);
    //
    //         for (var i = 0; i < grids.Count; i++)
    //         {
    //             var grid = grids[i];
    //             if (i < items.Count)
    //             {
    //                 if (selected && grid.index == 0)
    //                 {
    //                     grid.Select();
    //                 }
    //
    //                 grid.index = i;
    //                 grid.SetItem(items[i]);
    //                 SetPosition(grid, i);
    //             }
    //
    //             grid.gameObject.SetActive(i < items.Count);
    //         }
    //     }
    //
    //     private void SetPosition(TGrid grid, int index)
    //     {
    //         var x = rotation ? index % col : index / row;
    //         var y = rotation ? index / col : index % row;
    //         var rect = (RectTransform)grid.transform;
    //         rect.sizeDelta = new Vector2(width, height);
    //         rect.anchoredPosition = new Vector2(x * width, -y * height);
    //     }
    //
    //     public void Move(IGrid grid, int move)
    //     {
    //         var step = rotation ? new Vector2Int(col, row) : new Vector2Int(row, col);
    //         var index = grid.index / (rotation ? col : row);
    //         var pos = content.anchoredPosition;
    //         switch (move)
    //         {
    //             case 0 when !rotation && index < minIndex / step.y + 2: // 左
    //                 pos.x += width;
    //                 break;
    //             case 1 when rotation && index < minIndex / step.x + 2: // 上
    //                 pos.y -= height;
    //                 break;
    //             case 2 when !rotation && index > maxIndex / step.y - 2: // 右
    //                 pos.x -= width;
    //                 break;
    //             case 3 when rotation && index > maxIndex / step.x - 2: // 下
    //                 pos.y += height;
    //                 break;
    //         }
    //
    //         pos.x = (int)Mathf.Clamp(pos.x, 0, content.rect.width - (col - (rotation ? 0 : 1)) * width);
    //         pos.y = (int)Mathf.Clamp(pos.y, 0, content.rect.height - (row - (rotation ? 1 : 0)) * height);
    //         content.anchoredPosition = pos;
    //     }
    //
    //     public override void Enqueue()
    //     {
    //         for (int i = 0; i < grids.Count; i++)
    //         {
    //             grids[i].Dispose();
    //             PoolManager.Hide(grids[i]);
    //         }
    //
    //         OnMove = null;
    //         items.Clear();
    //         grids.Clear();
    //     }
    //
    //     public class Queue
    //     {
    //         private readonly TGrid[] Data;
    //         private int Head;
    //         private int Last;
    //         public int Count => Data.Length;
    //         public Queue(int count) => Data = new TGrid[count];
    //         public TGrid this[int index] => Data[(Last + index) % Count];
    //
    //         public void AddHead(TGrid item)
    //         {
    //             Data[Head] = item;
    //             Head = (Head + 1) % Count;
    //         }
    //
    //         public TGrid GetLast()
    //         {
    //             var item = Data[Last];
    //             Last = (Last + 1) % Count;
    //             return item;
    //         }
    //
    //         public void AddLast(TGrid item)
    //         {
    //             Last = (Last + Count - 1) % Count;
    //             Data[Last] = item;
    //         }
    //
    //         public TGrid GetHead()
    //         {
    //             Head = (Head + Count - 1) % Count;
    //             var item = Data[Head];
    //             return item;
    //         }
    //
    //         public void Clear()
    //         {
    //             Head = 0;
    //             Last = 0;
    //             Array.Clear(Data, 0, Count);
    //         }
    //     }
    // }
}