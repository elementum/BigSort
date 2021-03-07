using System;
using System.IO;
using System.Text;

namespace FileGenerator
{
    class Program
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        static void Main(string[] args)
        {
            double size = 0;
            string outFile = null;
            
            try
            {
                size = ReadSize(args);
                outFile = ReadOutFile(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Press any button to exit");
                Console.ReadLine();
                return;
            }

            Generate(size, outFile);
        }

        static void Generate(double sizeInGb, string outFile)
        {
            Console.WriteLine("Generating file");
            
            double sizeInBytes = 1024L * 1024 * 1024 * sizeInGb;
            double writtenBytes = 0;

            var numberRandomizer = new Random();

            using (var writer = File.Create(outFile, 1024 * 1024))
            {
                var newLine = new byte[] {10};

                var iteration = 0;
                
                writer.Write(Encoding.UTF8.GetBytes("1. AAAAAAAAAAAAAA"));
                writer.Write(newLine, 0, newLine.Length);

                writer.Write(Encoding.UTF8.GetBytes("120. AAAAAAAAAAAAAA"));
                writer.Write(newLine, 0, newLine.Length);

                while (writtenBytes < sizeInBytes)
                {
                    var number = numberRandomizer.Next(1, Int32.MaxValue);
                    var @string = GetStringFromNumber(numberRandomizer.Next(1, Int32.MaxValue));
                    var stringToWrite = $"{number}. {@string}";
                    var bytesToWrite = Encoding.UTF8.GetBytes(stringToWrite);

                    writer.Write(bytesToWrite, 0, bytesToWrite.Length);
                    writer.Write(newLine, 0, newLine.Length);

                    writtenBytes += bytesToWrite.Length;

                    if (iteration % 1000000 == 0)
                    {
                        Console.WriteLine($"Written {(writtenBytes / 1024 / 1024 / 1024):0.00} GB");
                    }

                    iteration++;
                }
                writer.Write(Encoding.UTF8.GetBytes("2. AAAAAAAAAAAAAA"));
            }

            Console.WriteLine("Generating finished");
        }
        
        private static string GetStringFromNumber(int number)
        {
            
            var sb = new StringBuilder();
            for (var i = 1; i < 64; i++)
            {
                if (sb.Length == 64)
                    return sb.ToString();

                if ((number & 1 << i) != 0)
                {
                    sb.Append(chars[i % 26]);
                }
            }

            return sb.ToString();
        }

        static double ReadSize(string[] args)
        {
            var sizeArg = ReadParameter(args, "--size");
            if(string.IsNullOrWhiteSpace(sizeArg))
                throw new ArgumentException("Invalid size value");
            
            if (!Double.TryParse(sizeArg, out var size))
                throw new ArgumentException("Invalid size value");

            return size;
        }

        static string ReadOutFile(string[] args)
        {
            var outFile = ReadParameter(args, "--outFile");
            if(string.IsNullOrWhiteSpace(outFile))
                throw new ArgumentException("--outFile was not specified");
            
            return Path.GetFullPath(outFile);
        }

        public static string ReadParameter(string[] args, string parameter)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (parameter.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length> i + 1)
                    {
                        return args[i + 1];
                    }
                }
            }

            return null;
        }
    }
}