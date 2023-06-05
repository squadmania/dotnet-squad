using System.Collections.Generic;

namespace Squadmania.Squad.Rcon.Models
{
    public sealed class ListPlayersResult
    {
        public ListPlayersResult(
            List<Player> activePlayers,
            List<DisconnectedPlayer> disconnectedPlayers
        )
        {
            ActivePlayers = activePlayers;
            DisconnectedPlayers = disconnectedPlayers;
        }

        public List<Player> ActivePlayers { get; }
        public List<DisconnectedPlayer> DisconnectedPlayers { get; }
    }
}