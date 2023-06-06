using System;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerPossessPayloadParser : IPayloadParser<PlayerPossessPayload>
    {
        private static readonly Regex PayloadRegex = new(@"\[DedicatedServer](?:ASQPlayerController::)?OnPossess\(\): PC=(.+) Pawn=([A-z0-9_]+_C)");
        
        public LogMessageType LogMessageType => LogMessageType.SquadTrace;

        public PlayerPossessPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerPossessPayload(
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