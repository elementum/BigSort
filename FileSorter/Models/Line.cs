using System;

namespace FileSorter.Models
{
    public struct Line : IComparable<Line>
    {
        public char[] Chars { get; }
        public readonly LineNumber Number;
        public readonly LineContent Content;

        public int Size
        {
            get { return this.Number.Length + 2 + this.Content.Length; }
        }

        public int StartIndex
        {
            get
            {
                return this.Number.StartIndex;
            }
        }

        public int EndIndex
        {
            get
            {
                return this.Content.EndIndex;
            }
        }

        public Line(LineNumber number, LineContent content, char[] chars)
        {
            Chars = chars;
            Number = number;
            Content = content;
        }

        public int CompareTo(Line other)
        {
            var result = this.Content.CompareTo(other.Content);
            if (result == 0)
                return this.Number.CompareTo(other.Number);

            return result;
        }

        public override string ToString()
        {
            return $"{Number}. {Content}";
        }
    }
}