using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileSorter.Models;

namespace FileSorter.FileSystem
{
    public interface IFileReader
    {
        IEnumerable<Line> ReadLines();
    }

    public class FileReader : IFileReader
    {
        private const char NULL = (char) 0;
        private const char PERIOD = '.';
        private const char EOL = (char) 10;

        public FileReader(Stream stream)
        {
            stream.Position = 0;

            _reader = new StreamReader(new BufferedStream(stream, 1024 * 1024 * 250), Encoding.UTF8, false);
        }

        private char[] _charsLeft = new char[0];
        private readonly StreamReader _reader;

        private IEnumerable<Line> ReadBlock(StreamReader reader)
        {
            var charsToRead = 25_000_000;
            var chars = new char[charsToRead];

            for (var i = 0; i < _charsLeft.Length; i++)
            {
                chars[i] = _charsLeft[i];
            }

            var endOfStringPosition = -1;
            var endOfNumberPosition = 0;

            reader.ReadBlock(chars, _charsLeft.Length, charsToRead - _charsLeft.Length);

            LineNumber? number = null;

            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == PERIOD)
                {
                    number = new LineNumber(chars, endOfStringPosition + 1, i - 1);
                    i += 2;
                    endOfNumberPosition = i;
                    continue;
                }

                if (chars[i] == EOL)
                {
                    var content = new LineContent(chars, endOfNumberPosition, i - 1);
                    endOfStringPosition = i;
                    yield return new Line(number.Value, content, chars);

                    number = null;
                    continue;
                }

                if (chars[i] == NULL)
                {
                    if (number != null)
                    {
                        var content = new LineContent(chars, endOfNumberPosition, i - 1);
                        yield return new Line(number.Value, content, chars);
                    }

                    _charsLeft = new char[0];
                    
                    yield break;
                }
            }

            var charsLeft = charsToRead - endOfStringPosition - 1;
            _charsLeft = new char[charsLeft];

            for (var i = 0; i < charsLeft; i++)
            {
                _charsLeft[i] = chars[endOfStringPosition + 1 + i];
            }
        }

        public IEnumerable<Line> ReadLines()
        {
            while (true)
            {
                var linesRead = 0;
                foreach (var line in ReadBlock(_reader))
                {
                    linesRead++;
                    yield return line;
                }

                if (linesRead == 0)
                {
                    break;
                }
            }
        }
    }
}