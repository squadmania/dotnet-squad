using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class RoundWinnerPayloadParser : IPayloadParser<RoundWinnerPayload>
    {
        private static readonly Regex PayloadRegex = new Regex(
            @"\[DedicatedServer](?:ASQGameMode::)?DetermineMatchWinner\(\): (.+) won on (.+)"
        );
        
        public LogMessageType LogMessageType => LogMessageType.SquadTrace;

        public RoundWinnerPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new RoundWinnerPayload(
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