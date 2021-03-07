using System.Collections;
using System.Collections.Generic;
using FileSorter.Models;

namespace FileSorter.Sorters
{
    public class LeafCollection : ILeafCollection
    {
        private readonly NodeStorage _storage;
        private readonly IComparer<Line> _comparer;
        private char _char;

        public LeafCollection(IComparer<Line> comparer, char @char)
        {
            _storage = new NodeStorage(1024 * 1024);
            _comparer = comparer;
            _char = @char;
        }

        public bool Add(Line leaf)
        {
            return _storage.Write(leaf);
        }

        public IEnumerable<LeafCollection> GetLeaves()
        {
            return new[] {this};
        }

        public IEnumerable<Line> Sort()
        {
            var lines = _storage.Read();
            lines.Sort(this._comparer);

            return lines;
        }

        public IEnumerator<Line> GetEnumerator()
        {
            // not good, but I've got no time to write sync method
            // this is safe since we're in a console app
            // even though a bit slower
            return _storage.Read().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _storage?.Dispose();
        }
    }
}