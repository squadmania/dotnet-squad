using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerConnectedPayloadParser : IPayloadParser<PlayerConnectedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"Join succeeded: (.+)");
        
        public LogMessageType LogMessageType => LogMessageType.Net;

        public PlayerConnectedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerConnectedPayload(
                match.Groups[1].Value
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