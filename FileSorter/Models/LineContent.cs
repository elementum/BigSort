using System;

namespace FileSorter.Models
{
    public struct LineContent : IComparable<LineContent>
    {
        private readonly char[] _chars;
        private readonly int _startIndex;
        public readonly int EndIndex;

        public readonly int Length;

        public LineContent(char[] chars, int startIndex, int endIndex)
        {
            _chars = chars;
            _startIndex = startIndex;
            EndIndex = endIndex;

            Length = endIndex - startIndex + 1;
        }

        public char CharAt(int index)
        {
            if (index > Length - 1)
                return (char) 0;

            return _chars[index + _startIndex];
        }

        public int CompareTo(LineContent other)
        {
            var minLength = Math.Max(Length, other.Length);

            for (var i = 0; i < minLength; i++)
            {
                var char1 = CharAt(i);
                var char2 = other.CharAt(i);

                var result = char1.CompareTo(char2);
                if (result != 0)
                    return result;
            }

            return Length.CompareTo(other.Length);
        }

        public override string ToString()
        {
            return new String(new ReadOnlySpan<char>(this._chars, this._startIndex, Length));
        }
    }
}