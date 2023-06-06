using System.Globalization;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class DeployableDamagedPayloadParser : IPayloadParser<DeployableDamagedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"\[DedicatedServer](?:ASQDeployable::)?TakeDamage\(\): ([A-z0-9_]+_C_[0-9]+): ([0-9.]+) damage attempt by causer ([A-z0-9_]+_C_[0-9]+) instigator (.+) with damage type ([A-z0-9_]+_C) health remaining ([0-9.]+)");

        public LogMessageType LogMessageType => LogMessageType.SquadTrace;

        public DeployableDamagedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new DeployableDamagedPayload(
                match.Groups[1].Value,
                float.Parse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                match.Groups[3].Value,
                match.Groups[4].Value,
                match.Groups[5].Value,
                float.Parse(match.Groups[6].Value, NumberStyles.Any, CultureInfo.InvariantCulture)
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