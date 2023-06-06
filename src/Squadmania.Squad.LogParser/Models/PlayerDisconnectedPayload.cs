namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerDisconnectedPayload
    {
        public PlayerDisconnectedPayload(
            ulong steamId64,
            string playerController
        )
        {
            SteamId64 = steamId64;
            PlayerController = playerController;
        }

        public ulong SteamId64 { get; }
        public string PlayerController { get; }
    }
}