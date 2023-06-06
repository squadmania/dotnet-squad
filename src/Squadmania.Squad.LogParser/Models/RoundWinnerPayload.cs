namespace Squadmania.Squad.LogParser.Models
{
    public sealed class RoundWinnerPayload
    {
        public string Winner { get; }
        public string Layer { get; }

        public RoundWinnerPayload(
            string winner,
            string layer
        )
        {
            Winner = winner;
            Layer = layer;
        }
    }
}