using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileSorter.Models;

namespace FileSorter.FileSystem
{
    public class FileMergerOptions
    {
        public TempFolderCollection TempFolders { get; set; }
        public string DestinationFile { get; set; }
        public int MaxLevel { get; set; }
    }

    public class FinalLevelFileMerger : FileMerger
    {
        private readonly FileMergerOptions _options;

        public FinalLevelFileMerger(int level, Semaphore semaphore, FileMergerOptions options) : base(level, semaphore,
            options)
        {
            _options = options;
        }

        protected override Task MergeNextLevel()
        {
            return Task.CompletedTask;
        }

        protected override Task HandleStreamLeftOver(List<Stream> streamsToMerge)
        {
            return MergeChunk(streamsToMerge);
        }

        protected override IFileWriter CreateDestinationFile()
        {
            return new FileWriter(File.Create(_options.DestinationFile));
        }

        public override void Dispose()
        {
            foreach (var fileStream in FileStreams)
            {
                fileStream.Dispose();
            }
        }
    }

    public class FileMerger : IDisposable
    {
        private readonly int _level;
        private readonly Semaphore _semaphore;
        private readonly FileMergerOptions _options;
        protected readonly BlockingCollection<Stream> FileStreams;
        private int _chunksAdded = 0;
        private readonly int _minStreamsToMerge;

        private Task _nextLevelMergeTask = Task.CompletedTask;
        private FileMerger _nextLevelMerger = null;

        public FileMerger(int level, Semaphore semaphore, FileMergerOptions options)
        {
            _level = level;
            _semaphore = semaphore;
            _options = options;
            FileStreams = new BlockingCollection<Stream>();

            _minStreamsToMerge = level == options.MaxLevel ? Int32.MaxValue : 2;
        }

        protected virtual Task MergeNextLevel()
        {
            if (_level == _options.MaxLevel - 1)
                _nextLevelMerger = new FinalLevelFileMerger(_level + 1, _semaphore, _options);
            else _nextLevelMerger = new FileMerger(_level + 1, _semaphore, _options);

            return _nextLevelMerger.Merge(FileStreams);
        }

        protected virtual Task HandleStreamLeftOver(List<Stream> streamsToMerge)
        {
            if (streamsToMerge.Count > 1)
            {
                return MergeChunk(streamsToMerge);
            }

            if (streamsToMerge.Count == 1)
            {
                FileStreams.Add(streamsToMerge[0]);
            }

            return Task.CompletedTask;
        }

        protected virtual async Task Listen(BlockingCollection<Stream> streamCollection)
        {
            Console.WriteLine("Listening for filestreams at level " + _level);
            
            var streams = streamCollection.GetConsumingEnumerable();
            var mergeTasks = new List<Task>();
            var streamsToMerge = new List<Stream>();

            foreach (var stream in streams)
            {
                Console.WriteLine("Chunk received at level " + _level);
                streamsToMerge.Add(stream);

                if (streamsToMerge.Count >= _minStreamsToMerge)
                {
                    var mergeTask = MergeChunk(Interlocked.Exchange(ref streamsToMerge, new List<Stream>()));
                    mergeTasks.Add(mergeTask);
                }

                if (_nextLevelMerger == null)
                    _nextLevelMergeTask = MergeNextLevel();
            }

            mergeTasks.Add(HandleStreamLeftOver(streamsToMerge));

            await Task.WhenAll(mergeTasks);
        }


        public Task Merge(BlockingCollection<Stream> streamCollection)
        {
            return Task.Factory.StartNew(() =>
            {
                Listen(streamCollection).Wait();
                FileStreams.CompleteAdding();

                Console.WriteLine("Level " + _level + " finished");

                _nextLevelMergeTask.Wait();
            });
        }

        protected Task MergeChunk(List<Stream> streamsToMerge)
        {
            return Task.Factory.StartNew(() =>
            {
                var fileStream = this.Merge(streamsToMerge);
                FileStreams.Add(fileStream);

                Interlocked.Increment(ref _chunksAdded);
            });
        }

        protected virtual IFileWriter CreateDestinationFile()
        {
            var fileFactory = new FileFactory(_options.TempFolders);
            return fileFactory.WriteNewFile();
        }

        private Stream Merge(List<Stream> streams)
        {
            _semaphore.WaitOne(TimeSpan.FromHours(1));

            Console.WriteLine("Merging at level " + _level);
            
            var readers = streams.Select(s => new FileReader(s));
            var writer = CreateDestinationFile();

            this.Merge(readers, writer);
            _semaphore.Release();

            foreach (var stream in streams)
                stream.Close();

            return writer.FinishWriting();
        }

        private void Merge(IEnumerable<IFileReader> readers, IFileWriter writer)
        {
            var enumerators = new List<IEnumerator<Line>>();
            var blockingCollection = new BlockingCollection<Line>(5_000_000);
            var writingTask = WriteToFile(writer, blockingCollection);

            // read from each stream one line
            foreach (var enumerator in readers.Select(r => r.ReadLines().GetEnumerator()))
            {
                if (enumerator.MoveNext())
                    enumerators.Add(enumerator);
            }

            // initial sorting of enumerators
            enumerators = enumerators.OrderBy(e => e.Current).ToList();

            while (true)
            {
                // read from the head
                var enumeratorToRead = enumerators[0];
                blockingCollection.Add(enumeratorToRead.Current);
                
                if (!enumeratorToRead.MoveNext())
                {
                    // if file was read to the end remove the enumerator
                    enumerators.Remove(enumeratorToRead);

                    // if only one enumerator left - fast forward
                    if (enumerators.Count == 1)
                    {
                        this.ReadToTheEnd(enumerators.FirstOrDefault(), blockingCollection);
                        break;
                    }

                    if (enumerators.Count == 0)
                    {
                        Console.WriteLine("Merging chunks at level " + _level + " complete");
                        break;
                    }

                    continue;
                }

                for (var i = 1; i < enumerators.Count; i++)
                {
                    var compareResult = enumeratorToRead.Current.CompareTo(enumerators[i].Current);
                    if (compareResult < 0)
                    {
                        // keep enumerator at the head
                        enumerators[i - 1] = enumeratorToRead;
                        break;
                    }
                    else
                    {
                        // move next enumerator one step closer to the head
                        enumerators[i - 1] = enumerators[i];
                    }

                    if (i == enumerators.Count - 1)
                    {
                        // move enumerator to the last position
                        enumerators[i] = enumeratorToRead;
                    }
                }
            }
            
            blockingCollection.CompleteAdding();
            writingTask.Wait();
        }

        private Task WriteToFile(IFileWriter writer, BlockingCollection<Line> blockingCollection)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var bLine in blockingCollection.GetConsumingEnumerable())
                {
                    writer.Write(bLine);
                }
            });
        }

        private void ReadToTheEnd(IEnumerator<Line> enumerator, BlockingCollection<Line> toWrite)
        {
            toWrite.Add(enumerator.Current);
            while (enumerator.MoveNext())
            {
                toWrite.Add(enumerator.Current);
            }
        }

        public virtual void Dispose()
        {
            _nextLevelMerger.Dispose();
        }
    }
}