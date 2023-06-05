using System.Collections.Generic;
using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ListSquadsParser : ICommandParser<List<Models.Squad>>
    {
        private const string Header = "----- Active Squads -----\n";

        private static readonly Regex TeamRegex = new Regex("^Team ID: ([0-9]+) \\(([A-Za-z0-9 \\-_]+)\\)$");

        private static readonly Regex SquadRegex = new Regex(
            "^ID: ([0-9]+) \\| Name: (.*) \\| Size: ([0-9]+) \\| Locked: (True|False) \\| Creator Name: (.*) \\| Creator Steam ID: ([0-9]+)$"
        );

        public List<Models.Squad> Parse(
            string input
        )
        {
            input = input.Replace(Header, "");
            var lines = input.Split("\n");

            var team = Team.Team1;
            var teamName = "";

            var squads = new List<Models.Squad>();
            
            foreach (var line in lines)
            {
                var match = TeamRegex.Match(line);
                if (match.Success)
                {
                    team = (Team)int.Parse(match.Groups[1].Value);
                    teamName = match.Groups[2].Value;
                    
                    continue;
                }

                match = SquadRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                squads.Add(
                    new Models.Squad(
                        int.Parse(match.Groups[1].Value),
                        team,
                        teamName,
                        match.Groups[2].Value,
                        int.Parse(match.Groups[3].Value),
                        match.Groups[5].Value,
                        ulong.Parse(match.Groups[6].Value),
                        match.Groups[4].Value == "True"
                    )
                );
            }

            return squads;
        }
    }
}