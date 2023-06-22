using System;

namespace Squadmania.Squad.Rcon.Parsers
{
    public readonly struct SquadCreatedMessage
    {
        public string PlayerNameWithoutPrefix { get; }
        public long PlayerSteamId64 { get; }
        public int SquadId { get; }
        public string SquadName { get; }
        public string TeamName { get; }

        public SquadCreatedMessage(
            string playerNameWithoutPrefix,
            long playerSteamId64,
            int squadId,
            string squadName,
            string teamName
        )
        {
            PlayerNameWithoutPrefix = playerNameWithoutPrefix;
            PlayerSteamId64 = playerSteamId64;
            SquadId = squadId;
            SquadName = squadName;
            TeamName = teamName;
        }
    }
}