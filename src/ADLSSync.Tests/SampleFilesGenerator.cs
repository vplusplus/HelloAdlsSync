
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADLSSync.Tests
{
    [TestClass]
    public class SampleFilesGenerator
    {
        const int OneKB = 1024;
        const int OneMB = OneKB * 1024;

        [TestMethod]
        public void CreateSampleDataFiles()
        {
            const int FileCount = 10;
            const int FileSize = OneMB * 100;
            const string BaseFolder = "D:/Junk/SampleFiles/DeleteMe";

            if (!Directory.Exists(BaseFolder)) Directory.CreateDirectory(BaseFolder);

            for (int i=0; i<FileCount; i++)
            {
                var timer = Stopwatch.StartNew();
                var fileName = Path.Combine(BaseFolder, $"File{i:00}.dat");

                var bytesWritten = 0;
                using(var writer = File.CreateText(fileName))
                {
                    while(bytesWritten < FileSize)
                    {
                        var more = Guid.NewGuid().ToString();
                        writer.Write(more);
                        bytesWritten += more.Length;
                    }
                }
                timer.Stop();

                var sizeInMB = bytesWritten / 1024 / 1024;
                var megaBitsPerSec = (bytesWritten * 8.0 / 1024.0 / 1024.0) / (timer.Elapsed.TotalSeconds);
                Console.WriteLine($"File #{i}: {sizeInMB:#,0.00} MB | {timer.Elapsed.TotalMilliseconds:#,0.00} milliSec | {megaBitsPerSec:#,0.00} Mb/s");
            }
        }
    }
}
