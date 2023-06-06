using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerWoundedPayloadParser : IPayloadParser<PlayerWoundedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"\[DedicatedServer](?:ASQSoldier::)?Wound\(\): Player:(.+) KillingDamage=(?:-)*([0-9.]+) from ([A-z_0-9]+) caused by ([A-z_0-9-]+_C)");
        
        public LogMessageType LogMessageType => LogMessageType.SquadTrace;

        public PlayerWoundedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerWoundedPayload(
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