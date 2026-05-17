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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Astraia
{
    internal static partial class FormManager
    {
        private static readonly List<(string, string[,])> Form = new List<(string, string[,])>();
        private static bool Loaded;

        private static IReadOnlyList<(string, string[,])> LoadDataTable(string filePath)
        {
            if (Loaded)
            {
                return Form;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileData = Path.Combine(Path.GetTempPath(), "{0}_{1}".Format(fileName, Guid.NewGuid()));
            File.Copy(filePath, fileData, true);
            try
            {
                using var archive = ZipFile.OpenRead(fileData);
                var sheetNames = ReadSheetNames(archive);
                var sharedStrings = ReadSharedStrings(archive);

                for (var i = 0; i < sheetNames.Count; i++)
                {
                    using var stream = archive.GetEntry("xl/worksheets/sheet{0}.xml".Format(i + 1))!.Open();
                    Form.Add(ReadSheet(stream, sheetNames[i], sharedStrings));
                }
            }
            finally
            {
                File.Delete(fileData);
            }

            return Form;
        }

        private static List<string> ReadSheetNames(ZipArchive archive)
        {
            var result = new List<string>();
            using var stream = archive.GetEntry("xl/workbook.xml")!.Open();
            using var reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "sheet")
                {
                    result.Add(reader.GetAttribute("name"));
                }
            }

            return result;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var result = new List<string>(1024);
            using var stream = archive.GetEntry("xl/sharedStrings.xml")!.Open();
            using var reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "t")
                {
                    result.Add(reader.ReadElementContentAsString());
                }
            }

            return result;
        }

        private static (string, string[,]) ReadSheet(Stream stream, string sheetName, List<string> sharedStrings)
        {
            var maxY = 0;
            var maxX = 0;
            string[,] sheetData = null;
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "dimension" && sheetData == null)
                        {
                            var dimension = reader.GetAttribute("ref");
                            if (!string.IsNullOrEmpty(dimension))
                            {
                                var index = dimension.IndexOf(':');
                                if (index != -1)
                                {
                                    maxX = GetXIndex(dimension.Substring(index + 1)) + 1;
                                    maxY = GetYIndex(dimension.Substring(index + 1));
                                }
                            }

                            sheetData = new string[maxX, maxY];
                        }

                        if (reader.Name == "c")
                        {
                            var r = reader.GetAttribute("r");
                            var t = reader.GetAttribute("t");
                            var x = GetXIndex(r);
                            var y = GetYIndex(r) - 1;

                            var value = string.Empty;
                            if (!reader.IsEmptyElement)
                            {
                                var depth = reader.Depth;
                                while (reader.Read() && reader.Depth > depth)
                                {
                                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "v")
                                    {
                                        value = reader.ReadElementContentAsString();
                                        break;
                                    }
                                }
                            }

                            if (t == "s" && int.TryParse(value, out var index))
                            {
                                value = sharedStrings[index];
                            }

                            if (sheetData != null)
                            {
                                sheetData[x, y] = value;
                            }
                        }
                    }
                }
            }


            return (sheetName, sheetData);
        }

        private static int GetYIndex(string reason)
        {
            var y = string.Empty;
            foreach (var c in reason)
            {
                if (char.IsDigit(c))
                {
                    y += c;
                }
            }

            return int.Parse(y);
        }

        private static int GetXIndex(string reason)
        {
            var x = 0;
            foreach (var c in reason)
            {
                if (char.IsLetter(c))
                {
                    x = x * 26 + (c - 'A') + 1;
                }
            }

            return x - 1;
        }
    }
}