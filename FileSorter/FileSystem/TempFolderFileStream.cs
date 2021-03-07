using System.IO;

namespace FileSorter.FileSystem
{
    public class TempFolderFileStream : Stream
    {
        private readonly FileStream _stream;
        private readonly TempFolder _tempFolder;

        public TempFolderFileStream(FileStream stream, TempFolder tempFolder)
        {
            _stream = stream;
            _tempFolder = tempFolder;

            _tempFolder.Use();
        }
        
        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Close()
        {
            _stream.Close();
            _tempFolder.Release();
            File.Delete(_stream.Name);
        }
    }
}