using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerUnPossessPayloadParser : IPayloadParser<PlayerUnPossessPayload>
    {
        private static readonly Regex PayloadRegex = new(@"\[DedicatedServer](?:ASQPlayerController::)?OnUnPossess\(\): PC=(.+)$");
        private static readonly Regex CurrentHealthRegex = new(@"(.+) current health value ([0-9\.]+)$");
        
        public LogMessageType LogMessageType => LogMessageType.SquadTrace;

        public PlayerUnPossessPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            var possessClassname = match.Groups[1].Value;

            match = CurrentHealthRegex.Match(possessClassname);
            if (match.Success)
            {
                possessClassname = match.Groups[1].Value;
            }

            return new PlayerUnPossessPayload(
                possessClassname
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