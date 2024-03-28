using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fanx.Util;
using Loon.Java;

namespace TaraTools
{
    public static class TaraTools
    {
        public static string ToTara(this DirectoryInfo directory)
        {
            var outputPath = $"{directory.FullName}.tara";
            directory.ToTara(outputPath);
            return outputPath;
        }

        public static void ToTara(this DirectoryInfo directory, string outputFileName) =>
            WriteTara(directory.GetFiles("*.*", SearchOption.AllDirectories), outputFileName, directory.FullName);

        public static void WriteTara(IEnumerable<FileInfo> files, string outputFileName, string root = "")
        {
            if (string.IsNullOrEmpty(outputFileName))
                throw new ArgumentException("Output file name cannot be null.");
            if (Path.GetExtension(outputFileName) != ".tara")
                outputFileName = $"{outputFileName}.tara";

            DirectoryInfo? parent = Directory.GetParent(outputFileName);
            if (parent != null) Directory.CreateDirectory(parent.FullName);

            using (DataOutputStream writer = new(File.Open(outputFileName, FileMode.Create)))
            {
                var fileEntries = files as FileInfo[] ?? files.ToArray();
                writer.WriteInt(fileEntries.Length);
                foreach (FileInfo fileEntry in fileEntries)
                {
                    string relativeName =
                        string.IsNullOrEmpty(root) || !fileEntry.FullName.Contains(root)
                            ? fileEntry.Name
                            : fileEntry.FullName.Replace(root, string.Empty)[1..];
                    writer.WriteUTF(relativeName);
                    writer.WriteInt((int) fileEntry.Length);
                }

                foreach (FileInfo fileEntry in fileEntries)
                {
                    byte[] bytes = File.ReadAllBytes(fileEntry.FullName);
                    writer.Write(bytes);
                }
            }
        }

        public static FileEntry[] ReadTara(string path)
        {
            if (string.IsNullOrEmpty(path) || Path.GetExtension(path) != ".tara")
                throw new ArgumentException("Only *.tara files are supported.");

            using FileStream baseStream = File.Open(path, FileMode.Open);
            DataReader reader = new DataReader(baseStream);
            List<(string fileName, int fileSize)> filesMap = new();

            int numFiles = reader.ReadInt();
            for (int i = 0; i < numFiles; i++)
            {
                string fileName = reader.ReadUTF();
                int fileSize = reader.ReadInt();
                filesMap.Add(new(fileName, fileSize));
            }

            var fileEntries =
                ReadFilesFromTaraStream(filesMap, reader)
                    .ToArray();
            baseStream.Close();

            return fileEntries;
        }

        private static IEnumerable<FileEntry> ReadFilesFromTaraStream(
            List<(string fileName, int fileSize)> filesMap,
            DataReader reader)
        {
            foreach ((string fileName, int fileSize) fileData in filesMap)
                yield return new FileEntry(fileData.fileName, reader.ReadBytes(fileData.fileSize));
        }
    }

    public class FileEntry
    {
        public readonly string RelativeName;
        public readonly byte[] Bytes;
        public readonly FileType Type;

        public FileEntry(string relativeName, byte[] bytes)
        {
            RelativeName = relativeName;
            Bytes = bytes;
            Type = GetFileType(relativeName);
        }

        public void Save(string path)
        {
            Directory.CreateDirectory(path);
            string outputPath = Path.Combine(path, RelativeName);
            if (RelativeName.Contains('\\') || RelativeName.Contains('/'))
            {
                var directoryInfo = Directory.GetParent(outputPath);
                if (directoryInfo?.FullName != null)
                    Directory.CreateDirectory(directoryInfo.FullName);
            }

            File.WriteAllBytes(outputPath, Bytes);
        }

        private static FileType GetFileType(string relativeName)
        {
            switch (Path.GetExtension(relativeName).ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".tiff":
                case ".webp":
                    return FileType.Image;
                case ".3ds":
                    return FileType.Model;
                case ".xml":
                    return FileType.XML;
                case ".json":
                    return FileType.JSON;
                default:
                    return FileType.Unclassified;
            }
        }
    }

    public enum FileType
    {
        Unclassified = 0,
        Image = 1,
        Model = 2,
        XML = 3,
        JSON = 4,
    }
}