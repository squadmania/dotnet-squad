
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ListTeamsParser : ICommandParser<List<Team>>
    {
        private const string Header = "----- Active Squads -----\n";

        private static readonly Regex TeamRegex = new ("^Team ID: ([0-9]+) \\((.+)\\)$");

        public List<Team> Parse(
            string input
        )
        {
            
            input = input
                .Replace(Header, "")
                .Replace("\r\n", "\n");
            var lines = input.Split("\n");

            var teams = new List<Team>();
            
            foreach (var line in lines)
            {
                var match = TeamRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }
                
                var teamId = (TeamId)int.Parse(match.Groups[1].Value);
                var teamName = match.Groups[2].Value;
                    
                teams.Add(new Team(teamId, teamName));
            }

            return teams;
        }
    }
}