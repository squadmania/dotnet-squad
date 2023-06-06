namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PendingConnectionDestroyedPayload
    {
        public ulong SteamId64 { get; }
        public string Connection { get; }
        public string Driver { get; }

        public PendingConnectionDestroyedPayload(
            ulong steamId64,
            string connection,
            string driver
        )
        {
            SteamId64 = steamId64;
            Connection = connection;
            Driver = driver;
        }
    }
}