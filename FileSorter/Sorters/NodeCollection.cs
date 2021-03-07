using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FileSorter.Comparers;
using FileSorter.Models;

namespace FileSorter.Sorters
{
    class NodeCollection : ILeafCollection
    {
        public NodeCollection(byte charIndex = 0)
        {
            this._charIndex = charIndex;
        }

        private NodeCollection(byte charIndex, ILeafCollection innerCollection): this(charIndex)
        {
            foreach (var leaf in innerCollection)
            {
                this.Add(leaf);
            }
        }

        private readonly Dictionary<char, ILeafCollection> _children = new Dictionary<char, ILeafCollection>();
        private readonly byte _charIndex;

        public IEnumerator<Line> GetEnumerator()
        {
            return _children.SelectMany(c => c.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Add(Line leaf)
        {
            var @char = leaf.Content.CharAt(_charIndex);
            if (!_children.TryGetValue(@char, out var node))
            {
                var comparer = GetComparer(@char, _charIndex);
                node = new LeafCollection(comparer, @char);
                _children[@char] = node;
            }

            var added = node.Add(leaf);
            if (!added)
            {
                var newCollection = new NodeCollection((byte) (this._charIndex + 1), node);
                node.Dispose();

                _children[@char] = newCollection;
                newCollection.Add(leaf);
            }

            return true;
        }

        private IComparer<Line> GetComparer(char @char, int charIndex)
        {
            IComparer<Line> comparer;
            if (@char == (char) 0)
                comparer = new NumberComparer();
            else comparer = new StringAndNumberComparer(charIndex);

            return comparer;
        }

        public IEnumerable<LeafCollection> GetLeaves()
        {
            return _children.OrderBy(c => c.Key).SelectMany(c => c.Value.GetLeaves());
        }

        public void Dispose()
        {
            foreach (var value in this._children.Values)
            {
                value.Dispose();
            }
        }
    }
}