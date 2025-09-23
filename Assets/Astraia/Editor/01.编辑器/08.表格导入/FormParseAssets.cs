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
using System.IO;
using System.Threading.Tasks;
using Astraia.Common;
using UnityEditor;
using UnityEngine;

namespace Astraia
{
    internal static partial class FormManager
    {
        public static async Task WriteAssets(string filePaths)
        {
            try
            {
                var excelPaths = new List<string>();
                var excelFiles = Directory.GetFiles(filePaths);
                foreach (var excelFile in excelFiles)
                {
                    if (IsSupport(excelFile))
                    {
                        excelPaths.Add(excelFile);
                    }
                }

                var dataTables = new Dictionary<string, List<string[]>>();
                foreach (var excelPath in excelPaths)
                {
                    await Task.Run(() =>
                    {
                        var assets = LoadAssets(excelPath);
                        foreach (var asset in assets)
                        {
                            if (!dataTables.ContainsKey(asset.Key))
                            {
                                dataTables.Add(asset.Key, asset.Value);
                            }
                        }
                    });
                }

                var progress = 0f;
                foreach (var data in dataTables)
                {
                    await WriteAssets(data.Key, data.Value);
                    EditorUtility.DisplayProgressBar(data.Key, "", ++progress / dataTables.Count);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private static Dictionary<string, List<string[]>> LoadAssets(string excelPath)
        {
            var excelFile = LoadDataTable(excelPath);
            if (excelFile == null)
            {
                return new Dictionary<string, List<string[]>>();
            }

            var dataTable = new Dictionary<string, List<string[]>>();
            foreach (var excelData in excelFile)
            {
                var sheetName = excelData.Item1;
                var sheetData = excelData.Item2;
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

        private static async Task WriteAssets(string sheetName, List<string[]> scriptTexts)
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
                var instance = (IData)Activator.CreateInstance(fileType);
                foreach (var scriptText in scriptTexts)
                {
                    if (!string.IsNullOrEmpty(scriptText[0]))
                    {
                        instance.Create(scriptText, 0);
                        fileData.AddData(instance);
                    }
                }
            });

            AssetDatabase.CreateAsset((ScriptableObject)fileData, filePath);
        }
    }
}