using System;
using System.IO;

namespace FileSorter.FileSystem
{
    public interface IFileFactory
    {
        IFileWriter WriteNewFile();
    }
    
    public class FileFactory : IFileFactory
    {
        private readonly TempFolderCollection _tempFolders;

        public FileFactory(TempFolderCollection tempFolders)
        {
            _tempFolders = tempFolders;
        }

        private TempFolderFileStream CreateFile()
        {
            var tempFolder = _tempFolders.GetLeastUsedFolder();
            var newFilePath = Path.Join(tempFolder.Path, Guid.NewGuid().ToString("N"));

            return new TempFolderFileStream(File.Create(newFilePath), tempFolder);
        }
        public virtual IFileWriter WriteNewFile()
        {
            return new FileWriter(CreateFile());
        }
    }
}