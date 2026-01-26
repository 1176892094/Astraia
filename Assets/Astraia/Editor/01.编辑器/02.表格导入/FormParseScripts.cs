// // *********************************************************************************
// // # Project: Astraia
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Astraia.Core;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Astraia
{
    internal static partial class FormManager
    {
        public static async Task<bool> WriteScripts(string reason)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                var formPath = new List<string>();
                var formData = Directory.GetFiles(reason);
                foreach (var data in formData)
                {
                    if (IsSupport(data))
                    {
                        formPath.Add(data);
                    }
                }

                Form.Clear();
                var formItem = new Dictionary<string, string>();
                foreach (var path in formPath)
                {
                    var items = LoadScripts(path);
                    foreach (var item in items)
                    {
                        if (!formItem.ContainsKey(item.Key))
                        {
                            formItem.Add(item.Key, item.Value);
                        }
                    }
                }

                Loaded = true;
                var instance = false;
                formItem.Add(GlobalSetting.Assembly, GlobalSetting.LoadAsset(AssetData.Assembly).Replace("REPLACE", GlobalSetting.Define));
                foreach (var item in formItem)
                {
                    instance |= await Task.Run(() => WriteScripts(item.Key, item.Value));
                }

                watch.Stop();
                if (instance)
                {
                    Debug.Log("自动生成脚本完成。耗时: {0}秒".Format((watch.ElapsedMilliseconds / 1000F).ToString("F").Color("G")));
                }
                else
                {
                    await WriteAssets(reason);
                }

                return instance;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                EditorUtility.ClearProgressBar();
                return false;
            }
        }

        private static Dictionary<string, string> LoadScripts(string path)
        {
            var sheetList = LoadDataTable(path);
            if (sheetList == null)
            {
                return new Dictionary<string, string>();
            }

            var dataTable = new Dictionary<string, string>();
            foreach (var (sheetName, sheetData) in sheetList)
            {
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
                        else if (type.StartsWith("enum"))
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

                            var pair = WriteEnum(name, members, type.EndsWith("flag"));
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
            var builder = HeapManager.Dequeue<StringBuilder>();
            var scriptText = GlobalSetting.LoadAsset(AssetData.DataTable).Replace("Template", className);
            var count = 0;
            foreach (var field in fields)
            {
                if (field.Key.EndsWith(":key"))
                {
                    builder.Append("\t\t[Primary]\n");
                }

                var index = field.Key.LastIndexOf(':');
                var fieldName = index < 0 ? field.Key : field.Key.Substring(0, index);
                index = field.Value.LastIndexOf(':');
                string fieldType;
                if (index < 0)
                {
                    fieldType = field.Value;
                }
                else
                {
                    if (field.Value.EndsWith("[]"))
                    {
                        fieldType = field.Value.Substring(0, index) + "[]";
                    }
                    else
                    {
                        fieldType = field.Value.Substring(0, index);
                    }
                }
#if ODIN_INSPECTOR
                builder.AppendFormat("#if ODIN_INSPECTOR && UNITY_EDITOR\n");
                builder.AppendFormat("\t\t[Sirenix.OdinInspector.ShowInInspector]\n");
                builder.AppendFormat("#endif\n");
#endif
                builder.AppendFormat("\t\tpublic {0} {1} => Bytes.Parse<{0}>({2});\n", fieldType, fieldName, count++);
            }

            scriptText = scriptText.Replace("//TODO:1", builder.ToString());

            builder.Length = 0;
            HeapManager.Enqueue(builder);
            return (GlobalSetting.DataPath.Format(className), scriptText);
        }

        private static (string, string) WriteStruct(string className, string classType)
        {
            var builder = HeapManager.Dequeue<StringBuilder>();
            var scriptText = GlobalSetting.LoadAsset(AssetData.Struct).Replace("Template", className);

            var members = classType.Substring(1, classType.IndexOf('}') - 1).Split(',');
            foreach (var member in members)
            {
                var index = member.LastIndexOf(' ');
                var fieldName = member.Substring(index + 1);
                var fieldType = member.Substring(0, index);
                builder.AppendFormat("\t\tpublic {0} {1};\n", fieldType, fieldName);
            }

            builder.Length -= 1;
            scriptText = scriptText.Replace("//TODO:1", builder.ToString());
            builder.Length = 0;
            HeapManager.Enqueue(builder);
            return (GlobalSetting.ItemPath.Format(className), scriptText);
        }

        private static (string, string) WriteEnum(string className, IEnumerable<string> members, bool isFlags)
        {
            var builder = HeapManager.Dequeue<StringBuilder>();
            var scriptText = GlobalSetting.LoadAsset(AssetData.Enum).Replace("Template", className);

            foreach (var member in members)
            {
                if (member != null)
                {
                    builder.AppendFormat("\t\t{0},\n", member);
                }
            }

            builder.Length -= 1;
            scriptText = scriptText.Replace("//TODO:1", isFlags ? "\t[Flags]" : "\t[Serializable]");
            scriptText = scriptText.Replace("//TODO:2", builder.ToString());
            builder.Length = 0;
            HeapManager.Enqueue(builder);
            return (GlobalSetting.EnumPath.Format(className), scriptText);
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