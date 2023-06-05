using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squadmania.Squad.LogParser
{
    public class FileLineReader : ILineReader, IDisposable, IAsyncDisposable
    {
        private readonly FileStream _fileStream;
        private readonly StreamReader _streamReader;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public FileLineReader(
            string filepath
        )
        {
            filepath = Path.GetFullPath(filepath);
            _fileStream = File.Open(
                filepath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            _streamReader = new StreamReader(_fileStream);

            _autoResetEvent = new AutoResetEvent(false);
            
            _fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(filepath));
            _fileSystemWatcher.Filter = Path.GetFileName(filepath);
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += FileChangedHandler;
        }

        private void FileChangedHandler(
            object sender,
            FileSystemEventArgs e
        )
        {
            _autoResetEvent.Set();
        }

        public virtual void Dispose()
        {
            _fileStream.Dispose();
            _streamReader.Dispose();
        }

        public virtual string ReadLine()
        {
            var line = _streamReader.ReadLine();
            while (line == null)
            {
                line = _streamReader.ReadLine();
                _autoResetEvent.WaitOne();
            }

            return line;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await _fileStream.DisposeAsync();
            _streamReader.Dispose();
            _fileSystemWatcher.Dispose();
            _autoResetEvent.Dispose();
        }
    }
}