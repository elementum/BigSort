using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSorter.FileSystem
{
    public class TempFolderCollection
    {
        private readonly List<TempFolder> _folders = new List<TempFolder>();
        private readonly List<string> _usedDrives = new List<string>();
        
        public TempFolderCollection(IEnumerable<TempFolder> tempFolderPaths)
        {
            foreach (var tempFolderPath in tempFolderPaths)
                Add(tempFolderPath);
            
            if (_folders.Count == 0)
                Add(new TempFolder(Path.GetTempPath()));
        }
        
        private void Add(TempFolder tempFolder)
        {
            if(tempFolder == null)
                throw new ArgumentNullException(nameof(tempFolder));

            var drive = new DirectoryInfo(tempFolder.Path).Root.FullName;
            if (!_usedDrives.Contains(drive))
            {
                _folders.Add(tempFolder);
                _usedDrives.Add(drive);
            }
        }

        public TempFolder GetLeastUsedFolder()
        {
            var leastUsedFoldersGroup = this._folders.GroupBy(t => t.Used).OrderBy(t => t.Key).Select(t => t.ToList()).FirstOrDefault();
            if (leastUsedFoldersGroup.Count == 1)
                return leastUsedFoldersGroup.FirstOrDefault();

            return leastUsedFoldersGroup[new Random().Next(0, leastUsedFoldersGroup.Count)];
        }
    }
}