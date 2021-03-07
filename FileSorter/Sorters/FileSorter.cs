using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileSorter.Models;

namespace FileSorter.Sorters
{
    public interface IFileSorter
    {
        void Add(Line bLine);
        IEnumerable<Line> Sort();
    }
    
    public class FileSorter : IFileSorter
    {
        private readonly NodeCollection _node;

        public FileSorter()
        {
            this._node = new NodeCollection();
        }

        public void Add(Line bLine)
        {
            this._node.Add(bLine);
        }

        public IEnumerable<Line> Sort()
        {
            var leaves = _node.GetLeaves().ToList();
            var sortingLeaves = new IEnumerable<Line>[leaves.Count];

            Parallel.For(0, leaves.Count, i =>
            {
                var sorted = leaves[i].Sort();
                sortingLeaves[i] = sorted;
            });

            foreach (var sortingLeaf in sortingLeaves)
            {
                foreach (var bLine in sortingLeaf)
                {
                    yield return bLine;
                }
            }
        }
    }
}