namespace Squadmania.Squad.LogParser.Models
{
    public sealed class AdminBroadcastPayload
    {
        public string Message { get; }
        public string From { get; }
        
        public AdminBroadcastPayload(
            string message,
            string from
        )
        {
            Message = message;
            From = from;
        }
    }
}