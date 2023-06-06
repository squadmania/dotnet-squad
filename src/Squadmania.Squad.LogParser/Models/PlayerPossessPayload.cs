namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerPossessPayload
    {
        public string PossessClassname { get; }
        public string Pawn { get; }

        public PlayerPossessPayload(
            string possessClassname,
            string pawn
        )
        {
            PossessClassname = possessClassname;
            Pawn = pawn;
        }
    }
}