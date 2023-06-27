namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerPossessPayload
    {
        public string PossessClassname { get; }
        public string Pawn { get; }
        public string FullPath { get; set; }

        public PlayerPossessPayload(
            string possessClassname,
            string pawn,
            string fullPath
        )
        {
            PossessClassname = possessClassname;
            Pawn = pawn;
            FullPath = fullPath;
        }
    }
}