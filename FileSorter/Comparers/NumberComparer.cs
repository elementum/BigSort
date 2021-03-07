using System.Collections.Generic;
using FileSorter.Models;

namespace FileSorter.Comparers
{
    public class NumberComparer : IComparer<Line>
    {
        public int Compare(Line that, Line other)
        {
            return CompareNumbers(that.Number, other.Number);
        }

        private int CompareNumbers(LineNumber that, LineNumber other)
        {
            return that.CompareTo(other);
        }
    }
}