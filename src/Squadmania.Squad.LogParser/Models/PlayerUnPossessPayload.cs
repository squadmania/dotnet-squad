namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerUnPossessPayload
    {
        public string PossessClassname { get; }

        public PlayerUnPossessPayload(string possessClassname)
        {
            PossessClassname = possessClassname;
        }
    }
}