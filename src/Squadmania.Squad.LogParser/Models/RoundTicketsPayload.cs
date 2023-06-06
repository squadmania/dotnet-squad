namespace Squadmania.Squad.LogParser.Models
{
    public sealed class RoundTicketsPayload
    {
        public RoundTicketsPayload(
            Team team,
            string subFaction,
            string faction,
            RoundTicketsPayloadAction action,
            int tickets,
            string layer,
            string level
        )
        {
            Team = team;
            SubFaction = subFaction;
            Faction = faction;
            Action = action;
            Tickets = tickets;
            Layer = layer;
            Level = level;
        }

        public Team Team { get; }
        public string SubFaction { get; }
        public string Faction { get; }
        public RoundTicketsPayloadAction Action { get; }
        public int Tickets { get; }
        public string Layer { get; }
        public string Level { get; }
    }
}