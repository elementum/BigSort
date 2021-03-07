using System.Threading;

namespace FileSorter.FileSystem
{
    public class TempFolder
    {
        public string Path { get; }

        private int _used;
        public int Used => _used;

        public TempFolder(string path)
        {
            Path = path;
        }

        public void Use()
        {
            Interlocked.Increment(ref _used);
        }

        public void Release()
        {
            Interlocked.Decrement(ref _used);
        }
    }
}