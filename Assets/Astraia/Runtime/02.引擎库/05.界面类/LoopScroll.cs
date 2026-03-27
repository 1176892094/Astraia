// using System;
// using System.Collections.Generic;
// using Astraia.Core;
// using UnityEngine;
//
// namespace Astraia
// {
//     [Serializable]
//     public abstract class UIPanel<T, TGrid> : UIPanel, IModule, ISystem, IMove where TGrid : Component, IGrid<T>
//     {
//         private readonly Dictionary<TGrid, int> nodes = new Dictionary<TGrid, int>();
//         private IList<T> items;
//         private int minIndex;
//         private bool rotation;
//         private bool selected;
//         private bool selector;
//         private float anchored;
//
//         private int row;
//         private int col;
//         private float width;
//         private float height;
//
//         public Action OnMove;
//         public RingQueue<TGrid> grids;
//         [Inject] public RectTransform content;
//
//
//         void IAcquire.Acquire(object item)
//         {
//             owner = (Entity)item;
//
//             if (GetType().GetAttribute(out UIMaskAttribute mask))
//             {
//                 layer = mask.layer;
//                 group = mask.group;
//             }
//
//             if (GetType().GetAttribute(out UIRectAttribute rect))
//             {
//                 row = rect.row;
//                 col = rect.col;
//                 width = rect.width;
//                 height = rect.height;
//                 rotation = rect.rotation;
//                 selected = rect.selected;
//             }
//
//             var prefab = GlobalSetting.Prefab.Format(typeof(TGrid).Name);
//             var result = prefab;
//             if (typeof(TGrid).GetAttribute(out UIPathAttribute path))
//             {
//                 result = GlobalSetting.Prefab.Format(path.asset);
//             }
//
//             grids = new RingQueue<TGrid>(col * row + (rotation ? col : row));
//             for (var i = 0; i < grids.Count; i++)
//             {
//                 var grid = PoolManager.Show<TGrid>(result, content, prefab);
//                 var data = (RectTransform)grid.transform;
//                 data.pivot = Vector2.up;
//                 data.anchorMin = Vector2.up;
//                 data.anchorMax = Vector2.up;
//                 data.sizeDelta = new Vector2(width, height);
//                 grids.Enqueue(grid);
//                 nodes.Add(grid, i);
//             }
//
//             content.pivot = Vector2.up;
//             content.anchorMin = Vector2.up;
//             content.anchorMax = Vector2.up;
//         }
//
//         void ISystem.Update()
//         {
//             if (items != null && items.Count != 0)
//             {
//                 var current = rotation ? content.anchoredPosition.y : -content.anchoredPosition.x;
//                 var stepped = rotation ? height : width;
//                 var delta = current - anchored;
//
//                 while (delta >= stepped)
//                 {
//                     ScrollForward();
//                     anchored += stepped;
//                     delta -= (int)stepped;
//                 }
//
//                 while (delta <= -stepped)
//                 {
//                     ScrollReverse();
//                     anchored -= stepped;
//                     delta += (int)stepped;
//                 }
//             }
//
//             Update();
//         }
//
//         private void ScrollForward()
//         {
//             var step = rotation ? col : row;
//             for (var i = 0; i < step; i++)
//             {
//                 var newIndex = minIndex + grids.Count;
//                 if (newIndex >= items.Count)
//                 {
//                     return;
//                 }
//
//              
//                 minIndex++;
//                 if (grids.Dequeue(out var grid))
//                 {
//                     grid.Dispose();
//                     OnMove?.Invoke();
//
//                     grids.Enqueue(grid);
//                     nodes[grid] = newIndex;
//                     grid.SetItem(items[newIndex]);
//                     SetPosition(grid, newIndex);
//                 }
//             }
//         }
//
//         private void ScrollReverse()
//         {
//             var step = rotation ? col : row;
//             for (var i = 0; i < step; i++)
//             {
//                 if (minIndex <= 0)
//                 {
//                     return;
//                 }
//
//                 minIndex--;
//            
//                 if (grids.Dequeue(out var grid))
//                 {
//                     grid.Dispose();
//                     OnMove?.Invoke();
//
//                     grids.Enqueue(grid, false);
//                     nodes[grid] = minIndex;
//                     grid.SetItem(items[minIndex]);
//                     SetPosition(grid, minIndex);
//                 }
//             
//             }
//         }
//
//         private void SetPosition(TGrid grid, int index)
//         {
//             var x = rotation ? index % col : index / row;
//             var y = rotation ? index / col : index % row;
//             ((RectTransform)grid.transform).anchoredPosition = new Vector2(x * width, -y * height);
//         }
//
//         public void SetItem(IList<T> item)
//         {
//             items = item;
//             minIndex = 0;
//             selector = true;
//
//             var r = rotation ? Mathf.CeilToInt((float)items.Count / col) : row;
//             var c = rotation ? col : Mathf.CeilToInt((float)items.Count / row);
//             content.sizeDelta = new Vector2(c * width, r * height);
//
//             for (var i = 0; i < grids.Index; i++)
//             {
//                 var grid = grids[i];
//                 if (i < items.Count)
//                 {
//                     if (selected && selector)
//                     {
//                         selector = false;
//                         grid.Select();
//                     }
//
//                     grid.SetItem(items[i]);
//                     grid.gameObject.SetActive(true);
//                     SetPosition(grid, i);
//                 }
//                 else
//                 {
//                     grid.gameObject.SetActive(false);
//                 }
//             }
//         }
//
//         public void Move(IGrid grid, int move)
//         {
//             var visibleX = rotation ? col : row;
//             var visibleY = rotation ? row : col;
//
//             var indexRow = nodes[(TGrid)grid] / visibleX;
//             var startRow = minIndex / visibleX;
//
//             var minRow = startRow + 2;
//             var maxRow = startRow + visibleY - 2;
//             var newPos = content.anchoredPosition;
//             switch (move)
//             {
//                 case 0 when !rotation && indexRow < minRow: // 左
//                     newPos.x += width;
//                     break;
//                 case 1 when rotation && indexRow < minRow: // 上
//                     newPos.y -= height;
//                     break;
//                 case 2 when !rotation && indexRow > maxRow: // 右
//                     newPos.x -= width;
//                     break;
//                 case 3 when rotation && indexRow > maxRow: // 下
//                     newPos.y += height;
//                     break;
//             }
//
//             newPos.x = Mathf.Clamp(newPos.x, 0, content.rect.width - col * width);
//             newPos.y = Mathf.Clamp(newPos.y, 0, content.rect.height - row * height);
//             content.anchoredPosition = newPos;
//         }
//
//         public override void Enqueue()
//         {
//             for (int i = 0; i < grids.Count; i++)
//             {
//                 grids[i].Dispose();
//                 PoolManager.Hide(grids[i]);
//             }
//
//             items = null;
//             OnMove = null;
//             grids.Clear();
//             nodes.Clear();
//         }
//     }
//

// }