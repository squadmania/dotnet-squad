namespace Squadmania.Squad.LogParser.Models
{
    public sealed class ClientConnectedPayload
    {
        public ulong SteamId64 { get; }
        public string Name { get; }
        public string Driver { get; }
        
        public ClientConnectedPayload(
            ulong steamId64,
            string name,
            string driver
        )
        {
            SteamId64 = steamId64;
            Name = name;
            Driver = driver;
        }
    }
}