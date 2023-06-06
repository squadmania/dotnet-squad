namespace Squadmania.Squad.LogParser.Models
{
    public sealed class ClientLoginPayload
    {
        public ClientLoginPayload(
            string connection
        )
        {
            Connection = connection;
        }

        public string Connection { get; }
    }
}