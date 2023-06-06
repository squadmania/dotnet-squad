using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerUnPossessPayloadParser : IPayloadParser<PlayerUnPossessPayload>
    {
        private static readonly Regex PayloadRegex = new(@"\[DedicatedServer](?:ASQPlayerController::)?OnUnPossess\(\): PC=(.+)");
        
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

            return new PlayerUnPossessPayload(
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