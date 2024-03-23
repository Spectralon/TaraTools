using Fanx.Util;
using Loon.Java;

namespace TaraTools
{
    public static class TaraTools
    {
        public static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.Error.WriteLine("No args specified");
                    Usage();
                    break;
                case 1:
                    string mode = args[0];
                    if (mode.Equals("pack", StringComparison.OrdinalIgnoreCase) ||
                        mode.Equals("unpack", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine("Only mode specified");
                        Usage();
                    }
                    else
                    {
                        Console.Error.WriteLine("Unknown mode '" + mode + "'");
                        Usage();
                    }

                    break;
                case 2:
                    Usage();
                    break;
                case 3:
                    string useMode = args[0];
                    try
                    {
                        if (useMode.Equals("pack", StringComparison.OrdinalIgnoreCase))
                        {
                            PackTara(args[1], args[2]);
                        }
                        else if (useMode.Equals("unpack", StringComparison.OrdinalIgnoreCase))
                        {
                            UnpackTara(args[1], args[2]);
                        }
                    }
                    catch (IOException e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }

                    break;
                default:
                    Console.Error.WriteLine("Too many args specified");
                    Usage();
                    break;
            }
        }

        private static List<FileEntry> CollectFiles(DirectoryInfo sourceDir, string baseName,
            List<FileEntry>? collector = null)
        {
            collector ??= new();
            FileSystemInfo[] files = sourceDir.GetFileSystemInfos();
            foreach (FileSystemInfo file in files)
            {
                string realName = string.IsNullOrEmpty(baseName) ? file.Name : Path.Combine(baseName, file.Name);
                if (file.Attributes.HasFlag(FileAttributes.Directory))
                {
                    return CollectFiles(new DirectoryInfo(file.FullName), realName, collector);
                }

                collector.Add(new FileEntry(realName, (FileInfo) file));
            }

            return collector;
        }

        private static void PackTara(string inputDirName, string outputFileName)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(inputDirName);
            List<FileEntry> fileEntries = CollectFiles(sourceDir, string.Empty);

            using (DataOutputStream writer = new DataOutputStream(File.Open(outputFileName, FileMode.Create)))
            {
                writer.WriteInt(fileEntries.Count);
                foreach (FileEntry fileEntry in fileEntries)
                {
                    writer.WriteUTF(fileEntry.RelativeName);
                    writer.WriteInt((int) fileEntry.File.Length);
                }

                foreach (FileEntry fileEntry in fileEntries)
                {
                    byte[] bytes = File.ReadAllBytes(fileEntry.File.FullName);
                    writer.Write(bytes);
                }
            }
        }

        private static void UnpackTara(string inputFileName, string outputDirName)
        {
            using FileStream baseStream = File.Open(inputFileName, FileMode.Open);
            DataReader reader = new DataReader(baseStream);
            int numFiles = reader.ReadInt();
            List<ListEntry> files = new List<ListEntry>();

            for (int i = 0; i < numFiles; i++)
            {
                string fileName = reader.ReadUTF();
                int fileSize = reader.ReadInt();
                files.Add(new ListEntry(fileName, fileSize));
            }

            Directory.CreateDirectory(outputDirName);

            foreach (ListEntry entry in files)
            {
                byte[] bytes = reader.ReadBytes(entry.FileSize);
                string outputPath = Path.Combine(outputDirName, entry.FileName);
                if (entry.FileName.Contains('\\') || entry.FileName.Contains('/'))
                {
                    var directoryInfo = Directory.GetParent(outputPath);
                    if (directoryInfo?.FullName != null)
                        Directory.CreateDirectory(directoryInfo.FullName);
                }

                File.WriteAllBytes(outputPath, bytes);
            }
        }

        private static void Usage()
        {
            string usage = "Usage: TaraTools <mode> <input/output file> <input/output folder>\n" +
                           "  Modes:\n" +
                           "    pack - Pack tara\n" +
                           "      Args: <input folder> <output file>\n" +
                           "    unpack - Unpack tara\n" +
                           "      Args: <input file> <output folder>\n";

            Console.Error.WriteLine(usage);
        }

        private class FileEntry
        {
            public string RelativeName;
            public FileInfo File;

            public FileEntry(string relativeName, FileInfo file)
            {
                RelativeName = relativeName;
                File = file;
            }
        }

        private class ListEntry
        {
            public string FileName;
            public int FileSize;

            public ListEntry(string fileName, int fileSize)
            {
                FileName = fileName;
                FileSize = fileSize;
            }
        }
    }
}