
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ListPlayersParser : ICommandParser<ListPlayersResult>
    {
        private const string ActivePlayersHeader = "----- Active Players -----";
        private const string DisconnectedPlayersHeader = "----- Recently Disconnected Players [Max of 15] -----";

        private static readonly Regex ActivePlayerRegex = new Regex(
            "^ID: ([0-9]+) \\| SteamID: ([0-9]+) \\| Name: (.*) \\| Team ID: ([0-9]+) \\| Squad ID: (N/A|[0-9]+) \\| Is Leader: (False|True) \\| Role: ([A-Za-z0-9_-]+)"
        );

        private static readonly Regex DisconnectedPlayerRegex = new Regex(
            "^ID: ([0-9]+) \\| SteamID: ([0-9]+) \\| Since Disconnect: ([0-9]+)m\\.([0-9]+)s \\| Name: (.*)$");
        
        public ListPlayersResult Parse(
            string input
        )
        {
            input = input
                .Replace(ActivePlayersHeader, "")
                .Replace(DisconnectedPlayersHeader, "")
                .Replace("\r\n", "\n");
            var lines = input.Split("\n");

            var result = new ListPlayersResult(
                new List<Player>(),
                new List<DisconnectedPlayer>()
            );
            
            foreach (var line in lines)
            {
                var match = ActivePlayerRegex.Match(line);
                if (match.Success)
                {
                    var squadIdStr = match.Groups[5].Value;
                    var squadId = squadIdStr == "N/A" ? (int?)null : int.Parse(squadIdStr);

                    result.ActivePlayers.Add(
                        new Player(
                            int.Parse(match.Groups[1].Value),
                            ulong.Parse(match.Groups[2].Value),
                            match.Groups[3].Value,
                            (TeamId)int.Parse(match.Groups[4].Value),
                            match.Groups[6].Value == "True",
                            match.Groups[7].Value,
                            squadId
                        )
                    );
                    continue;
                }

                match = DisconnectedPlayerRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                result.DisconnectedPlayers.Add(
                    new DisconnectedPlayer(
                        int.Parse(match.Groups[1].Value),
                        ulong.Parse(match.Groups[2].Value),
                        new TimeSpan(0, 0, int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value)),
                        match.Groups[5].Value
                    )
                );
            }

            return result;
        }
    }
}