namespace Squadmania.Squad.Rcon.Models
{
    public readonly struct SquadCreatedMessage
    {
        public string PlayerNameWithoutPrefix { get; }
        public ulong PlayerSteamId64 { get; }
        public int SquadId { get; }
        public string SquadName { get; }
        public string TeamName { get; }

        public SquadCreatedMessage(
            string playerNameWithoutPrefix,
            ulong playerSteamId64,
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