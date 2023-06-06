namespace Squadmania.Squad.LogParser.Models
{
    public sealed class ServerTickRatePayload
    {
        public float TickRate { get; }

        public ServerTickRatePayload(
            float tickRate
        )
        {
            TickRate = tickRate;
        }
    }
}