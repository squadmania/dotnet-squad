using System.Reflection;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerRevivedPayloadParser : IPayloadParser<PlayerRevivedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"(.+) has revived (.+)\.");

        public LogMessageType LogMessageType => LogMessageType.Squad;

        public PlayerRevivedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerRevivedPayload(
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