using System;
using System.Collections.Generic;
using FileSorter.Models;

namespace FileSorter.Sorters
{
    public class NodeStorage : IDisposable
    {
        private readonly int _capacity;

        private long _written = 0;
        private List<Line> _cache;

        public NodeStorage(int capacity)
        {
            _capacity = capacity;
            _cache = new List<Line>(1024);
        }

        public bool Write(Line line)
        {
            if (this._written == this._capacity)
                return false;

            _cache.Add(line);

            _written++;

            return true;
        }

        public List<Line> Read()
        {
            var toReturn = this._cache;
            this._cache = new List<Line>(1024);

            return toReturn;
        }

        public void Dispose()
        {
            _cache.Clear();
        }
    }
}