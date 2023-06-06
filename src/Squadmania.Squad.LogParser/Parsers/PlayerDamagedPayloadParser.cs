using System.Globalization;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerDamagedPayloadParser : IPayloadParser<PlayerDamagedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"Player:(.+) ActualDamage=([0-9.]+) from (.+) caused by ([A-z_0-9-]+_C)");
        public LogMessageType LogMessageType => LogMessageType.Squad;

        public PlayerDamagedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerDamagedPayload(
                match.Groups[1].Value,
                float.Parse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                match.Groups[3].Value,
                match.Groups[4].Value
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