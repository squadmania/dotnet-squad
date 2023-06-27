using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Squadmania.Squad.LogParser
{
    public class ContinuousFileLineReader : ILineReader
    {
        private readonly string _filepath;

        public ContinuousFileLineReader(
            string filepath
        )
        {
            _filepath = Path.GetFullPath(filepath);
        }

        public async IAsyncEnumerator<string> GetAsyncEnumerator(
            CancellationToken cancellationToken = new()
        )
        {
            var random = new Random(Environment.TickCount);
            
            var readBytes = 0L;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var fileInfo = new FileInfo(_filepath);
                var fileSize = fileInfo.Length;

                if (fileSize == readBytes)
                {
                    continue;
                }

                if (fileSize < readBytes)
                {
                    readBytes = 0;
                }

                await using var fileStream = File.Open(
                    _filepath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                using var streamReader = new StreamReader(fileStream);

                fileStream.Seek(readBytes, SeekOrigin.Begin);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await streamReader.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }

                    yield return line;
                }

                readBytes = fileStream.Length;

                Thread.Sleep(random.Next(100, 500));
            }
        }
    }
}