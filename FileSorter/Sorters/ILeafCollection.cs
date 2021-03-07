using System;
using System.Collections.Generic;
using FileSorter.Models;

namespace FileSorter.Sorters
{
    public interface ILeafCollection : IEnumerable<Line>, IDisposable
    {
        bool Add(Line leaf);
        IEnumerable<LeafCollection> GetLeaves();
    }
}