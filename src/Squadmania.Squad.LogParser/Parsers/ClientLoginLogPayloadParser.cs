using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class ClientLoginPayloadParser : IPayloadParser<ClientLoginPayload>
    {
        private static readonly Regex PayloadRegex = new(@"Login: NewPlayer: SteamNetConnection \/Engine\/Transient\.(SteamNetConnection_[0-9]+)");

        public LogMessageType LogMessageType => LogMessageType.Squad;

        public ClientLoginPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new ClientLoginPayload(
                match.Groups[1].Value
            );
        }

        public object? Parse(
            string value
        )
        {
            return ParseTyped(value);
        }
    }
}