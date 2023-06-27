using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squadmania.Squad.LogParser
{
    public sealed class FileLineReader : ILineReader
    {
        private readonly string _filepath;

        public FileLineReader(
            string filepath)
        {
            _filepath = Path.GetFullPath(filepath);
        }

        public async IAsyncEnumerator<string> GetAsyncEnumerator(
            CancellationToken cancellationToken = new()
        )
        {
            await using var fileStream = File.Open(
                _filepath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var streamReader = new StreamReader(fileStream);

            while (!cancellationToken.IsCancellationRequested)
            {
                yield return await streamReader.ReadLineAsync();
            }
        }
    }
}