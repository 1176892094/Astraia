// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:21
// // # Recently: 2025-04-09 21:04:21
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Astraia.Core;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Astraia
{
    internal static partial class FormManager
    {
        public static async Task WriteAssets(string filePaths)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                var formPath = new List<string>();
                var formData = Directory.GetFiles(filePaths);
                foreach (var data in formData)
                {
                    if (IsSupport(data))
                    {
                        formPath.Add(data);
                    }
                }

                var formItem = new Dictionary<string, List<string[]>>();
                foreach (var excelPath in formPath)
                {
                    var items = LoadAssets(excelPath);
                    foreach (var item in items)
                    {
                        if (!formItem.ContainsKey(item.Key))
                        {
                            formItem.Add(item.Key, item.Value);
                        }
                    }
                }

                var progress = 0F;
                foreach (var data in formItem)
                {
                    await WriteAssets(data.Key, data.Value);
                    EditorUtility.DisplayProgressBar(data.Key, "", ++progress / formItem.Count);
                }

                watch.Stop();
                Debug.Log("自动生成资源完成。耗时: {0}秒".Format((watch.ElapsedMilliseconds / 1000F).ToString("F").Color("G")));
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                Loaded = false;
                DataManager.isLoaded = false;
                DataManager.LoadDataTable();
            }
        }

        private static Dictionary<string, List<string[]>> LoadAssets(string path)
        {
            var sheetList = LoadDataTable(path);
            if (sheetList == null)
            {
                return new Dictionary<string, List<string[]>>();
            }

            var dataTable = new Dictionary<string, List<string[]>>();
            foreach (var (sheetName, sheetData) in sheetList)
            {
                var row = sheetData.GetLength(1);
                var column = sheetData.GetLength(0);
                var columns = new List<int>(column);
                for (var x = 0; x < column; x++)
                {
                    var name = sheetData[x, NAME_LINE];
                    var type = sheetData[x, TYPE_LINE];
                    if (!string.IsNullOrEmpty(name))
                    {
                        if (IsStruct(type))
                        {
                            columns.Add(x);
                        }
                        else if (IsBasic(type))
                        {
                            columns.Add(x);
                        }
                    }
                }

                if (columns.Count == 0)
                {
                    continue;
                }

                var copies = new List<string[]>();
                for (var y = DATA_LINE; y < row; ++y)
                {
                    var rows = new string[columns.Count];
                    for (var x = 0; x < columns.Count; ++x)
                    {
                        var value = sheetData[columns[x], y];
                        if (value != null)
                        {
                            rows[x] = value;
                        }
                        else
                        {
                            rows[x] = string.Empty;
                        }
                    }

                    copies.Add(rows);
                }

                if (copies.Count > 0)
                {
                    dataTable.Add(sheetName, copies);
                }
            }

            return dataTable;
        }

        private static async Task WriteAssets(string sheetName, List<string[]> scripts)
        {
            var filePath = GlobalSetting.DataPath.Format(sheetName);
            if (!File.Exists(filePath))
            {
                return;
            }

            filePath = Path.GetDirectoryName(GlobalSetting.EditTable.Format(sheetName));
            if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            filePath = GlobalSetting.EditTable.Format(sheetName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var fileData = (IDataTable)ScriptableObject.CreateInstance(GlobalSetting.SheetName.Format(sheetName));
            if (fileData == null) return;
            var fileType = Service.Ref.GetType(GlobalSetting.SheetData.Format(sheetName));
            await Task.Run(() =>
            {
                foreach (var column in scripts)
                {
                    if (!string.IsNullOrEmpty(column[0]))
                    {
                        var bytes = column.Select(c => new Xor.Bytes(Service.Text.GetBytes(c))).ToArray();
                        fileData.AddData((IData)Activator.CreateInstance(fileType, bytes));
                    }
                }
            });

            AssetDatabase.CreateAsset((ScriptableObject)fileData, filePath);
        }
    }
}