using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSorter.Models;
using FileSorter.Sorters;

namespace FileSorter.FileSystem
{
    public class FileSplitterOptions
    {
        public string SourceFile { get; set; }
        public TempFolderCollection TempFolders { get; set; }
        public int MaxChunkSize { get; set; }
    }

    public class FileSplitter
    {
        private const int MinChunkSize = 100 * 1024 * 1024;
        
        private readonly Semaphore _semaphore;
        private readonly FileSplitterOptions _options;

        public FileSplitter(Semaphore semaphore, FileSplitterOptions options)
        {
            _semaphore = semaphore;
            _options = options;
        }

        private int GetChunkSize(int iteration)
        {
            var multiplier = 1 + iteration / Environment.ProcessorCount;
            return Math.Min(MinChunkSize * multiplier, _options.MaxChunkSize);
        }

        public void Split(BlockingCollection<Stream> chunks)
        {
            using (var fileToSort = File.OpenRead(_options.SourceFile))
            {
                _semaphore.WaitOne(TimeSpan.FromHours(1));

                var reader = new FileReader(fileToSort);
                var fileFactory = new FileFactory(_options.TempFolders);
                var sorter = new Sorters.FileSorter();

                var sizeRead = 0;
                var lines = reader.ReadLines();

                var iteration = 1;
                var chunkSize = GetChunkSize(iteration);
                foreach (var bLine in lines)
                {
                    sorter.Add(bLine);
                    sizeRead += bLine.Size;

                    if (sizeRead > chunkSize)
                    {
                        SortAndWrite(sorter, fileFactory, chunks);
                        sizeRead = 0;

                        iteration++;
                        chunkSize = GetChunkSize(iteration);
                    }
                }

                SortAndWrite(sorter, fileFactory, chunks);

                chunks.CompleteAdding();
                _semaphore.Release();
            }
        }

        public void SortAndWrite(IFileSorter sorter, IFileFactory fileFactory, BlockingCollection<Stream> chunks)
        {
            var sortedLines = sorter.Sort();
            var stream = Write(fileFactory, sortedLines);
                        
            chunks.Add(stream);
        }

        private Stream Write(IFileFactory fileFactory, IEnumerable<Line> lines)
        {
            var writer = fileFactory.WriteNewFile();

            foreach (var line in lines)
                writer.Write(line);

            return writer.FinishWriting();
        }
    }
}