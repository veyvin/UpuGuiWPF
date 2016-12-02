
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using tar_cs;


namespace UpuCore
{
    public class KISSUnpacker
    {
        private string GetDefaultOutputPathName(string inputFilepath, string outputPath = null)
        {
            FileInfo fileInfo = new FileInfo(inputFilepath);
            bool flag = false;
            if (outputPath == null)
            {
                outputPath = Path.Combine(fileInfo.Directory.FullName, fileInfo.Name + "_unpacked");
                flag = true;
            }
            if (Directory.Exists(outputPath) && flag)
            {
                int num = 2;
                string path;
                while (true)
                {
                    path = Path.Combine(fileInfo.Directory.FullName, string.Concat(new object[4]
                    {
            (object) outputPath,
            (object) " (",
            (object) num,
            (object) ")"
                    }));
                    if (Directory.Exists(path))
                        ++num;
                    else
                        break;
                }
                Directory.CreateDirectory(path);
                outputPath = path;
            }
            return outputPath;
        }

        public string GetTempPath()
        {
            return Path.Combine(Path.Combine(Path.GetTempPath(), "Upu"), Path.GetRandomFileName());
        }

        public Dictionary<string, string> Unpack(string inputFilepath, string outputPath)
        {
            Console.WriteLine("Extracting " + inputFilepath + " to " + outputPath);
            FileInfo fileInfo = new FileInfo(inputFilepath);
            if (!File.Exists(inputFilepath))
            {
                inputFilepath = Path.Combine(Environment.CurrentDirectory, inputFilepath);
                if (!File.Exists(inputFilepath))
                    throw new FileNotFoundException(inputFilepath);
            }
            if (!inputFilepath.ToLower().EndsWith(".unitypackage"))
                throw new ArgumentException("File should have unitypackage extension");
            outputPath = this.GetDefaultOutputPathName(inputFilepath, outputPath);
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            string tempPath = this.GetTempPath();
            string str1 = Path.Combine(tempPath, "_UPU_TAR");
            string tarFileName = this.DecompressGZip(new FileInfo(inputFilepath), str1);
            string str2 = Path.Combine(tempPath, "content");
            this.ExtractTar(tarFileName, str2);
            Directory.Delete(str1, true);
            return this.GenerateRemapInfo(str2, outputPath);
        }

        private void RemoveTempFiles(string tempPath)
        {
            DirectoryInfo directoryInfo1 = new DirectoryInfo(tempPath);
            foreach (FileSystemInfo fileSystemInfo in directoryInfo1.GetFiles())
                fileSystemInfo.Delete();
            foreach (DirectoryInfo directoryInfo2 in directoryInfo1.GetDirectories())
                directoryInfo2.Delete(true);
        }

        private Dictionary<string, string> GenerateRemapInfo(string extractedContentPath, string remapPath)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (DirectoryInfo directoryInfo in new DirectoryInfo(extractedContentPath).GetDirectories())
            {
                string path2 = File.ReadAllLines(Path.Combine(directoryInfo.FullName, "pathname"))[0].Replace('/', Path.DirectorySeparatorChar);
                string key = Path.Combine(directoryInfo.FullName, "asset");
                string fileName = Path.Combine(remapPath, path2);
                string fullName = new FileInfo(fileName).Directory.FullName;
                dictionary.Add(key, fileName);
            }
            return dictionary;
        }

        public void RemapFiles(Dictionary<string, string> map)
        {
            foreach (KeyValuePair<string, string> keyValuePair in map)
            {
                string str = keyValuePair.Value;
                string key = keyValuePair.Key;
                FileInfo fileInfo = new FileInfo(keyValuePair.Value);
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Console.WriteLine("Creating directory " + str + "...");
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                if (File.Exists(key))
                {
                    Console.WriteLine("Extracting file " + str + "...");
                    if (File.Exists(str))
                        File.Delete(str);
                    File.Move(key, str);
                }
            }
        }

        private string DecompressGZip(FileInfo fileToDecompress, string outputPath)
        {
            using (FileStream fileStream1 = fileToDecompress.OpenRead())
            {
                string path2 = fileToDecompress.Name;
                if (fileToDecompress.Extension.Length > 0)
                    path2 = path2.Remove(path2.Length - fileToDecompress.Extension.Length);
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);
                string path = Path.Combine(outputPath, path2);
                using (FileStream fileStream2 = File.Create(path))
                {
                    using (GZipStream gzipStream = new GZipStream((Stream)fileStream1, CompressionMode.Decompress))
                    {
                        this.CopyStreamDotNet20((Stream)gzipStream, (Stream)fileStream2);
                        Console.WriteLine("Decompressed: {0}", (object)fileToDecompress.Name);
                    }
                }
                return path;
            }
        }

        private void CopyStreamDotNet20(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, count);
        }

        public bool ExtractTar(string tarFileName, string destFolder)
        {
            Console.WriteLine("Extracting " + tarFileName + " to " + destFolder + "...");
            string currentDirectory = Directory.GetCurrentDirectory();
            using (Stream tarredData = (Stream)File.OpenRead(tarFileName))
            {
                Directory.CreateDirectory(destFolder);
                Directory.SetCurrentDirectory(destFolder);
                new TarReader(tarredData).ReadToEnd(".");
            }
            Directory.SetCurrentDirectory(currentDirectory);
            return true;
        }
    }
}
