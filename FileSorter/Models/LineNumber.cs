using System;

namespace FileSorter.Models
{
    public readonly struct LineNumber : IComparable<LineNumber>
    {
        private readonly char[] _chars;
        public readonly int StartIndex;
        public readonly int Length;

        public LineNumber(char[] chars, int startIndex, int endIndex)
        {
            _chars = chars;
            StartIndex = startIndex;

            Length = endIndex - startIndex + 1;
        }

        public char CharAt(int index)
        {
            if (index > Length - 1)
                return (char) 0;

            return _chars[index + StartIndex];
        }
        

        public int CompareTo(LineNumber other)
        {
            if (this.Length == other.Length)
            {
                for (var i = 0; i < this.Length; i++)
                {
                    var char1 = CharAt(i);
                    var char2 = other.CharAt(i);

                    var result = char1.CompareTo(char2);
                    if (result != 0)
                        return result;
                }
            }

            return -other.Length.CompareTo(this.Length);
        }

        public override string ToString()
        {
            return new String(new ReadOnlySpan<char>(this._chars, this.StartIndex, Length));
        }
    }
}