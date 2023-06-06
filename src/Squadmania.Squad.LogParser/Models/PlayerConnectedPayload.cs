namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerConnectedPayload
    {
        public string PlayerSuffix { get; }

        public PlayerConnectedPayload(string playerSuffix)
        {
            PlayerSuffix = playerSuffix;
        }
    }
}