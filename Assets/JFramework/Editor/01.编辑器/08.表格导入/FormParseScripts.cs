// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 20:04:29
// // # Recently: 2025-04-09 20:04:29
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace JFramework
{
    internal static partial class FormManager
    {
        public static async Task<bool> WriteScripts(string filePaths)
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

                var dataTables = new Dictionary<string, string>();
                foreach (var excelPath in excelPaths)
                {
                    var scripts = LoadScripts(excelPath);
                    foreach (var script in scripts)
                    {
                        if (!dataTables.ContainsKey(script.Key))
                        {
                            dataTables.Add(script.Key, script.Value);
                        }
                    }
                }

                var writeAssets = false;
                dataTables.Add(assemblyPath, assemblyData.Replace("Template", assemblyName));
                var progress = 0f;
                foreach (var data in dataTables)
                {
                    var result = await Task.Run(() => WriteScripts(data.Key, data.Value));
                    EditorUtility.DisplayProgressBar(data.Key, "", ++progress / dataTables.Count);
                    if (result)
                    {
                        writeAssets = true;
                    }
                }

                if (!writeAssets)
                {
                    await WriteAssets(filePaths);
                }

                return writeAssets;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return false;
            }
        }

        private static Dictionary<string, string> LoadScripts(string excelPath)
        {
            var excelFile = LoadDataTable(excelPath);
            if (excelFile == null)
            {
                return new Dictionary<string, string>();
            }

            var dataTable = new Dictionary<string, string>();
            foreach (var excelData in excelFile)
            {
                var sheetName = excelData.Key;
                var sheetData = excelData.Value;
                var row = sheetData.GetLength(1);
                var column = sheetData.GetLength(0);
                var fields = new Dictionary<string, string>();
                for (var x = 0; x < column; x++)
                {
                    var name = sheetData[x, NAME_LINE];
                    var type = sheetData[x, TYPE_LINE];
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = char.ToUpper(name[0]) + name.Substring(1);
                        if (IsStruct(type))
                        {
                            fields.Add(name, type.EndsWith("[]") ? name + "[]" : name);
                            var pair = WriteStruct(name, type);
                            dataTable[pair.Item1] = pair.Item2;
                        }
                        else if (IsBasic(type))
                        {
                            fields.Add(name, type);
                        }
                        else if (type == "enum")
                        {
                            var members = new List<string>();
                            for (var y = DATA_LINE; y < row; y++)
                            {
                                var data = sheetData[x, y];
                                if (!string.IsNullOrEmpty(data))
                                {
                                    members.Add(data);
                                }
                            }

                            var pair = WriteEnum(name, members);
                            dataTable[pair.Item1] = pair.Item2;
                        }
                    }
                }

                if (fields.Count > 0)
                {
                    var pair = WriteTable(sheetName, fields);
                    dataTable[pair.Item1] = pair.Item2;
                }
            }

            return dataTable;
        }

        private static (string, string) WriteTable(string className, Dictionary<string, string> fields)
        {
            var builder = new StringBuilder(1024);
            var scriptText = mainData.Replace("Template", className);

            foreach (var field in fields)
            {
                if (field.Key.EndsWith(":key"))
                {
                    builder.Append("\t\t[Primary]\n");
                }

                var index = field.Key.LastIndexOf(':');
                var fieldName = index < 0 ? field.Key : field.Key.Substring(0, index);
                index = field.Value.LastIndexOf(':');
                var fieldType = index < 0 ? field.Value : field.Value.Substring(0, index);
                var fieldData = char.ToLower(fieldName[0]) + fieldName.Substring(1);
                if (odinInspector)
                {
                    builder.AppendFormat("#if ODIN_INSPECTOR && UNITY_EDITOR\n");
                    builder.AppendFormat("\t\t[Sirenix.OdinInspector.ShowInInspector]\n");
                    builder.AppendFormat("#endif\n");
                    builder.AppendFormat("\t\tpublic {0} {1} => {2}.Parse<{0}>();\n", fieldType, fieldName, fieldData);
                    builder.AppendFormat("\t\t[HideInInspector, SerializeField] private byte[] {0};\n", fieldData);
                }
                else
                {
                    builder.AppendFormat("\t\tpublic {0} {1} => {2}.Parse<{0}>();\n", fieldType, fieldName, fieldData);
                    builder.AppendFormat("\t\t[SerializeField] private byte[] {0};\n", fieldData);
                }
            }

            scriptText = scriptText.Replace("//TODO:1", builder.ToString());
            builder.Length = 0;

            var count = 0;
            foreach (var field in fields)
            {
                count++;
                var index = field.Key.LastIndexOf(':');
                var column = count < fields.Count ? "column++" : "column";
                var fieldName = index < 0 ? field.Key : field.Key.Substring(0, index);
                var fieldData = char.ToLower(fieldName[0]) + fieldName.Substring(1);
                builder.AppendFormat("\t\t\t{0} = Encoding.UTF8.GetBytes(sheet[{1}]);\n", fieldData, column);
            }

            builder.Length -= 1;
            scriptText = scriptText.Replace("//TODO:2", builder.ToString());
            builder.Length = 0;
            return (MainPath(className), scriptText);
        }

        private static (string, string) WriteStruct(string className, string classType)
        {
            var builder = new StringBuilder(1024);
            var scriptText = itemData.Replace("Template", className);

            var members = classType.Substring(1, classType.IndexOf('}') - 1).Split(',');
            foreach (var member in members)
            {
                var index = member.LastIndexOf(' ');
                var fieldName = member.Substring(index + 1);
                var fieldType = member.Substring(0, index);
                var fieldData = char.ToLower(fieldName[0]) + fieldName.Substring(1);
                if (odinInspector)
                {
                    builder.AppendFormat("#if ODIN_INSPECTOR && UNITY_EDITOR\n");
                    builder.AppendFormat("\t\t[Sirenix.OdinInspector.ShowInInspector]\n");
                    builder.AppendFormat("#endif\n");
                    builder.AppendFormat("\t\tpublic {0} {1} => {2}.Parse<{0}>();\n", fieldType, fieldName, fieldData);
                    builder.AppendFormat("\t\t[HideInInspector, SerializeField] private byte[] {0};\n", fieldData);
                }
                else
                {
                    builder.AppendFormat("\t\tpublic {0} {1} => {2}.Parse<{0}>();\n", fieldType, fieldName, fieldData);
                    builder.AppendFormat("\t\t[SerializeField] private byte[] {0};\n", fieldData);
                }
            }

            builder.Length -= 1;
            scriptText = scriptText.Replace("//TODO:1", builder.ToString());
            builder.Length = 0;
            return (ItemPath(className), scriptText);
        }

        private static (string, string) WriteEnum(string className, IEnumerable<string> members)
        {
            var builder = new StringBuilder(1024);
            var scriptText = enumData.Replace("Template", className);

            foreach (var member in members)
            {
                if (member == null) continue;
                var index = member.LastIndexOf(' ');
                if (index < 0)
                {
                    builder.AppendFormat("\t\t{0},\n", member);
                }
                else
                {
                    builder.AppendFormat("\t\t{0} = {1},\n", member.Substring(0, index), member.Substring(index + 1));
                }
            }

            builder.Length -= 1;
            scriptText = scriptText.Replace("//TODO:1", builder.ToString());
            builder.Length = 0;
            return (EnumPath(className), scriptText);
        }

        private static bool WriteScripts(string filePath, string fileData)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                return false;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            if (File.ReadAllText(filePath) == fileData)
            {
                return false;
            }

            File.WriteAllText(filePath, fileData);
            Debug.Log("生成 CSharp 脚本:" + filePath);
            return true;
        }
    }
}