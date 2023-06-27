using System.Collections.Generic;
using System.Threading;
using Squadmania.Squad.LogParser.Models;
using Squadmania.Squad.LogParser.Parsers;

namespace Squadmania.Squad.LogParser
{
    public sealed class LogMessageReader : IAsyncEnumerable<LogMessage>
    {
        private readonly ILineReader _lineReader;
        private readonly LogMessageParser _logMessageParser;

        public LogMessageReader(
            ILineReader lineReader,
            LogMessageParser logMessageParser
        )
        {
            _lineReader = lineReader;
            _logMessageParser = logMessageParser;
        }

        public async IAsyncEnumerator<LogMessage> GetAsyncEnumerator(
            CancellationToken cancellationToken = new()
        )
        {
            await foreach (var line in _lineReader)
            {
                var logMessage = _logMessageParser.Parse(line);
                if (logMessage == null)
                {
                    continue;
                }

                yield return logMessage;
            }
        }
    }
}