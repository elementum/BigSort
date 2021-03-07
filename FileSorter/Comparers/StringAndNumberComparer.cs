using System;
using System.Collections.Generic;
using FileSorter.Models;

namespace FileSorter.Comparers
{
    public class StringAndNumberComparer : IComparer<Line>
    {
        private readonly int _charIndex;

        public StringAndNumberComparer(int charIndex)
        {
            _charIndex = charIndex;
        }

        public int Compare(Line that, Line other)
        {
            var result = CompareContent(that.Content, other.Content);
            if (result == 0)
                return that.Number.CompareTo(other.Number);

            return result;
        }

        private int CompareContent(LineContent that, LineContent other)
        {
            var minLength = Math.Min(that.Length, other.Length);

            for (var i = _charIndex; i < minLength; i++)
            {
                var char1 = that.CharAt(i);
                var char2 = other.CharAt(i);

                var result = char1.CompareTo(char2);
                if (result != 0)
                    return result;
            }

            return that.Length.CompareTo(other.Length);
        }
    }
}