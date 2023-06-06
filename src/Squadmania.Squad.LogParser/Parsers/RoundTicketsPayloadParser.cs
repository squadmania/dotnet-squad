using System;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class RoundTicketsPayloadParser : IPayloadParser<RoundTicketsPayload>
    {
        private static readonly Regex PayloadRegex = new(@"Display: Team ([0-9]), (.*) \( ?(.*?) ?\) has (won|lost) the match with ([0-9]+) Tickets on layer (.*) \(level (.*)\)!");
        
        public LogMessageType LogMessageType => LogMessageType.SquadGameEvents;

        public RoundTicketsPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new RoundTicketsPayload(
                ParseTeam(match.Groups[1].Value),
                match.Groups[2].Value,
                match.Groups[3].Value,
                ParseAction(match.Groups[4].Value),
                int.Parse(match.Groups[5].Value),
                match.Groups[6].Value,
                match.Groups[7].Value
            );
        }

        private RoundTicketsPayloadAction ParseAction(
            string actionStr
        )
        {
            return actionStr switch
            {
                "won" => RoundTicketsPayloadAction.Won,
                "lost" => RoundTicketsPayloadAction.Lost,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Team ParseTeam(
            string teamStr
        )
        {
            var teamId = int.Parse(teamStr);

            return teamId switch
            {
                1 => Team.Team1,
                2 => Team.Team2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public object? Parse(
            string value
        )
        {
            return ParseTyped(value);
        }
    }
}