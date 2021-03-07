using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FileSorter.FileSystem;

namespace FileSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, eventArgs) => WriteLine(eventArgs.ExceptionObject.ToString());

            String inFile;
            String outFile;

            try
            {
#if !DEBUG
                inFile = ReadInFile(args);
                outFile = ReadOutFile(args);
#else
                inFile = "file_to_sort.txt";
                outFile = "sorted.txt";
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Press any button to exit");
                Console.ReadLine();
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            WriteLine("Sorting... Please be patient");
            Sort(inFile, outFile);
            WriteLine("Sorting finished after " + stopwatch.Elapsed.ToString());
            WriteLine("");
            Preview(outFile);
            Console.WriteLine("Press any key to quit");
            ReadLine();
        }

        private static void Preview(string sortedFilePath)
        {
            var result = "";

            while (result != "y" && result != "n")
            {
                WriteLine("Do you want to preview the sorted file? (y/n)");
                result = Console.ReadLine()?.ToLower();
            }

            if (result == "y")
            {
                Console.WriteLine("Press enter to scroll through the results, Esc to exit preview");
                using (var stream = File.OpenRead(sortedFilePath))
                {
                    var fileReader = new FileReader(stream);
                    foreach (var line in fileReader.ReadLines())
                    {
                        Console.WriteLine(line);
                        while (true)
                        {
                            var key = Console.ReadKey();
                            if (key.Key == ConsoleKey.Enter)
                                break;

                            if (key.Key == ConsoleKey.Escape)
                                return;
                        }
                    }
                }
            }
        }

        public static string ReadParameter(string[] args, string parameter)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (parameter.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length > i + 1)
                    {
                        return args[i + 1];
                    }
                }
            }

            return null;
        }

        static string ReadInFile(string[] args)
        {
            var inFile = ReadParameter(args, "--inFile");
            if (String.IsNullOrWhiteSpace(inFile))
                throw new ArgumentException("--inFile was not specified");

            var exists = File.Exists(inFile);
            if (!exists)
                throw new ArgumentException("File supplied in --inFile parameter does not exist");

            return inFile;
        }

        static string ReadOutFile(string[] args)
        {
            var inFile = ReadParameter(args, "--outFile");
            if (String.IsNullOrWhiteSpace(inFile))
                throw new ArgumentException("--outFile was not specified");

            return Path.GetFullPath(inFile);
        }

        private static string ReadLine()
        {
            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadLine();
        }

        private static void WriteLine(string value)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
        }

        static int DetermineMaxSplitDepth(string sourceFile, int chunkSize)
        {
            var file = new FileInfo(sourceFile);
            var fileSize = file.Length;
            var maxLevel = 0;

            for (var i = 0;; i++)
            {
                fileSize = fileSize / 4;
                if (fileSize < chunkSize)
                {
                    maxLevel = i + 1;
                    break;
                }
            }

            return Math.Max(maxLevel, Environment.ProcessorCount - 1);
        }

        static void Sort(string sourceFile, string destinationFile)
        {
            Console.ForegroundColor = ConsoleColor.White;

            var semaphore =
                new Semaphore(Environment.ProcessorCount + 1, Environment.ProcessorCount + 1);

            var maxChunkSize = 1024 * 1024 * 1024;
            var maxDepth = DetermineMaxSplitDepth(sourceFile, maxChunkSize);

            var chunks = new BlockingCollection<Stream>();
            var tempFolders = Configuration.Configuration.Instance.TempFolders;

            var fileMerger = new FileMerger(0, semaphore, new FileMergerOptions()
            {
                DestinationFile = destinationFile,
                TempFolders = tempFolders,
                MaxLevel = maxDepth
            });

            var mergeTask = fileMerger.Merge(chunks);

            var fileSplitter = new FileSplitter(semaphore, new FileSplitterOptions()
            {
                SourceFile = sourceFile,
                TempFolders = tempFolders,
                MaxChunkSize = maxChunkSize
            });

            fileSplitter.Split(chunks);
            mergeTask.Wait();

            fileMerger.Dispose();
        }
    }
}