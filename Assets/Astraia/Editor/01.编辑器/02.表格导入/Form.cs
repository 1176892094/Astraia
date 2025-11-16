// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 20:04:16
// // # Recently: 2025-04-09 20:04:16
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;

namespace Astraia
{
    internal static partial class FormManager
    {
        private static readonly List<(string, string[,])> Form = new List<(string, string[,])>();
        private static bool Loaded;

        private static async Task<List<(string, string[,])>> LoadDataTable(string filePath)
        {
            if (Loaded)
            {
                return Form;
            }

            var fileType = Path.GetExtension(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileData = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(fileData))
            {
                return Form;
            }

            fileData = Path.Combine(fileData, "{0}_TMP{1}".Format(fileName, fileType));
            File.Copy(filePath, fileData, true);
            try
            {
                var progress = 0F;
                using var archive = ZipFile.OpenRead(fileData);
                var sheetName = LoadSheetName(LoadDocument(archive, "xl/workbook.xml"));
                var sharedString = LoadSharedString(LoadDocument(archive, "xl/sharedStrings.xml"));
                for (var i = 0; i < sheetName.Count; i++)
                {
                    await Task.Run(() =>
                    {
                        var sheet = "xl/worksheets/sheet{0}.xml".Format(i + 1);
                        var worksheet = GetWorksheet(LoadDocument(archive, sheet), sharedString);
                        Form.Add((sheetName[i], worksheet));
                    });
                    EditorUtility.DisplayProgressBar(sheetName[i], "", ++progress / sheetName.Count);
                }

                return Form;
            }
            finally
            {
                File.Delete(fileData);
            }
        }

        private static XmlDocument LoadDocument(ZipArchive archive, string name)
        {
            var zipEntry = archive.GetEntry(name);
            var document = new XmlDocument();
            if (zipEntry != null)
            {
                using var stream = zipEntry.Open();
                document.Load(stream);
            }

            return document;
        }

        private static List<string> LoadSheetName(XmlDocument document)
        {
            var manager = new XmlNamespaceManager(document.NameTable);
            manager.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            var childNodes = document.SelectNodes("//x:sheet", manager);
            var sheetName = new List<string>();
            if (childNodes != null)
            {
                foreach (XmlNode childNode in childNodes)
                {
                    if (childNode.Attributes != null)
                    {
                        sheetName.Add(childNode.Attributes["name"].Value);
                    }
                }
            }

            return sheetName;
        }

        private static List<string> LoadSharedString(XmlDocument document)
        {
            var sharedString = new List<string>();
            if (document.DocumentElement != null)
            {
                foreach (XmlNode childNode in document.DocumentElement.ChildNodes)
                {
                    var shared = string.Empty;
                    foreach (XmlNode node in childNode.ChildNodes)
                    {
                        if (node.Name == "t")
                        {
                            shared += node.InnerText;
                        }
                    }

                    if (!string.IsNullOrEmpty(shared))
                    {
                        sharedString.Add(shared);
                    }
                }
            }

            return sharedString;
        }

        private static string[,] GetWorksheet(XmlDocument document, List<string> sharedStrings)
        {
            var rowNodes = document.GetElementsByTagName("sheetData")[0].ChildNodes;
            if (rowNodes.Count == 0)
            {
                return null;
            }

            var columnCount = GetDimensions(rowNodes[0]);
            if (columnCount == 0)
            {
                return null;
            }

            var dataTable = new string[columnCount, rowNodes.Count];
            SetWorksheet(dataTable, rowNodes, sharedStrings);
            return dataTable;
        }

        private static int GetDimensions(XmlNode node)
        {
            var column = 0;
            var childNode = node.Attributes?["spans"].Value.Split(':')[1];
            if (childNode != null)
            {
                column = int.Parse(childNode);
            }

            return column;
        }


        private static int GetDimensions(string node)
        {
            var column = 0;
            foreach (var c in node)
            {
                if (char.IsLetter(c))
                {
                    column = column * 26 + (c - 'A') + 1;
                }
            }

            return column - 1;
        }

        private static void SetWorksheet(string[,] dataTable, XmlNodeList childNodes, IReadOnlyList<string> sharedStrings)
        {
            var rowCount = dataTable.GetLength(1);
            var columnCount = dataTable.GetLength(0);
            foreach (XmlNode rowNode in childNodes)
            {
                var rowAttribute = rowNode.Attributes?["r"].Value;
                if (string.IsNullOrEmpty(rowAttribute))
                {
                    continue;
                }

                var rowIndex = int.Parse(rowAttribute) - 1;
                foreach (XmlNode cellNode in rowNode.ChildNodes)
                {
                    var cellReference = cellNode.Attributes?["r"]?.Value;
                    var cellValue = cellNode["v"]?.InnerText;

                    if (cellReference != null && cellValue != null)
                    {
                        var column = GetDimensions(cellReference);

                        if (column >= 0 && column < columnCount && rowIndex >= 0 && rowIndex < rowCount)
                        {
                            if (cellNode.Attributes?["t"]?.Value == "s" && int.TryParse(cellValue, out var index))
                            {
                                cellValue = sharedStrings[index];
                            }

                            dataTable[column, rowIndex] = cellValue;
                        }
                    }
                }
            }
        }
    }
}