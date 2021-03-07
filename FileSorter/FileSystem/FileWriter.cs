using System;
using System.IO;
using System.Text;
using FileSorter.Models;

namespace FileSorter.FileSystem
{
    public interface IFileWriter
    {
        void Write(Line line);
        Stream FinishWriting();
    }
    
    public class FileWriter : IFileWriter
    {
        private readonly StreamWriter _writer;
        private readonly Stream _stream;

        public FileWriter(Stream stream)
        {
            _stream = stream;
            _writer = new StreamWriter(_stream, Encoding.UTF8, -1, true) {NewLine = "\n"};
        }

        public void Write(Line line)
        {
            _writer.WriteLine(new ReadOnlySpan<char>(line.Chars, line.StartIndex, line.Size));
        }

        public Stream FinishWriting()
        {
            _writer.Flush();
            return _stream;
        }
    }
}