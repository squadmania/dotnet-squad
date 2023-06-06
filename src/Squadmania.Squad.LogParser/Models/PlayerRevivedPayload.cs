namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerRevivedPayload
    {
        public string ReviverName { get; }
        public string VictimName { get; }
        
        public PlayerRevivedPayload(
            string reviverName,
            string victimName
        )
        {
            ReviverName = reviverName;
            VictimName = victimName;
        } 
    }
}