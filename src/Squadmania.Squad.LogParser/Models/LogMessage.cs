using System;
using System.Diagnostics.CodeAnalysis;

namespace Squadmania.Squad.LogParser.Models
{
    public class LogMessage
    {
        public LogMessage(
            string raw,
            LogMessageType type,
            int chainId,
            DateTime dateTime,
            string rawLog,
            object? payload = null
        )
        {
            Raw = raw;
            Type = type;
            DateTime = dateTime;
            ChainId = chainId;
            RawLog = rawLog;
            Payload = payload;
        }

        public string Raw { get; }
        public LogMessageType Type { get; }
        public DateTime DateTime { get; }
        public int ChainId { get; }
        public string RawLog { get; }
        public object? Payload { get; }
    }
}