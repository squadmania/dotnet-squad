using System;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public class AdminBroadcastPayloadParser : IPayloadParser<AdminBroadcastPayload>
    {
        private static readonly Regex PayloadRegex = new ("ADMIN COMMAND: Message broadcasted <(.+)> from (.+)");

        public LogMessageType LogMessageType => LogMessageType.Squad;

        public AdminBroadcastPayload? ParseTyped(string value)
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new AdminBroadcastPayload(
                match.Groups[1].Value,
                match.Groups[2].Value
            );
        }

        public object? Parse(
            string value
        )
        {
            return ParseTyped(value);
        }
    }
}