using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public sealed class SquadCreatedMessageParser
    {
        private static readonly Regex SquadCreatedMessageRegex = new(@"^(.*) \(Steam ID: ([0-9]+)\) has created Squad ([0-9]+) \(Squad Name: (.*)\) on (.*)$"); 
        
        public SquadCreatedMessage? Parse(string value)
        {
            var match = SquadCreatedMessageRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            var playerNameWithoutPrefix = match.Groups[1].Value;
            if (!ulong.TryParse(match.Groups[2].Value, out var playerSteamId64))
            {
                return null;
            }

            if (!int.TryParse(match.Groups[3].Value, out var squadId))
            {
                return null;
            }

            var squadName = match.Groups[4].Value;
            var teamName = match.Groups[5].Value;

            return new SquadCreatedMessage(
                playerNameWithoutPrefix,
                playerSteamId64,
                squadId,
                squadName,
                teamName
            );
        }
    }
}